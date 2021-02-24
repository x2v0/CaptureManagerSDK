namespace Rtsp.Sdp
{
   public class AttributRtpMap : Attribut
   {
      #region Constants

      // Format
      //   rtpmap:<payload type> <encoding name>/<clock rate> [/<encoding parameters>] 
      // Examples
      //   rtpmap:96 H264/90000
      //   rtpmap:8 PCMA/8000

      public const string NAME = "rtpmap";

      #endregion

      #region Constructors and destructors

      #endregion

      #region Public properties

      public string ClockRate
      {
         get;
         set;
      }

      public string EncodingName
      {
         get;
         set;
      }

      public string EncodingParameters
      {
         get;
         set;
      }

      public override string Key => NAME;

      public int PayloadNumber
      {
         get;
         set;
      }

      public override string Value
      {
         get
         {
            if (EncodingParameters.Length == 0) {
               return string.Format("{0} {1}/{2}", PayloadNumber, EncodingName, ClockRate);
            }

            return string.Format("{0} {1}/{2}/{3}", PayloadNumber, EncodingName, ClockRate, EncodingParameters);
         }
         protected set => ParseValue(value);
      }

      #endregion

      #region Protected methods

      protected override void ParseValue(string value)
      {
         var parts = value.Split(' ', '/');

         if (parts.Length >= 1) {
            int tmp_payloadNumber;
            if (int.TryParse(parts[0], out tmp_payloadNumber)) {
               PayloadNumber = tmp_payloadNumber;
            }
         }

         if (parts.Length >= 2) {
            EncodingName = parts[1];
         }

         if (parts.Length >= 3) {
            ClockRate = parts[2];
         }

         if (parts.Length >= 4) {
            EncodingParameters = parts[3];
         }
      }

      #endregion
   }
}
