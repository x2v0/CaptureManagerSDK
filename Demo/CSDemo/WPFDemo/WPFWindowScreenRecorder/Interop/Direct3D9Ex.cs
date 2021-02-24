using System;
using System.Runtime.InteropServices;

namespace WPFWindowScreenRecorder.Interop
{
   internal sealed class Direct3D9Ex : IDisposable
   {
      #region Constructors and destructors

      private Direct3D9Ex(ComInterface.IDirect3D9Ex obj)
      {
         comObject = obj;
         ComInterface.GetComMethod(comObject, 20, out createDeviceEx);
      }

      ~Direct3D9Ex()
      {
         Release();
      }

      #endregion

      #region  Fields

      private ComInterface.IDirect3D9Ex comObject;
      private ComInterface.CreateDeviceEx createDeviceEx;

      #endregion

      #region Interface methods

      public void Dispose()
      {
         Release();
         GC.SuppressFinalize(this);
      }

      #endregion

      #region Public methods

      public static Direct3D9Ex Create(int SDKVersion)
      {
         ComInterface.IDirect3D9Ex obj;
         Marshal.ThrowExceptionForHR(NativeMethods.Direct3DCreate9Ex(SDKVersion, out obj));

         return new Direct3D9Ex(obj);
      }

      public Direct3DDevice9Ex CreateDeviceEx(uint Adapter,
                                              int DeviceType,
                                              IntPtr hFocusWindow,
                                              int BehaviorFlags,
                                              NativeStructs.D3DPRESENT_PARAMETERS pPresentationParameters,
                                              NativeStructs.D3DDISPLAYMODEEX pFullscreenDisplayMode)
      {
         ComInterface.IDirect3DDevice9Ex obj = null;
         var result = createDeviceEx(comObject, Adapter, DeviceType, hFocusWindow, BehaviorFlags, pPresentationParameters, pFullscreenDisplayMode, out obj);
         Marshal.ThrowExceptionForHR(result);

         return new Direct3DDevice9Ex(obj);
      }

      #endregion

      #region Private methods

      private void Release()
      {
         if (comObject != null) {
            Marshal.ReleaseComObject(comObject);
            comObject = null;
            createDeviceEx = null;
         }
      }

      #endregion
   }
}
