namespace Rtsp.Messages
{
   public class RtspRequestTeardown : RtspRequest
   {
      #region Constructors and destructors

      // Constructor
      public RtspRequestTeardown()
      {
         Command = "TEARDOWN * RTSP/1.0";
      }

      #endregion
   }
}
