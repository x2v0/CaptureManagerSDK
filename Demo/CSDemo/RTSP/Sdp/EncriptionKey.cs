using System;
using System.Diagnostics.Contracts;

namespace Rtsp.Sdp
{
   public class EncriptionKey
   {
      #region Constructors and destructors

      public EncriptionKey(string p)
      {
      }

      #endregion

      #region Public methods

      public static EncriptionKey ParseInvariant(string value)
      {
         if (value == null) {
            throw new ArgumentNullException("value");
         }

         Contract.EndContractBlock();

         throw new NotImplementedException();
      }

      #endregion
   }
}
