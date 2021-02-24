namespace Rtsp.Messages
{
   public class RtspRequestAnnounce : RtspRequest
   {
      #region Constructors and destructors

      // constructor

      public RtspRequestAnnounce()
      {
         Command = "ANNOUNCE * RTSP/1.0";
      }

      #endregion
   }
}
