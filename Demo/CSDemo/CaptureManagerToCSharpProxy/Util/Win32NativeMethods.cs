﻿/*
MIT License

Copyright(c) 2020 Evgeny Pereguda

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions :

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DISPPARAMS = System.Runtime.InteropServices.ComTypes.DISPPARAMS;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace CaptureManagerToCSharpProxy
{
   [ComImport]
   [Guid("00000001-0000-0000-C000-000000000046")]
   [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
   public interface IClassFactory
   {
      [return: MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 1)]
      void CreateInstance([MarshalAs(UnmanagedType.IUnknown)]
                          object pUnkOuter,
                          [In] ref Guid riid,
                          [MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 1)]
                          out object pUnknown);

      void LockServer([MarshalAs(UnmanagedType.Bool)]
                      bool fLock);
   }

   [ComImport]
   [Guid("00020400-0000-0000-C000-000000000046")]
   [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
   internal interface IDispatch
   {
      [PreserveSig]
      int GetTypeInfoCount(out int Count);

      [PreserveSig]
      int GetTypeInfo([MarshalAs(UnmanagedType.U4)]
                      int iTInfo,
                      [MarshalAs(UnmanagedType.U4)]
                      int lcid,
                      out ITypeInfo typeInfo);

      [PreserveSig]
      int GetIDsOfNames(ref Guid riid,
                        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]
                        string[] rgsNames,
                        int cNames,
                        int lcid,
                        [MarshalAs(UnmanagedType.LPArray)]
                        int[] rgDispId);

      [PreserveSig]
      int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, ref DISPPARAMS pDispParams, out object pVarResult, ref EXCEPINFO pExcepInfo, out uint pArgErr);
   }

   internal struct Win32NativeMethods
   {
      #region Constants

      private const ushort DISPATCH_PROPERTYPUT = 4;
      private const int DISPID_PROPERTYPUT = -3;

      private const int LOCALE_SYSTEM_DEFAULT = 2048;
      private const int SizeOfNativeVariant = 16;

      #endregion

      #region  Delegates

      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      public delegate uint DllGetClassObjectDelegate([MarshalAs(UnmanagedType.LPStruct)]
                                                     Guid rclsid,
                                                     [MarshalAs(UnmanagedType.LPStruct)]
                                                     Guid riid,
                                                     [MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 1)]
                                                     out object pUnknown);

      #endregion

      #region Enums

      public enum CommonErrors
      {
         Unknown = 0

         // A load of values from http://blogs.msdn.com/b/eldar/archive/2007/04/03/a-lot-of-hresult-codes.aspx
      }

      public enum InvokeFlags : ushort
      {
         DISPATCH_METHOD = 1,
         DISPATCH_PROPERTYGET = 2,
         DISPATCH_PROPERTYPUT = 4
      }

      #endregion

      #region Public methods

      [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
      public static extern uint FreeLibrary(IntPtr hLibModule);

      [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
      public static extern IntPtr GetProcAddress(IntPtr hModule,
                                                 [MarshalAs(UnmanagedType.LPStr)]
                                                 string lpProcName);


      public static T Invoke<T>(object source, InvokeFlags invokeFlags, string name, params object[] args)
      {
         if (source == null) {
            throw new ArgumentNullException("source");
         }

         if (!Enum.IsDefined(typeof(InvokeFlags), invokeFlags)) {
            throw new ArgumentOutOfRangeException("invokeFlags");
         }

         if (args == null) {
            throw new ArgumentNullException("args");
         }

         var memoryAllocationsToFree = new List<IntPtr>();
         IntPtr rgdispidNamedArgs;
         int cNamedArgs;
         if (invokeFlags == InvokeFlags.DISPATCH_PROPERTYPUT) {
            // There must be at least one argument specified; only one if it is a non-indexed property and
            // multiple if there are index values as well as the value to set to
            if (args.Length < 1) {
               throw new ArgumentException("At least one argument must be specified for DISPATCH_PROPERTYPUT");
            }

            var pdPutID = Marshal.AllocCoTaskMem(sizeof(long));
            Marshal.WriteInt64(pdPutID, DISPID_PROPERTYPUT);
            memoryAllocationsToFree.Add(pdPutID);

            rgdispidNamedArgs = pdPutID;
            cNamedArgs = 1;
         } else {
            rgdispidNamedArgs = IntPtr.Zero;
            cNamedArgs = 0;
         }

         var variantsToClear = new List<IntPtr>();
         IntPtr rgvarg;
         if (args.Length == 0) {
            rgvarg = IntPtr.Zero;
         } else {
            // We need to allocate enough memory to store a variant for each argument (and then populate this
            // memory)
            rgvarg = Marshal.AllocCoTaskMem(SizeOfNativeVariant * args.Length);
            memoryAllocationsToFree.Add(rgvarg);
            for (var index = 0; index < args.Length; index++) {
               // Note: The "IDispatch::Invoke method (Automation)" page
               // (http://msdn.microsoft.com/en-us/library/windows/desktop/ms221479(v=vs.85).aspx) states that
               // "Arguments are stored in pDispParams->rgvarg in reverse order" so we'll reverse them here
               var arg = args[args.Length - 1 - index];

               // According to http://stackoverflow.com/a/1866268 it seems like using ToInt64 here will be valid
               // for both 32 and 64 bit machines. While this may apparently not be the most performant approach,
               // it should do the job.
               // Don't think we have to worry about pinning any references when we do this manipulation here
               // since we are allocating the array in unmanaged memory and so the garbage collector won't be
               // moving anything around (and GetNativeVariantForObject copies the reference and automatic
               // pinning will prevent the GC from interfering while this is happening).
               var pVariant = new IntPtr(rgvarg.ToInt64() + (SizeOfNativeVariant * index));
               Marshal.GetNativeVariantForObject(arg, pVariant);
               variantsToClear.Add(pVariant);
            }
         }

         var dispParams = new DISPPARAMS {
            cArgs = args.Length,
            rgvarg = rgvarg,
            cNamedArgs = cNamedArgs,
            rgdispidNamedArgs = rgdispidNamedArgs
         };

         try {
            var dispId = GetDispId(source, name);
            var IID_NULL = new Guid("00000000-0000-0000-0000-000000000000");
            uint pArgErr = 0;
            object varResult;
            var excepInfo = new EXCEPINFO();
            var hrRet = ((IDispatch) source).Invoke(dispId, ref IID_NULL, LOCALE_SYSTEM_DEFAULT, (ushort) invokeFlags, ref dispParams, out varResult, ref excepInfo, out pArgErr);
            if (hrRet != 0) {
               var message = "Failing attempting to invoke method with DispId " + dispId + ": ";
               if ((excepInfo.bstrDescription ?? "").Trim() == "") {
                  message += "Unspecified error";
               } else {
                  message += excepInfo.bstrDescription;
               }

               var errorType = GetErrorMessageForHResult(hrRet);
               if (errorType != CommonErrors.Unknown) {
                  message += " [" + errorType + "]";
               }

               throw new ArgumentException(message);
            }

            return (T) varResult;
         } finally {
            foreach (var variantToClear in variantsToClear) {
               VariantClear(variantToClear);
            }

            foreach (var memoryAllocationToFree in memoryAllocationsToFree) {
               Marshal.FreeCoTaskMem(memoryAllocationToFree);
            }
         }
      }

      [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
      public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]
                                              string lpFileName);

      #endregion

      #region Private methods

      private static int GetDispId(object source, string name)
      {
         if (source == null) {
            throw new ArgumentNullException("source");
         }

         if (string.IsNullOrEmpty(name)) {
            throw new ArgumentNullException("Null/blank name specified");
         }

         // This will be populated with a the DispId of the named member (if available)
         var rgDispId = new int[1] {
            0
         };
         var IID_NULL = new Guid("00000000-0000-0000-0000-000000000000");
         var hrRet = ((IDispatch) source).GetIDsOfNames(ref IID_NULL, new string[1] {
                                                           name
                                                        }, 1, // number of names to get ids for
                                                        LOCALE_SYSTEM_DEFAULT, rgDispId);
         if (hrRet != 0) {
            var message = "Invalid member \"" + name + "\"";
            var errorType = GetErrorMessageForHResult(hrRet);
            if (errorType != CommonErrors.Unknown) {
               message += " [" + errorType + "]";
            }

            throw new ArgumentException(message);
         }

         return rgDispId[0];
      }

      private static CommonErrors GetErrorMessageForHResult(int hrRet)
      {
         if (Enum.IsDefined(typeof(CommonErrors), hrRet)) {
            return (CommonErrors) hrRet;
         }

         return CommonErrors.Unknown;
      }


      [DllImport(@"oleaut32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
      private static extern int VariantClear(IntPtr pvarg);

      #endregion
   }
}
