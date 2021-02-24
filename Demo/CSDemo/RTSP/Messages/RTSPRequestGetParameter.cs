namespace Rtsp.Messages
{
   public class RtspRequestGetParameter : RtspRequest
   {
      #region Constructors and destructors

      // Constructor
      public RtspRequestGetParameter()
      {
         Command = "GET_PARAMETER * RTSP/1.0";
      }

      #endregion
   }
}
