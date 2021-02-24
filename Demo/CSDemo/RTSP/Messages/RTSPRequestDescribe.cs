namespace Rtsp.Messages
{
   public class RtspRequestDescribe : RtspRequest
   {
      #region Constructors and destructors

      // constructor

      public RtspRequestDescribe()
      {
         Command = "DESCRIBE * RTSP/1.0";
      }

      #endregion
   }
}
