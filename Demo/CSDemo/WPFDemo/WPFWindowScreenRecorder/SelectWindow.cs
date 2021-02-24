/*
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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using gma.System.Windows;

namespace WPFWindowScreenRecorder
{
   internal delegate void updateWindow(string a_Name, IntPtr a_HWND);

   internal delegate void pressedKey(char a_KeyChar);

   internal delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

   internal class SelectWindow
   {
      #region Constants

      private const int WH_MOUSE = 7;
      private const int WM_LBUTTONDOWN = 0x0201;

      #endregion

      #region Static fields

      private static SelectWindow m_Instance;

      #endregion

      #region Constructors and destructors

      private SelectWindow()
      {
      }

      #endregion

      #region  Fields

      private UserActivityHook actHook;

      #endregion

      #region Public events

      public static event pressedKey m_pressedKey;

      public static event updateWindow m_updateWindowNameEvent;

      #endregion

      #region Public methods

      public static SelectWindow getInstance()
      {
         if (m_Instance == null) {
            m_Instance = new SelectWindow();
         }

         return m_Instance;
      }

      public void setupMouseHook()
      {
         actHook = new UserActivityHook();

         actHook.OnMouseActivity += actHook_OnMouseActivity;

         actHook.KeyPress += actHook_KeyPress;
      }

      public void uninstallMouseHook()
      {
         actHook.Stop();
      }

      #endregion

      #region Private methods

      private static string GetCaptionOfWindow(IntPtr hwnd)
      {
         var caption = "";
         StringBuilder windowText = null;
         try {
            var max_length = NativeMethods.GetWindowTextLength(hwnd);
            windowText = new StringBuilder("", max_length + 5);
            NativeMethods.GetWindowText(hwnd, windowText, max_length + 2);

            if (!string.IsNullOrEmpty(windowText.ToString()) &&
                !string.IsNullOrWhiteSpace(windowText.ToString())) {
               caption = windowText.ToString();
            }
         } catch (Exception ex) {
            caption = ex.Message;
         } finally {
            windowText = null;
         }

         return caption;
      }

      private void actHook_KeyPress(object sender, KeyPressEventArgs e)
      {
         if (m_pressedKey != null) {
            m_pressedKey(e.KeyChar);
         }
      }


      private void actHook_OnMouseActivity(object sender, MouseEventArgs e)
      {
         if (m_updateWindowNameEvent != null) {
            var pt = new NativeMethods.POINT();

            var buf = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.POINT)));

            var h = NativeMethods.GetCursorPos(buf);

            pt = (NativeMethods.POINT) Marshal.PtrToStructure(buf, typeof(NativeMethods.POINT));

            Marshal.FreeHGlobal(buf);

            var hwnd = NativeMethods.WindowFromPhysicalPoint(pt);

            m_updateWindowNameEvent(GetCaptionOfWindow(hwnd), hwnd);
         }
      }

      #endregion
   }
}
