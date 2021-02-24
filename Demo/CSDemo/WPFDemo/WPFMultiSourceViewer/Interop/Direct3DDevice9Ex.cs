using System;
using System.Runtime.InteropServices;

namespace WPFMultiSourceViewer.Interop
{
   internal sealed class Direct3DDevice9Ex : IDisposable
   {
      #region Constructors and destructors

      internal Direct3DDevice9Ex(ComInterface.IDirect3DDevice9Ex obj)
      {
         comObject = obj;
         ComInterface.GetComMethod(comObject, 28, out createRenderTarget);
      }

      ~Direct3DDevice9Ex()
      {
         Release();
      }

      #endregion

      #region  Fields

      private ComInterface.IDirect3DDevice9Ex comObject;
      private ComInterface.CreateRenderTarget createRenderTarget;

      #endregion

      #region Interface methods

      public void Dispose()
      {
         Release();
         GC.SuppressFinalize(this);
      }

      #endregion

      #region Public methods

      public Direct3DSurface9 CreateRenderTarget(uint Width, uint Height, int Format, int MultiSample, uint MultisampleQuality, int Lockable, ref IntPtr pSharedHandle)
      {
         ComInterface.IDirect3DSurface9 obj = null;
         var result = createRenderTarget(comObject, Width, Height, Format, MultiSample, MultisampleQuality, Lockable, out obj, ref pSharedHandle);
         Marshal.ThrowExceptionForHR(result);

         return new Direct3DSurface9(obj);
      }

      #endregion

      #region Private methods

      private void Release()
      {
         if (comObject != null) {
            Marshal.ReleaseComObject(comObject);
            comObject = null;
            createRenderTarget = null;
         }
      }

      #endregion
   }
}
