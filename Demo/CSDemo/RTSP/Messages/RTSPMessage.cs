using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Rtsp.Messages
{
   public class RtspMessage : RtspChunk
   {
      #region Static fields

      //private static NLog.Logger //_logger = NLog.LogManager.GetCurrentClassLogger();

      /// <summary>
      ///    The regex to validate the Rtsp message.
      /// </summary>
      private static readonly Regex _rtspVersionTest = new Regex(@"^RTSP/\d\.\d", RegexOptions.Compiled);

      #endregion

      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="RtspMessage" /> class.
      /// </summary>
      public RtspMessage()
      {
         Data = new byte[0];
         Creation = DateTime.Now;
      }

      #endregion

      #region  Fields

      protected internal string[] commandArray;

      #endregion

      #region Public properties

      /// <summary>
      ///    Gets or sets the command of the message (first line).
      /// </summary>
      /// <value>The command.</value>
      public string Command
      {
         get
         {
            if (commandArray == null) {
               return string.Empty;
            }

            return string.Join(" ", commandArray);
         }
         set
         {
            if (value == null) {
               commandArray = new[] {
                  string.Empty
               };
            } else {
               commandArray = value.Split(new[] {
                  ' '
               }, 3);
            }
         }
      }

      /// <summary>
      ///    Gets or sets the creation time.
      /// </summary>
      /// <value>The creation time.</value>
      public DateTime Creation
      {
         get;
      }

      /// <summary>
      ///    Gets or sets the Ccommande Seqquence number.
      ///    <remarks>If the header is not define or not a valid number it return 0</remarks>
      /// </summary>
      /// <value>The sequence number.</value>
      public int CSeq
      {
         get
         {
            string returnStringValue;
            int returnValue;
            if (!(Headers.TryGetValue("CSeq", out returnStringValue) && int.TryParse(returnStringValue, out returnValue))) {
               returnValue = 0;
            }

            return returnValue;
         }
         set => Headers["CSeq"] = value.ToString(CultureInfo.InvariantCulture);
      }


      /// <summary>
      ///    Gets the headers of the message.
      /// </summary>
      /// <value>The headers.</value>
      public Dictionary<string, string> Headers
      {
         get;
      } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


      /// <summary>
      ///    Gets the Method of the message (eg OPTIONS, DESCRIBE, SETUP, PLAY).
      /// </summary>
      /// <value>The Method</value>
      public string Method
      {
         get
         {
            if (commandArray == null) {
               return string.Empty;
            }

            return commandArray[0];
         }
      }

      /// <summary>
      ///    Gets the session ID.
      /// </summary>
      /// <value>The session ID.</value>
      public virtual string Session
      {
         get
         {
            if (!Headers.ContainsKey("Session")) {
               return null;
            }

            return Headers["Session"];
         }
         set => Headers["Session"] = value;
      }

      #endregion

      #region Public methods

      /// <summary>
      ///    Create the good type of Rtsp Message from the header.
      /// </summary>
      /// <param name="aRequestLine">A request line.</param>
      /// <returns>An Rtsp message</returns>
      public static RtspMessage GetRtspMessage(string aRequestLine)
      {
         // We can't determine the message 
         if (string.IsNullOrEmpty(aRequestLine)) {
            return new RtspMessage();
         }

         var requestParts = aRequestLine.Split(new[] {
            ' '
         }, 3);
         RtspMessage returnValue;
         if (requestParts.Length == 3) {
            // A request is : Method SP Request-URI SP RTSP-Version
            // A response is : RTSP-Version SP Status-Code SP Reason-Phrase
            // RTSP-Version = "RTSP" "/" 1*DIGIT "." 1*DIGIT
            if (_rtspVersionTest.IsMatch(requestParts[2])) {
               returnValue = RtspRequest.GetRtspRequest(requestParts);
            } else if (_rtspVersionTest.IsMatch(requestParts[0])) {
               returnValue = new RtspResponse();
            } else {
               //_logger.Warn(CultureInfo.InvariantCulture, "Got a strange message {0}", aRequestLine);
               returnValue = new RtspMessage();
            }
         } else {
            //_logger.Warn(CultureInfo.InvariantCulture, "Got a strange message {0}", aRequestLine);
            returnValue = new RtspMessage();
         }

         returnValue.Command = aRequestLine;
         return returnValue;
      }

      /// <summary>
      ///    Adds one header from a string.
      /// </summary>
      /// <param name="line">The string containing header of format Header: Value.</param>
      /// <exception cref="ArgumentNullException"><paramref name="line" /> is null</exception>
      public void AddHeader(string line)
      {
         if (line == null) {
            throw new ArgumentNullException("line");
         }

         //spliter
         var elements = line.Split(new[] {
            ':'
         }, 2);
         if (elements.Length == 2) {
            Headers[elements[0].Trim()] = elements[1].TrimStart();
         }
      }

      /// <summary>
      ///    Adjusts the content length header.
      /// </summary>
      public void AdjustContentLength()
      {
         if (Data.Length > 0) {
            Headers["Content-Length"] = Data.Length.ToString(CultureInfo.InvariantCulture);
         } else {
            Headers.Remove("Content-Length");
         }
      }


      /// <summary>
      /// Logs the message.
      /// </summary>
      /// <param name="aLevel">A log level.</param>
      //public override void LogMessage(NLog.LogLevel aLevel)
      //{
      //    // Default value to debug
      //    if (aLevel == null)
      //        aLevel = NLog.LogLevel.Debug;
      //    // if the level is not logged directly return
      //    if (!//_logger.IsEnabled(aLevel))
      //        return;

      //    //_logger.Log(aLevel, "Commande : {0}", Command);
      //    foreach (KeyValuePair<string, string> item in _headers)
      //    {
      //        //_logger.Log(aLevel, "Header : {0}: {1}", item.Key, item.Value);
      //    }

      //    if (Data.Length > 0)
      //    {
      //        //_logger.Log(aLevel, "Data :-{0}-", ASCIIEncoding.ASCII.GetString(Data));
      //    }
      //}

      /// <summary>
      ///    Crée un nouvel objet qui est une copie de l'instance en cours.
      /// </summary>
      /// <returns>
      ///    Nouvel objet qui est une copie de cette instance.
      /// </returns>
      public override object Clone()
      {
         var returnValue = GetRtspMessage(Command);

         foreach (var item in Headers) {
            if (item.Value == null) {
               returnValue.Headers.Add(item.Key.Clone() as string, null);
            } else {
               returnValue.Headers.Add(item.Key.Clone() as string, item.Value.Clone() as string);
            }
         }

         returnValue.Data = Data.Clone() as byte[];
         returnValue.SourcePort = SourcePort;

         return returnValue;
      }

      /// <summary>
      ///    Initialises the length of the data byte array from content lenth header.
      /// </summary>
      public void InitialiseDataFromContentLength()
      {
         int dataLength;
         if (!(Headers.ContainsKey("Content-Length") && int.TryParse(Headers["Content-Length"], out dataLength))) {
            dataLength = 0;
         }

         Data = new byte[dataLength];
      }

      /// <summary>
      ///    Sends to the message to a stream.
      /// </summary>
      /// <param name="stream">The stream.</param>
      /// <exception cref="ArgumentNullException"><paramref name="stream" /> is empty</exception>
      /// <exception cref="ArgumentException"><paramref name="stream" /> can't be written.</exception>
      public void SendTo(Stream stream)
      {
         // <pex>
         if (stream == null) {
            throw new ArgumentNullException("stream");
         }

         if (!stream.CanWrite) {
            throw new ArgumentException("Stream CanWrite == false, can't send message to it", "stream");
         }

         // </pex>
         Contract.EndContractBlock();

         var encoder = Encoding.UTF8;
         var outputString = new StringBuilder();

         AdjustContentLength();

         // output header
         outputString.Append(Command);
         outputString.Append("\r\n");
         foreach (var item in Headers) {
            outputString.AppendFormat("{0}: {1}\r\n", item.Key, item.Value);
         }

         outputString.Append("\r\n");
         var buffer = encoder.GetBytes(outputString.ToString());
         lock (stream) {
            stream.Write(buffer, 0, buffer.Length);

            // Output data
            if (Data.Length > 0) {
               stream.Write(Data, 0, Data.Length);
            }
         }

         stream.Flush();
      }

      #endregion
   }
}
