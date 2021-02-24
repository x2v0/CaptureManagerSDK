﻿using System;
using System.Runtime.InteropServices;

namespace WPFPauseResumeRecording.Interop
{
   // These are the declarations taken from the DirectX SDK
   internal static class ComInterface
   {
      #region  Delegates

      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      public delegate int CreateDeviceEx(IDirect3D9Ex d3D9,
                                         uint Adapter,
                                         int DeviceType,
                                         IntPtr hFocusWindow,
                                         int BehaviorFlags,
                                         NativeStructs.D3DPRESENT_PARAMETERS pPresentationParameters,
                                         NativeStructs.D3DDISPLAYMODEEX pFullscreenDisplayMode,
                                         out IDirect3DDevice9Ex ppReturnedDeviceInterface);

      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      public delegate int CreateRenderTarget(IDirect3DDevice9Ex device,
                                             uint Width,
                                             uint Height,
                                             int Format,
                                             int MultiSample,
                                             uint MultisampleQuality,
                                             int Lockable,
                                             out IDirect3DSurface9 ppSurface,
                                             ref IntPtr pSharedHandle);

      #endregion

      #region Interfaces

      [ComImport] [Guid("02177241-69FC-400C-8FF1-93A44DF6861D")] [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
      public interface IDirect3D9Ex
      {
      }

      [ComImport] [Guid("B18B10CE-2649-405a-870F-95F777D4313A")] [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
      public interface IDirect3DDevice9Ex
      {
      }

      [ComImport] [Guid("0CFBAF3A-9FF6-429a-99B3-A2796AF8B89B")] [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
      public interface IDirect3DSurface9
      {
      }

      #endregion

      #region Public methods

      // This is a helper method that accesses the COM objects v-table and
      // turns it into a delegate.
      public static bool GetComMethod<T, U>(T comObj, int slot, out U method) where U : class
      {
         var objectAddress = Marshal.GetComInterfaceForObject(comObj, typeof(T));
         if (objectAddress == IntPtr.Zero) {
            method = null;
            return false;
         }

         try {
            var vTable = Marshal.ReadIntPtr(objectAddress, 0);
            var methodAddress = Marshal.ReadIntPtr(vTable, slot * IntPtr.Size);

            // We can't have a Delegate constraint, so we have to cast to
            // object then to our desired delegate
            method = (U) (object) Marshal.GetDelegateForFunctionPointer(methodAddress, typeof(U));
            return true;
         } finally {
            Marshal.Release(objectAddress); // Prevent memory leak
         }
      }

      #endregion
   }
}
