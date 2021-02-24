namespace Rtsp.Sdp
{
   public class Timing
   {
      #region Constructors and destructors

      public Timing(string timing, string repeat)
      {
         // TODO: Complete member initialization
         this.timing = timing;
         this.repeat = repeat;
      }

      #endregion

      #region  Fields

      private string repeat;
      private string timing;

      #endregion
   }
}
