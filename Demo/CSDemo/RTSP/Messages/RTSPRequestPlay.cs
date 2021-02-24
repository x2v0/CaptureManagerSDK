namespace Rtsp.Messages
{
   public class RtspRequestPlay : RtspRequest
   {
      #region Constructors and destructors

      // Constructor
      public RtspRequestPlay()
      {
         Command = "PLAY * RTSP/1.0";
      }

      #endregion
   }
}
