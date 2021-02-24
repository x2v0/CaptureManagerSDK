﻿using System;
using System.Runtime.InteropServices;

namespace WPFMultiSourceViewer.Interop
{
   internal sealed class Direct3DSurface9 : IDisposable
   {
      #region Constructors and destructors

      internal Direct3DSurface9(ComInterface.IDirect3DSurface9 obj)
      {
         texture = obj;
         NativeInterface = Marshal.GetIUnknownForObject(texture);
      }

      ~Direct3DSurface9()
      {
         Release();
      }

      #endregion

      #region  Fields

      #endregion

      #region Public properties

      public IntPtr NativeInterface
      {
         get;
         private set;
      }

      public ComInterface.IDirect3DSurface9 texture
      {
         get;
         private set;
      }

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
         if (texture != null) {
            Marshal.Release(NativeInterface);
            NativeInterface = IntPtr.Zero;

            Marshal.ReleaseComObject(texture);
            texture = null;
         }
      }

      #endregion
   }
}
