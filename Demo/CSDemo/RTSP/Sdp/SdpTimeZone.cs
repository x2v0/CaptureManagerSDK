using System;
using System.Diagnostics.Contracts;

namespace Rtsp.Sdp
{
   public class SdpTimeZone
   {
      #region Constructors and destructors

      #endregion

      #region Public methods

      public static SdpTimeZone ParseInvariant(string value)
      {
         if (value == null) {
            throw new ArgumentNullException("value");
         }

         Contract.EndContractBlock();

         var returnValue = new SdpTimeZone();

         throw new NotImplementedException();


         return returnValue;
      }

      #endregion
   }
}
