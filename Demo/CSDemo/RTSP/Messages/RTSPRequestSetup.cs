using System;

namespace Rtsp.Messages
{
   public class RtspRequestSetup : RtspRequest
   {
      #region Constructors and destructors

      // Constructor
      public RtspRequestSetup()
      {
         Command = "SETUP * RTSP/1.0";
      }

      #endregion

      #region Public methods

      public void AddTransport(RtspTransport newTransport)
      {
         var actualTransport = string.Empty;
         if (Headers.ContainsKey(RtspHeaderNames.Transport)) {
            actualTransport = Headers[RtspHeaderNames.Transport] + ",";
         }

         Headers[RtspHeaderNames.Transport] = actualTransport + newTransport;
      }


      /// <summary>
      ///    Gets the transports associate with the request.
      /// </summary>
      /// <value>The transport.</value>
      public RtspTransport[] GetTransports()
      {
         if (!Headers.ContainsKey(RtspHeaderNames.Transport)) {
            return new[] {
               new RtspTransport()
            };
         }

         var items = Headers[RtspHeaderNames.Transport].Split(',');
         return Array.ConvertAll(items, RtspTransport.Parse);
      }

      #endregion
   }
}
