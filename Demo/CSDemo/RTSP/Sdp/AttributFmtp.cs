namespace Rtsp.Sdp
{
   public class AttributFmtp : Attribut
   {
      #region Constants

      public const string NAME = "fmtp";

      #endregion

      #region Constructors and destructors

      #endregion

      #region Public properties

      // temporary aatibute to store remaning data not parsed
      public string FormatParameter
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
         get => string.Format("{0} {1}", PayloadNumber, FormatParameter);
         protected set => ParseValue(value);
      }

      #endregion

      #region Protected methods

      protected override void ParseValue(string value)
      {
         var parts = value.Split(new[] {
            ' '
         }, 2);

         int payloadNumber;
         if (int.TryParse(parts[0], out payloadNumber)) {
            PayloadNumber = payloadNumber;
         }

         if (parts.Length > 1) {
            FormatParameter = parts[1];
         }
      }

      #endregion
   }
}
