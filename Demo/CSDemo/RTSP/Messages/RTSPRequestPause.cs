namespace Rtsp.Messages
{
   public class RtspRequestPause : RtspRequest
   {
      #region Constructors and destructors

      // Constructor
      public RtspRequestPause()
      {
         Command = "PAUSE * RTSP/1.0";
      }

      #endregion
   }
}
