using System;
using System.Runtime.InteropServices;

namespace WPFMediaFoundationPlayerHandler.Interop
{
   internal class Direct3DTexture9 : IDisposable
   {
      #region Constructors and destructors

      internal Direct3DTexture9(ComInterface.IDirect3DTexture9 obj, IntPtr sharedhandle)
      {
         comObject = obj;
         NativeInterface = Marshal.GetIUnknownForObject(comObject);
         SharedHandle = sharedhandle;
         ComInterface.GetComMethod(comObject, 13, out getLevelCount);
         ComInterface.GetComMethod(comObject, 18, out getSurfaceLevel);
      }

      ~Direct3DTexture9()
      {
         Release();
      }

      #endregion

      #region  Fields

      private ComInterface.IDirect3DTexture9 comObject;
      private readonly ComInterface.GetLevelCount getLevelCount;
      private readonly ComInterface.GetSurfaceLevel getSurfaceLevel;

      #endregion

      #region Public properties

      public IntPtr NativeInterface
      {
         get;
         private set;
      }

      public IntPtr SharedHandle
      {
         get;
      }

      #endregion

      #region Interface methods

      public void Dispose()
      {
         Release();
         GC.SuppressFinalize(this);
      }

      #endregion

      #region Public methods

      public uint GetLevelCount()
      {
         ComInterface.IDirect3DTexture9 obj = null;
         var result = getLevelCount(comObject);
         return result;
      }

      public Direct3DSurface9 GetSurfaceLevel(uint level)
      {
         ComInterface.IDirect3DSurface9 obj = null;
         var result = getSurfaceLevel(comObject, level, out obj);
         Marshal.ThrowExceptionForHR(result);

         return new Direct3DSurface9(obj, IntPtr.Zero);
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
