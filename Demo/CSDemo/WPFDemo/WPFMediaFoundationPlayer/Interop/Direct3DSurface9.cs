using System;
using System.Runtime.InteropServices;

namespace WPFMediaFoundationPlayer.Interop
{
   public sealed class Direct3DSurface9 : IDisposable
   {
      #region Constructors and destructors

      internal Direct3DSurface9(ComInterface.IDirect3DSurface9 obj)
      {
         comObject = obj;
         NativeInterface = Marshal.GetIUnknownForObject(comObject);
      }

      ~Direct3DSurface9()
      {
         Release();
      }

      #endregion

      #region  Fields

      private ComInterface.IDirect3DSurface9 comObject;

      #endregion

      #region Public properties

      public IntPtr NativeInterface
      {
         get;
         private set;
      }

      public object texture => comObject;

      #endregion

      #region Interface methods

      public void Dispose()
      {
         Release();
         GC.SuppressFinalize(this);
      }

      #endregion

      #region Private methods

      private void Release()
      {
         if (comObject != null) {
            Marshal.Release(NativeInterface);
            NativeInterface = IntPtr.Zero;

            Marshal.ReleaseComObject(comObject);
            comObject = null;
         }
      }

      #endregion
   }
}
