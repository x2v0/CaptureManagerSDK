namespace Rtsp.Messages
{
   public class RtspRequestRecord : RtspRequest
   {
      #region Constructors and destructors

      public RtspRequestRecord()
      {
         Command = "RECORD * RTSP/1.0";
      }

      #endregion
   }
}
