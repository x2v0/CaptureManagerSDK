namespace Rtsp.Messages
{
   public class RtspRequestOptions : RtspRequest
   {
      #region Constructors and destructors

      // Constructor
      public RtspRequestOptions()
      {
         Command = "OPTIONS * RTSP/1.0";
      }

      #endregion

      #region Public methods

      /// <summary>
      ///    Gets the assiociate OK response with the request.
      /// </summary>
      /// <returns>
      ///    an Rtsp response corresponding to request.
      /// </returns>
      public override RtspResponse CreateResponse()
      {
         var response = base.CreateResponse();
         // Add genric suported operations.
         response.Headers.Add(RtspHeaderNames.Public, "OPTIONS,DESCRIBE,ANNOUNCE,SETUP,PLAY,PAUSE,TEARDOWN,GET_PARAMETER,SET_PARAMETER,REDIRECT");

         return response;
      }

      #endregion
   }
}
