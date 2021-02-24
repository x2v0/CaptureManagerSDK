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
using System.Diagnostics;
using System.Threading;
using System.Timers;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using InterProcessRenderer.Communication;
using Timer = System.Timers.Timer;

namespace InterProcessRenderer
{
   internal class Program
   {
      #region Static fields

      private static Program _Instance;

      #endregion

      #region Constructors and destructors

      private Program()
      {
      }

      #endregion

      #region  Fields

      public uint FrameHeight;

      public uint FrameWidth;
      private CaptureManager _CaptureManager;
      private readonly object _CaptureManagerLock = new object();
      private bool _close = true;
      private EVROutputAdapter _EVROutputAdapter;

      private IntPtr _HWND = IntPtr.Zero;
      private IEVRStreamControl _IEVRStreamControl;

      private uint _MediaTypeIndex;

      private ISession _Session;

      private uint _StreamIndex;

      private string _SymbolicLink = "";
      private Timer _Timer;

      private readonly AutoResetEvent mCloseEvent = new AutoResetEvent(false);
      private PipeProcessor mPipeProcessor;

      private readonly AutoResetEvent mStartEvent = new AutoResetEvent(false);

      #endregion

      #region Public properties

      public static Program Instance
      {
         get
         {
            if (_Instance == null) {
               _Instance = new Program();
            }

            return _Instance;
         }
      }

      public CaptureManager CaptureManager
      {
         get
         {
            lock (_CaptureManagerLock) {
               if (_CaptureManager == null) {
                  try {
                     _CaptureManager = new CaptureManager("CaptureManager.dll");
                  } catch (Exception) {
                     try {
                        _CaptureManager = new CaptureManager();
                     } catch (Exception) {
                     }
                  }
               }
            }

            return _CaptureManager;
         }
      }

      #endregion

      #region  Other properties

      private ISession Session
      {
         get => _Session;
         set
         {
            if (_Session != value) {
               if (_Session != null) {
                  _Session.stopSession();
                  _Session.closeSession();
                  //  Panel.Invalidate();
               }

               _Session = value;
            }
         }
      }

      private Timer Timer
      {
         get
         {
            if (_Timer == null) {
               _Timer = new Timer(2000);
               _Timer.Elapsed += _Timer_Elapsed;
            }

            return _Timer;
         }
      }

      #endregion

      #region Public methods

      public void start()
      {
         mPipeProcessor = new PipeProcessor("Client", "Server");

         _IEVRStreamControl = CaptureManager.createEVRStreamControl();

         mPipeProcessor.MessageDelegateEvent += mPipeProcessor_MessageDelegateEvent;

         mPipeProcessor.send("Initilized");

         mStartEvent.WaitOne();

         StartSession();

         mCloseEvent.WaitOne();

         if (Session != null) {
            Session.stopSession();

            Session.closeSession();
         }

         mPipeProcessor.closeConnection();
      }

      #endregion

      #region Private methods

      [MTAThread]
      private static void Main(string[] args)
      {
         Instance.Timer.Start();

         foreach (var item in args) {
            if (item.Contains("SymbolicLink=")) {
               Instance._SymbolicLink = item.Replace("SymbolicLink=", "");
            } else if (item.Contains("StreamIndex=")) {
               Instance._StreamIndex = uint.Parse(item.Replace("StreamIndex=", ""));
            } else if (item.Contains("MediaTypeIndex=")) {
               Instance._MediaTypeIndex = uint.Parse(item.Replace("MediaTypeIndex=", ""));
            } else if (item.Contains("WindowHandler=")) {
               Instance._HWND = new IntPtr(long.Parse(item.Replace("WindowHandler=", "")));
            } else if (item.Contains("FrameHeight=")) {
               Instance.FrameHeight = uint.Parse(item.Replace("FrameHeight=", ""));
            } else if (item.Contains("FrameWidth=")) {
               Instance.FrameWidth = uint.Parse(item.Replace("FrameWidth=", ""));
            }
         }

         Instance.start();
      }

      private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
      {
         if (_close) {
            Process.GetCurrentProcess().Kill();
         }

         _close = true;
      }

      private void getEVRStreamFilters()
      {
         if ((_EVROutputAdapter == null) ||
             (_IEVRStreamControl == null)) {
            return;
         }

         var lxmldoc = "";

         _IEVRStreamControl.getCollectionOfFilters(_EVROutputAdapter.Node, out lxmldoc);

         if (string.IsNullOrEmpty(lxmldoc)) {
            return;
         }

         mPipeProcessor.send("Get_EVRStreamFilters=" + lxmldoc);
      }

      private void getEVRStreamOutputFeatures()
      {
         if ((_EVROutputAdapter == null) ||
             (_IEVRStreamControl == null)) {
            return;
         }

         var lxmldoc = "";

         _IEVRStreamControl.getCollectionOfOutputFeatures(_EVROutputAdapter.Node, out lxmldoc);

         if (string.IsNullOrEmpty(lxmldoc)) {
            return;
         }

         mPipeProcessor.send("Get_EVRStreamOutputFeatures=" + lxmldoc);
      }

      private void HandleSessionStateChanged(uint aCallbackEventCode, uint aSessionDescriptor)
      {
         var k = (SessionCallbackEventCode) aCallbackEventCode;

         switch (k) {
            case SessionCallbackEventCode.Unknown:
               break;
            case SessionCallbackEventCode.Error:
               break;
            case SessionCallbackEventCode.Status_Error:
               break;
            case SessionCallbackEventCode.Execution_Error:
               break;
            case SessionCallbackEventCode.ItIsReadyToStart:
               break;
            case SessionCallbackEventCode.ItIsStarted:
               break;
            case SessionCallbackEventCode.ItIsPaused:
               break;
            case SessionCallbackEventCode.ItIsStopped:
               break;
            case SessionCallbackEventCode.ItIsEnded:
               break;
            case SessionCallbackEventCode.ItIsClosed:
               break;
            case SessionCallbackEventCode.VideoCaptureDeviceRemoved:
            {
            }
               break;
         }
      }

      private void mPipeProcessor_MessageDelegateEvent(string aMessage)
      {
         switch (aMessage) {
            case "Start":
               mStartEvent.Set();
               break;

            case "Close":
               mCloseEvent.Set();
               break;

            case "HeartBeat":
               _close = false;
               break;

            case "Get_EVRStreamFilters":
               getEVRStreamFilters();
               break;

            case "Get_EVRStreamOutputFeatures":
               getEVRStreamOutputFeatures();
               break;
         }


         if (aMessage.Contains("Set_EVRStreamOutputFeatures=")) {
            var lparts = aMessage.Replace("Set_EVRStreamOutputFeatures=", "").Split('_');

            if ((lparts != null) &&
                (lparts.Length == 2)) {
               var lIndex = uint.Parse(lparts[0]);

               var lvalue = int.Parse(lparts[1]);

               setEVRStreamOutputFeatures(lIndex, lvalue);
            }
         }


         if (aMessage.Contains("Set_EVRStreamFilters=")) {
            var lparts = aMessage.Replace("Set_EVRStreamFilters=", "").Split('_');

            if ((lparts != null) &&
                (lparts.Length == 3)) {
               var lIndex = uint.Parse(lparts[0]);

               var lvalue = int.Parse(lparts[1]);

               var lIsEnable = bool.Parse(lparts[2]);

               setFilterParametr(lIndex, lvalue, lIsEnable);
            }
         }
      }

      private void setEVRStreamOutputFeatures(uint aIndex, int aValue)
      {
         if ((_EVROutputAdapter == null) ||
             (_IEVRStreamControl == null)) {
            return;
         }

         _IEVRStreamControl.setOutputFeatureParametr(_EVROutputAdapter.Node, aIndex, aValue);
      }

      private void setFilterParametr(uint aParametrIndex, int aNewValue, bool aIsEnabled)
      {
         if ((_EVROutputAdapter == null) ||
             (_IEVRStreamControl == null)) {
            return;
         }

         _IEVRStreamControl.setFilterParametr(_EVROutputAdapter.Node, aParametrIndex, aNewValue, aIsEnabled);
      }

      private void StartSession()
      {
         object lPtrSourceNode = null;
         var sourceControl = CaptureManager.createSourceControl();
         var sessionControl = CaptureManager.createSessionControl();

         _EVROutputAdapter = new EVROutputAdapter(Instance, _HWND);

         if ((sessionControl != null) &&
             (sessionControl != null) &&
             (_EVROutputAdapter.Node != null)) {
            Session = null;

            sourceControl.createSourceNode(_SymbolicLink, _StreamIndex, _MediaTypeIndex, _EVROutputAdapter.Node, out lPtrSourceNode);

            Session = sessionControl.createSession(new[] {
               lPtrSourceNode
            });

            if (Session != null) {
               Session.registerUpdateStateDelegate(HandleSessionStateChanged);
               Session.startSession(0, Guid.Empty);
            }
         }
      }

      #endregion
   }
}
