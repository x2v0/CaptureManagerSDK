using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Rtsp;
using Rtsp.Messages;

// RTSP Server Example by Roger Hardiman, 2016
// Re-uses some code from the Multiplexer example of SharpRTSP
//
// This example simulates a live RTSP video stream, for example a CCTV Camera
// It creates a Video Source (a test card) that creates a YUV Image
// The image is then encoded as H264 data
// The H264 data is sent to the RTSP clients

// The Tiny H264 Encoder is a 100% .NET encoder which is lossless and creates large bitstreams as
// there is no compression. It is limited to 128x96 resolution. However it makes it easy to write a quick
// demo without needing native APIs or cross compiled C libraries for H264


public class RtspServer : IDisposable
{
   #region Constants

   private const uint global_ssrc = 0x4321FADE; // 8 hex digits

   #endregion

   #region Constructors and destructors

   /// <summary>
   ///    Initializes a new instance of the <see cref="RTSPServer" /> class.
   /// </summary>
   /// <param name="aPortNumber">A numero port.</param>
   /// <param name="username">username.</param>
   /// <param name="password">password.</param>
   public RtspServer(List<Tuple<StreamType, int, string>> aStreams, int portNumber, string username, string password)
   {
      if ((portNumber < IPEndPoint.MinPort) ||
          (portNumber > IPEndPoint.MaxPort)) {
         throw new ArgumentOutOfRangeException("aPortNumber", portNumber, "Port number must be between System.Net.IPEndPoint.MinPort and System.Net.IPEndPoint.MaxPort");
      }

      Contract.EndContractBlock();

      if ((string.IsNullOrEmpty(username) == false) &&
          (string.IsNullOrEmpty(password) == false)) {
         var realm = "WPFRtspServer";
         auth = new Authentication(username, password, realm, Authentication.Type.Digest);
      } else {
         auth = null;
      }

      mStreams = aStreams;

      RtspUtils.RegisterUri();
      _RTSPServerListener = new TcpListener(IPAddress.Any, portNumber);
   }

   #endregion

   #region  Fields

   private Thread _ListenTread;

   private readonly TcpListener _RTSPServerListener;
   private ManualResetEvent _Stopping;

   private readonly Authentication auth;

   private readonly List<Tuple<StreamType, int, string>> mStreams;


   private readonly Random rnd = new Random();

   private readonly List<RTPSession> rtp_list = new List<RTPSession>(); // list of RTSP Listeners, used when sending RTP over RTSP
   private int session_count;

   #endregion

   #region Enums

   public enum StreamType
   {
      None,
      Video = 97,
      Audio = 96
   }

   #endregion

   #region Interface methods

   public void Dispose()
   {
      Dispose(true);
      GC.SuppressFinalize(this);
   }

   #endregion

   #region Public methods

   public void sendData(int trackID, int rtp_payload_type, uint ts, byte[] raw_nal)
   {
      // Check if there are any clients. Only run the encoding if someone is connected
      // Could exand this to check if someone is connected and in PLAY mode
      var current_rtp_count = rtp_list.Count;

      if (current_rtp_count == 0) {
         return;
      }

      // Take the YUV image and encode it into a H264 NAL
      // This returns a NAL with no headers (no 00 00 00 01 header and no 32 bit sizes)
      //Console.WriteLine("Compressing video at time(ms) " + timestamp_ms + "    " + current_rtp_count + " RTSP clients connected");


      //UInt32 ts = timestamp_ms * 90; // 90kHz clock

      // The H264 Payload could be sent as one large RTP packet (assuming the receiver can handle it)
      // or as a Fragmented Data, split over several RTP packets with the same Timestamp.
      var fragmenting = false;
      if (raw_nal.Length > 1400) {
         fragmenting = true;
      }

      // Build a list of 1 or more RTP packets
      var rtp_packets = new List<byte[]>();

      if (fragmenting == false) {
         // Put the whole NAL into one RTP packet.
         // Note some receivers will have maximum buffers and be unable to handle large RTP packets.
         // Also with RTP over RTSP there is a limit of 65535 bytes for the RTP packet.

         var rtp_packet = new byte[12 + raw_nal.Length]; // 12 is header size when there are no CSRCs or extensions
         // Create an single RTP fragment

         // RTP Packet Header
         // 0 - Version, P, X, CC, M, PT and Sequence Number
         //32 - Timestamp. H264 uses a 90kHz clock
         //64 - SSRC
         //96 - CSRCs (optional)
         //nn - Extension ID and Length
         //nn - Extension header

         var rtp_version = 2;
         var rtp_padding = 0;
         var rtp_extension = 0;
         var rtp_csrc_count = 0;
         var rtp_marker = 1;

         RTPPacketUtil.WriteHeader(rtp_packet, rtp_version, rtp_padding, rtp_extension, rtp_csrc_count, rtp_marker, rtp_payload_type);

         uint empty_sequence_id = 0;
         RTPPacketUtil.WriteSequenceNumber(rtp_packet, empty_sequence_id);

         //Console.WriteLine("adjusted TS at 90khz=" + ts);
         RTPPacketUtil.WriteTS(rtp_packet, ts);

         uint empty_ssrc = 0;
         RTPPacketUtil.WriteSSRC(rtp_packet, empty_ssrc);

         // Now append the raw NAL
         Array.Copy(raw_nal, 0, rtp_packet, 12, raw_nal.Length);

         rtp_packets.Add(rtp_packet);
      } else {
         var data_remaining = raw_nal.Length;
         var nal_pointer = 0;
         var start_bit = 1;
         var end_bit = 0;

         // consume first byte of the raw_nal. It is used in the FU header
         var first_byte = raw_nal[0];
         nal_pointer++;
         data_remaining--;

         while (data_remaining > 0) {
            var payload_size = Math.Min(1400, data_remaining);
            if ((data_remaining - payload_size) == 0) {
               end_bit = 1;
            }

            var rtp_packet = new byte[12 + 2 + payload_size]; // 12 is header size. 2 bytes for FU-A header. Then payload

            // RTP Packet Header
            // 0 - Version, P, X, CC, M, PT and Sequence Number
            //32 - Timestamp. H264 uses a 90kHz clock
            //64 - SSRC
            //96 - CSRCs (optional)
            //nn - Extension ID and Length
            //nn - Extension header

            var rtp_version = 2;
            var rtp_padding = 0;
            var rtp_extension = 0;
            var rtp_csrc_count = 0;
            var rtp_marker = end_bit == 1 ? 1 : 0; // Marker set to 1 on last packet


            RTPPacketUtil.WriteHeader(rtp_packet, rtp_version, rtp_padding, rtp_extension, rtp_csrc_count, rtp_marker, rtp_payload_type);

            uint empty_sequence_id = 0;
            RTPPacketUtil.WriteSequenceNumber(rtp_packet, empty_sequence_id);

            Console.WriteLine("adjusted TS at 90khz=" + ts);
            RTPPacketUtil.WriteTS(rtp_packet, ts);

            uint empty_ssrc = 0;
            RTPPacketUtil.WriteSSRC(rtp_packet, empty_ssrc);

            // Now append the Fragmentation Header (with Start and End marker) and part of the raw_nal
            byte f_bit = 0;
            var nri = (byte) ((first_byte >> 5) & 0x03); // Part of the 1st byte of the Raw NAL (NAL Reference ID)
            byte type = 28; // FU-A Fragmentation

            rtp_packet[12] = (byte) ((f_bit << 7) + (nri << 5) + type);
            rtp_packet[13] = (byte) ((start_bit << 7) + (end_bit << 6) + (0 << 5) + (first_byte & 0x1F));

            Array.Copy(raw_nal, nal_pointer, rtp_packet, 14, payload_size);
            nal_pointer = nal_pointer + payload_size;
            data_remaining = data_remaining - payload_size;

            rtp_packets.Add(rtp_packet);

            start_bit = 0;
         }
      }

      lock (rtp_list) {
         // Go through each RTSP session and output the NAL
         foreach (var session in rtp_list.ToArray()) // ToArray makes a temp copy of the list.
            // This lets us delete items in the foreach
         {
            // Only process Sessions in Play Mode
            if (session.play == false) {
               continue;
            }

            if (session.trackID != trackID) {
               continue;
            }

            // There could be more than 1 RTP packet (if the data is fragmented)
            var write_error = false;
            foreach (var rtp_packet in rtp_packets) {
               // Add the specific data for each transmission
               RTPPacketUtil.WriteSequenceNumber(rtp_packet, session.sequence_number);
               session.sequence_number++;

               // Add the specific SSRC for each transmission
               RTPPacketUtil.WriteSSRC(rtp_packet, session.ssrc);

               //Console.WriteLine("trackID=" + trackID);

               // Send as RTP over RTSP
               if (session.transport_reply.LowerTransport == RtspTransport.LowerTransportType.TCP) {
                  var video_channel = session.transport_reply.Interleaved.First; // second is for RTCP status messages)
                  var state = new object();
                  try {
                     // send the whole NAL. With RTP over RTSP we do not need to Fragment the NAL (as we do with UDP packets or Multicast)
                     //session.listener.BeginSendData(video_channel, rtp_packet, new AsyncCallback(session.listener.EndSendData), state);
                     session.listener.SendData(video_channel, rtp_packet);
                  } catch {
                     Console.WriteLine("Error writing to listener " + session.listener.RemoteAdress);
                     write_error = true;
                     break; // exit out of foreach loop
                  }
               }

               // TODO. Add UDP and Multicast
            }

            if (write_error) {
               session.play = false; // stop sending data
               session.listener.Dispose();
               rtp_list.Remove(session); // remove the session. It is dead
               Console.WriteLine(rtp_list.Count + " remaining sessions open");
            }
         }
      }
   }

   /// <summary>
   ///    Starts the listen.
   /// </summary>
   public void StartListen()
   {
      _RTSPServerListener.Start();

      _Stopping = new ManualResetEvent(false);
      _ListenTread = new Thread(AcceptConnection);
      _ListenTread.Start();
   }


   public void StopListen()
   {
      _RTSPServerListener.Stop();
      _Stopping.Set();
      _ListenTread.Join();
   }

   #endregion

   #region Protected methods

   protected virtual void Dispose(bool disposing)
   {
      if (disposing) {
         StopListen();
         _Stopping.Dispose();
      }
   }

   #endregion

   #region Private methods

   /// <summary>
   ///    Accepts the connection.
   /// </summary>
   private void AcceptConnection()
   {
      try {
         while (!_Stopping.WaitOne(0)) {
            var oneClient = _RTSPServerListener.AcceptTcpClient();
            Console.WriteLine("Connection from " + oneClient.Client.RemoteEndPoint);

            var rtsp_socket = new RtspTcpTransport(oneClient);
            var newListener = new RtspListener(rtsp_socket);
            newListener.MessageReceived += RTSP_Message_Received;
            //RTSPDispatcher.Instance.AddListener(newListener);
            newListener.Start();
         }
      } catch (SocketException error) {
         // _logger.Warn("Got an error listening, I have to handle the stopping which also throw an error", error);
      }
   }

   // Process each RTSP message that is received
   private void RTSP_Message_Received(object sender, RtspChunkEventArgs e)
   {
      // Cast the 'sender' and 'e' into the RTSP Listener (the Socket) and the RTSP Message
      var listener = sender as RtspListener;
      var message = e.Message as RtspMessage;

      Console.WriteLine("RTSP message received " + message);


      // Check if the RTSP Message has valid authentication (validating against username,password,realm and nonce)
      if (auth != null) {
         var authorized = false;
         if (message.Headers.ContainsKey("Authorization")) {
            // Check the message has the correct Authorization
            authorized = auth.IsValid(message);
         }

         if ((message.Headers.ContainsKey("Authorization") == false) ||
             (authorized == false)) {
            // Send a 401 Authentication Required reply
            var authorization_response = (e.Message as RtspRequest).CreateResponse();
            authorization_response.AddHeader("WWW-Authenticate: " + auth.GetHeader());
            authorization_response.ReturnCode = 401;
            listener.SendMessage(authorization_response);
            return;
         }
      }

      // Handle OPTIONS message
      if (message is RtspRequestOptions) {
         // Create the reponse to OPTIONS
         var options_response = (e.Message as RtspRequestOptions).CreateResponse();
         listener.SendMessage(options_response);
      }

      // Handle DESCRIBE message
      if (message is RtspRequestDescribe) {
         var requested_url = (message as RtspRequestDescribe).RtspUri.ToString();
         Console.WriteLine("Request for " + requested_url);

         // TODO. Check the requsted_url is valid. In this example we accept any RTSP URL

         //// Make the Base64 SPS and PPS
         //byte[] raw_sps = h264_encoder.GetRawSPS(); // no 0x00 0x00 0x00 0x01 or 32 bit size header
         //byte[] raw_pps = h264_encoder.GetRawPPS(); // no 0x00 0x00 0x00 0x01 or 32 bit size header
         //String sps_str = Convert.ToBase64String(raw_sps);
         //String pps_str = Convert.ToBase64String(raw_pps);

         var sdp = new StringBuilder();

         // Generate the SDP
         // The sprop-parameter-sets provide the SPS and PPS for H264 video
         // The packetization-mode defines the H264 over RTP payloads used but is Optional
         sdp.Append("v=0\n");
         sdp.Append("o=user 123 0 IN IP4 0.0.0.0\n");
         sdp.Append("s=WPFRtspServer\n");
         sdp.Append("c=IN IP4 0.0.0.0\n");

         if (mStreams != null) {
            foreach (var item in mStreams) {
               switch (item.Item1) {
                  case StreamType.Video:
                  {
                     sdp.Append(string.Format("m=video 0 RTP/AVP {0}\n", (uint) item.Item1));
                     sdp.Append(string.Format("a=rtpmap:{0} {1}/90000\n", (uint) item.Item1, item.Item3));
                     sdp.Append(string.Format("a=fmtp:{0}\n", (uint) item.Item1));
                     sdp.Append(string.Format("a=control:trackID={0}\n", item.Item2));
                     //sdp.Append("a=fmtp:96 profile-level-id=42A01E; sprop-parameter-sets=" + sps_str + "," + pps_str + ";\n");
                  }
                     break;
                  case StreamType.Audio:
                  {
                     sdp.Append(string.Format("m=audio 0 RTP/AVP {0}\n", (uint) item.Item1));
                     sdp.Append(string.Format("a=rtpmap:{0} mpeg4-generic/90000/2\n", (uint) item.Item1));
                     sdp.Append(string.Format("a=fmtp:{0}\n", (uint) item.Item1));
                     sdp.Append(string.Format("a=control:trackID={0}\n", item.Item2));
                     //sdp.Append("a=fmtp:96 profile-level-id=42A01E; sprop-parameter-sets=" + sps_str + "," + pps_str + ";\n");
                  }
                     break;
                  case StreamType.None:
                  default:
                     break;
               }
            }
         }

         var sdp_bytes = Encoding.ASCII.GetBytes(sdp.ToString());

         // Create the reponse to DESCRIBE
         // This must include the Session Description Protocol (SDP)
         var describe_response = (e.Message as RtspRequestDescribe).CreateResponse();

         describe_response.AddHeader("Content-Base: " + requested_url);
         describe_response.AddHeader("Content-Type: application/sdp");
         describe_response.Data = sdp_bytes;
         describe_response.AdjustContentLength();
         listener.SendMessage(describe_response);
      }

      // Handle SETUP message
      if (message is RtspRequestSetup) {
         // 
         var setupMessage = message as RtspRequestSetup;

         // Check the RTSP transport
         // If it is UDP or Multicast, create the sockets
         // If it is RTP over RTSP we send data via the RTSP Listener

         // FIXME client may send more than one possible transport.
         // very rare
         var transport = setupMessage.GetTransports()[0];


         // Construct the Transport: reply from the Server to the client
         var transport_reply = new RtspTransport();
         transport_reply.SSrc = global_ssrc.ToString("X8"); // Convert to Hex, padded to 8 characters

         if (transport.LowerTransport == RtspTransport.LowerTransportType.TCP) {
            // RTP over RTSP mode}
            transport_reply.LowerTransport = RtspTransport.LowerTransportType.TCP;
            transport_reply.Interleaved = new PortCouple(transport.Interleaved.First, transport.Interleaved.Second);
         }

         if ((transport.LowerTransport == RtspTransport.LowerTransportType.UDP) &&
             (transport.IsMulticast == false)) {
            // RTP over UDP mode}
            // Create a pair of UDP sockets
            // Pass the Port of the two sockets back in the reply
            transport_reply.LowerTransport = RtspTransport.LowerTransportType.UDP;
            transport_reply.IsMulticast = false;
            transport_reply.ClientPort = transport.ClientPort; // FIX
            // for now until implemented
            transport_reply = null;
         }

         if ((transport.LowerTransport == RtspTransport.LowerTransportType.UDP) &&
             transport.IsMulticast) {
            // RTP over Multicast UDP mode}
            // Create a pair of UDP sockets in Multicast Mode
            // Pass the Ports of the two sockets back in the reply
            transport_reply.LowerTransport = RtspTransport.LowerTransportType.UDP;
            transport_reply.IsMulticast = true;
            transport_reply.Port = new PortCouple(7000, 7001); // FIX

            // for now until implemented
            transport_reply = null;
         }


         if (transport_reply != null) {
            var new_session = new RTPSession();
            new_session.listener = listener;
            new_session.sequence_number = (ushort) rnd.Next(65535); // start with a random 16 bit sequence number
            new_session.ssrc = global_ssrc;

            // Add the transports to the Session
            new_session.client_transport = transport;
            new_session.transport_reply = transport_reply;

            lock (rtp_list) {
               if ((setupMessage.RtspUri.Segments != null) &&
                   (setupMessage.RtspUri.Segments.Length == 2)) {
                  var split = setupMessage.RtspUri.Segments[1].Split('=');

                  if ((split != null) &&
                      (split.Length == 2)) {
                     int.TryParse(split[1], out new_session.trackID);
                  }
               }


               //setupMessage.SourcePort.RemoteAdress

               // Create a 'Session' and add it to the Session List
               // ToDo - Check the Track ID. In the SDP the H264 video track is TrackID 0
               // Place Lock() here so the Session Count and the addition to the list is locked
               new_session.session_id = session_count.ToString();

               // Add the new session to the Sessions List
               rtp_list.Add(new_session);
               session_count++;
            }


            var setup_response = setupMessage.CreateResponse();
            setup_response.Headers[RtspHeaderNames.Transport] = transport_reply.ToString();
            setup_response.Session = new_session.session_id;
            listener.SendMessage(setup_response);
         } else {
            var setup_response = setupMessage.CreateResponse();
            // unsuported transport
            setup_response.ReturnCode = 461;
            listener.SendMessage(setup_response);
         }
      }

      // Handle PLAY message
      if (message is RtspRequestPlay) {
         lock (rtp_list) {
            // Search for the Session in the Sessions List. Change the state of "PLAY"
            var session_found = false;
            foreach (var session in rtp_list) {
               //if (session.session_id.Equals(message.Session))
               {
                  // found the session
                  session_found = true;
                  session.play = true;

                  var range = "npt=0-"; // Playing the 'video' from 0 seconds until the end
                  var rtp_info = "url=" + ((RtspRequestPlay) message).RtspUri + ";seq=" + session.sequence_number; // TODO Add rtptime  +";rtptime="+session.rtp_initial_timestamp;

                  // Send the reply
                  var play_response = (e.Message as RtspRequestPlay).CreateResponse();
                  play_response.AddHeader("Range: " + range);
                  play_response.AddHeader("RTP-Info: " + rtp_info);
                  listener.SendMessage(play_response);
               }
            }

            if (session_found == false) {
               // Session ID was not found in the list of Sessions. Send a 454 error
               var play_failed_response = (e.Message as RtspRequestPlay).CreateResponse();
               play_failed_response.ReturnCode = 454; // Session Not Found
               listener.SendMessage(play_failed_response);
            }
         }
      }

      // Handle PLAUSE message
      if (message is RtspRequestPause) {
         lock (rtp_list) {
            // Search for the Session in the Sessions List. Change the state of "PLAY" 
            foreach (var session in rtp_list) {
               if (session.session_id.Equals(message.Session)) {
                  // found the session
                  session.play = false;
                  break;
               }
            }
         }

         // ToDo - only send back the OK response if the Session in the RTSP message was found
         var pause_response = (e.Message as RtspRequestPause).CreateResponse();
         listener.SendMessage(pause_response);
      }


      // Handle GET_PARAMETER message, often used as a Keep Alive
      if (message is RtspRequestGetParameter) {
         // Create the reponse to GET_PARAMETER
         var getparameter_response = (e.Message as RtspRequestGetParameter).CreateResponse();
         listener.SendMessage(getparameter_response);
      }


      // Handle TEARDOWN
      if (message is RtspRequestTeardown) {
         lock (rtp_list) {
            // Search for the Session in the Sessions List.
            foreach (var session in rtp_list.ToArray()) // Convert to ToArray so we can delete from the rtp_list
            {
               if (session.session_id.Equals(message.Session)) {
                  // TODO - Close UDP or Multicast transport
                  // For TCP there is no transport to close
                  rtp_list.Remove(session);
                  // Close the RTSP socket
                  listener.Dispose();
               }
            }
         }
      }
   }

   #endregion

   #region Nested classes

   public class RTPSession
   {
      #region  Fields

      public RtspTransport client_transport; // Transport: string from the client to the server
      public RtspListener listener; // The RTSP client connection
      public bool play; // set to true when Session is in Play mode
      public ushort sequence_number = 1; // 16 bit RTP packet sequence number used with this client connection
      public string session_id = ""; // RTSP Session ID used with this client connection
      public uint ssrc = 0x12345678; // SSRC value used with this client connection
      public int trackID = -1;
      public RtspTransport transport_reply; // Transport: reply from the server to the client

      #endregion
   }

   #endregion
}
