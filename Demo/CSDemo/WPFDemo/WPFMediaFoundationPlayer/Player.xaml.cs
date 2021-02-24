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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CaptureManagerToCSharpProxy.Interfaces;
using MediaFoundation;
using MediaFoundation.Misc;
using Microsoft.Win32;

namespace WPFMediaFoundationPlayer
{
   /// <summary>
   ///    Interaction logic for Player.xaml
   /// </summary>
   public partial class Player : UserControl
   {
      #region Constructors and destructors

      public Player()
      {
         InitializeComponent();

         mTickTimer.Interval = TimeSpan.FromMilliseconds(100);

         mTickTimer.Tick += mTickTimer_Tick;
      }

      #endregion

      #region  Fields

      public IEVRStreamControl mIEVRStreamControl = null;

      public IMFTopologyNode mIMFTopologyNode = null;

      public uint mMaxVideoRenderStreamCount = 0;
      protected IMFMediaSession m_pSession;
      protected IMFMediaSource m_pSource;

      private bool lPress;

      private bool mIsPlaying;

      private bool mIsSeek;

      private long mMediaDuration;

      private double mNewValue = -1.0;

      private IMFClock mPresentationClock;

      private Point mPrevPoint;

      private readonly DispatcherTimer mTickTimer = new DispatcherTimer();

      #endregion

      #region Public methods

      public void Stop()
      {
         if (m_pSession != null) {
            var hr = m_pSession.Stop();
         }
      }

      #endregion

      #region Protected methods

      protected void AddBranchToPartialTopology(IMFTopology pTopology, IMFPresentationDescriptor pSourcePD, int iStream)
      {
         MFError throwonhr;

         IMFStreamDescriptor pSourceSD = null;
         IMFTopologyNode pSourceNode = null;
         IMFTopologyNode pOutputNode = null;
         var fSelected = false;

         // Get the stream descriptor for this stream.
         throwonhr = pSourcePD.GetStreamDescriptorByIndex(iStream, out fSelected, out pSourceSD);

         // Create the topology branch only if the stream is selected.
         // Otherwise, do nothing.
         if (fSelected) {
            // Create a source node for this stream.
            CreateSourceStreamNode(pSourcePD, pSourceSD, out pSourceNode);

            // Create the output node for the renderer.
            CreateOutputNode(pSourceSD, out pOutputNode);

            // Add both nodes to the topology.
            throwonhr = pTopology.AddNode(pSourceNode);
            throwonhr = pTopology.AddNode(pOutputNode);

            // Connect the source node to the output node.
            throwonhr = pSourceNode.ConnectOutput(0, pOutputNode, 0);
         }
      }


      protected void CreateMediaSource(string sURL)
      {
         IMFSourceResolver pSourceResolver;
         object pSource;

         // Create the source resolver.
         var hr = MFExtern.MFCreateSourceResolver(out pSourceResolver);
         MFError.ThrowExceptionForHR(hr);

         try {
            // Use the source resolver to create the media source.
            var ObjectType = MFObjectType.Invalid;

            hr = pSourceResolver.CreateObjectFromURL(sURL, // URL of the source.
                                                     MFResolution.MediaSource, // Create a source object.
                                                     null, // Optional property store.
                                                     out ObjectType, // Receives the created object type.
                                                     out pSource // Receives a pointer to the media source.
                                                    );
            MFError.ThrowExceptionForHR(hr);

            // Get the IMFMediaSource interface from the media source.
            m_pSource = (IMFMediaSource) pSource;
         } finally {
            // Clean up
            Marshal.ReleaseComObject(pSourceResolver);
         }
      }

      protected void CreateOutputNode(IMFStreamDescriptor pSourceSD, out IMFTopologyNode ppNode)
      {
         IMFTopologyNode pNode = null;
         IMFMediaTypeHandler pHandler = null;
         IMFActivate pRendererActivate = null;

         var guidMajorType = Guid.Empty;
         MFError throwonhr;

         // Get the stream ID.
         var streamID = 0;

         HResult hr;

         hr = pSourceSD.GetStreamIdentifier(out streamID); // Just for debugging, ignore any failures.
         if (MFError.Failed(hr)) {
            //TRACE("IMFStreamDescriptor::GetStreamIdentifier" + hr.ToString());
         }

         // Get the media type handler for the stream.
         throwonhr = pSourceSD.GetMediaTypeHandler(out pHandler);

         // Get the major media type.
         throwonhr = pHandler.GetMajorType(out guidMajorType);

         // Create a downstream node.
         throwonhr = MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out pNode);

         // Create an IMFActivate object for the renderer, based on the media type.
         if (MFMediaType.Audio == guidMajorType) {
            // Create the audio renderer.
            //TRACE(string.Format("Stream {0}: audio stream", streamID));
            throwonhr = MFExtern.MFCreateAudioRendererActivate(out pRendererActivate);

            // Set the IActivate object on the output node.
            throwonhr = pNode.SetObject(pRendererActivate);
         } else if (MFMediaType.Video == guidMajorType) {
            // Create the video renderer.
            //TRACE(string.Format("Stream {0}: video stream", streamID));
            //throwonhr = MFExtern.MFCreateVideoRendererActivate(m_hwndVideo, out pRendererActivate);

            mIMFTopologyNode.GetObject(out pRendererActivate);

            throwonhr = pNode.SetObject(pRendererActivate);
         }


         // Return the IMFTopologyNode pointer to the caller.
         ppNode = pNode;
      }

      protected void CreateSourceStreamNode(IMFPresentationDescriptor pSourcePD, IMFStreamDescriptor pSourceSD, out IMFTopologyNode ppNode)
      {
         MFError throwonhr;
         IMFTopologyNode pNode = null;

         // Create the source-stream node.
         throwonhr = MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out pNode);

         // Set attribute: Pointer to the media source.
         throwonhr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, m_pSource);

         // Set attribute: Pointer to the presentation descriptor.
         throwonhr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR, pSourcePD);

         // Set attribute: Pointer to the stream descriptor.
         throwonhr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR, pSourceSD);

         // Return the IMFTopologyNode pointer to the caller.
         ppNode = pNode;
      }

      protected void CreateTopologyFromSource(out IMFTopology ppTopology)
      {
         IMFTopology pTopology = null;
         IMFPresentationDescriptor pSourcePD = null;
         var cSourceStreams = 0;

         MFError throwonhr;

         // Create a new topology.
         throwonhr = MFExtern.MFCreateTopology(out pTopology);

         // Create the presentation descriptor for the media source.
         throwonhr = m_pSource.CreatePresentationDescriptor(out pSourcePD);

         // Get the number of streams in the media source.
         throwonhr = pSourcePD.GetStreamDescriptorCount(out cSourceStreams);

         //TRACE(string.Format("Stream count: {0}", cSourceStreams));

         // For each stream, create the topology nodes and add them to the topology.
         for (var i = 0; i < cSourceStreams; i++) {
            AddBranchToPartialTopology(pTopology, pSourcePD, i);
         }

         // Return the IMFTopology pointer to the caller.
         ppTopology = pTopology;
      }

      protected void StartPlayback()
      {
         var hr = m_pSession.Start(Guid.Empty, new PropVariant());

         if (hr == HResult.S_OK) {
            mPlayPauseBtn.IsEnabled = true;

            mImageBtn.Source = new BitmapImage(new Uri("pack://application:,,,/WPFMediaFoundationPlayer;component/Images/pause.png", UriKind.Absolute));

            mIsPlaying = true;

            mPresentationClock = null;

            m_pSession.GetClock(out mPresentationClock);

            mTickTimer.Start();

            mIsSeek = false;
         }

         MFError.ThrowExceptionForHR(hr);

         //m_pSession.
      }

      #endregion

      #region Private methods

      private void createSession(string sFilePath)
      {
         try {
            MFError throwonhr = null;

            if (m_pSession == null) {
               throwonhr = MFExtern.MFCreateMediaSession(null, out m_pSession);
            } else {
               Stop();
            }

            // Create the media source.

            CreateMediaSource(sFilePath);

            if (m_pSource == null) {
               return;
            }

            IMFPresentationDescriptor lPresentationDescriptor = null;

            m_pSource.CreatePresentationDescriptor(out lPresentationDescriptor);

            if (lPresentationDescriptor == null) {
               return;
            }

            lPresentationDescriptor.GetUINT64(MFAttributesClsid.MF_PD_DURATION, out mMediaDuration);

            IMFTopology pTopology = null;

            // Create a partial topology.
            CreateTopologyFromSource(out pTopology);

            var hr = HResult.S_OK;
            // Set the topology on the media session.
            hr = m_pSession.SetTopology(MFSessionSetTopologyFlags.Immediate, pTopology);

            StartPlayback();
         } catch (Exception) {
         }
      }

      private void m_SelectVideoFileBtn_Click(object sender, RoutedEventArgs e)
      {
         do {
            var lopenFileDialog = new OpenFileDialog();

            lopenFileDialog.AddExtension = true;

            var lresult = lopenFileDialog.ShowDialog();

            if (lresult != true) {
               break;
            }

            createSession(lopenFileDialog.FileName);
         } while (false);
      }

      private void mPlayPauseBtn_Click(object sender, RoutedEventArgs e)
      {
         mIsSeek = false;

         mNewValue = -1.0;

         if (mIsPlaying) {
            var hr = m_pSession.Pause();

            if (hr == HResult.S_OK) {
               mImageBtn.Source = new BitmapImage(new Uri("pack://application:,,,/WPFMediaFoundationPlayer;component/Images/play-button.png", UriKind.Absolute));

               mIsPlaying = false;

               mTickTimer.Stop();
            }
         } else {
            StartPlayback();
         }
      }

      private void mSlider_MouseLeave(object sender, MouseEventArgs e)
      {
         mIsSeek = false;

         mTickTimer.Start();
      }

      private void mSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         mIsSeek = true;

         mTickTimer.Stop();
      }

      private void mSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         mIsSeek = false;

         if (mIsPlaying && (mNewValue >= 0.0)) {
            var lselectedProp = mNewValue / mSlider.Maximum;

            var startPosition = lselectedProp * mMediaDuration;

            var lStartTimeSpan = new TimeSpan((long) startPosition);

            var hr = m_pSession.Start(lStartTimeSpan);
         }

         mTickTimer.Start();
      }

      private void mSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         if (mIsPlaying && mIsSeek) {
            mNewValue = e.NewValue;
         }
      }

      private void mTickTimer_Tick(object sender, EventArgs e)
      {
         if (mPresentationClock == null) {
            return;
         }

         if (mMediaDuration == 0) {
            return;
         }

         if (mIsSeek) {
            return;
         }

         long lClockTime = 0;

         long lSystemTime = 0;

         mPresentationClock.GetCorrelatedTime(0, out lClockTime, out lSystemTime);

         mSlider.Value = lClockTime / (double) mMediaDuration * mSlider.Maximum;
      }

      private void onDragCompleted(object sender, DragCompletedEventArgs e)
      {
         myThumb.Background = Brushes.Blue;
      }

      private void onDragDelta(object sender, DragDeltaEventArgs e)
      {
         var lParentCanvas = Parent as Canvas;

         if (lParentCanvas == null) {
            return;
         }

         var lLeftPos = Canvas.GetLeft(this);

         var lTopPos = Canvas.GetTop(this);

         //Move the Thumb to the mouse position during the drag operation
         var yadjust = Height + e.VerticalChange;
         var xadjust = Width + e.HorizontalChange;
         if ((xadjust >= 0) &&
             (yadjust >= 0) &&
             ((lLeftPos + xadjust) <= lParentCanvas.Width) &&
             ((lTopPos + yadjust) <= lParentCanvas.Height)) {
            Width = xadjust;
            Height = yadjust;

            updatePosition();
         }
      }

      private void onDragStarted(object sender, DragStartedEventArgs e)
      {
         myThumb.Background = Brushes.Orange;
      }

      private void updatePosition()
      {
         var lParentCanvas = Parent as Canvas;

         if (lParentCanvas == null) {
            return;
         }

         var lLeftPos = Canvas.GetLeft(this);

         var lLeftProp = lLeftPos / lParentCanvas.Width;

         var lRightProp = (lLeftPos + Width) / lParentCanvas.Width;

         var lTopPos = Canvas.GetTop(this);

         var lTopProp = lTopPos / lParentCanvas.Height;

         var lBottomProp = (lTopPos + Height) / lParentCanvas.Height;


         mIEVRStreamControl.setPosition(mIMFTopologyNode, (float) lLeftProp, (float) lRightProp, (float) lTopProp, (float) lBottomProp);
      }

      private void UserControl_MouseLeave(object sender, MouseEventArgs e)
      {
         lPress = false;
      }

      private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         lPress = true;

         var lParentCanvas = Parent as Canvas;

         if (lParentCanvas == null) {
            return;
         }

         mPrevPoint = Mouse.GetPosition(lParentCanvas);

         var lcurrZIndex = Panel.GetZIndex(this);

         var l = new List<int>();

         foreach (var item in lParentCanvas.Children) {
            var lZIndex = Panel.GetZIndex((UIElement) item);

            if ((lParentCanvas.Children.Count - 1) == lZIndex) {
               Panel.SetZIndex(this, lZIndex);

               Panel.SetZIndex((UIElement) item, lcurrZIndex);

               break;
            }
         }

         if (mMaxVideoRenderStreamCount > 0) {
            mIEVRStreamControl.setZOrder(mIMFTopologyNode, mMaxVideoRenderStreamCount - 1);
         }
      }

      private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         lPress = false;
      }

      private void UserControl_MouseMove(object sender, MouseEventArgs e)
      {
         if (lPress) {
            var lParentCanvas = Parent as Canvas;

            if (lParentCanvas == null) {
               return;
            }

            var lPoint = Mouse.GetPosition(lParentCanvas);

            var ldiff = mPrevPoint - lPoint;

            var lLeftPos = Canvas.GetLeft(this);

            var lTopPos = Canvas.GetTop(this);

            var lnewLeftPos = lLeftPos - ldiff.X;

            if ((lnewLeftPos >= 0) &&
                (lnewLeftPos <= (lParentCanvas.Width - Width))) {
               Canvas.SetLeft(this, lnewLeftPos);
            }


            var lnewTopPos = lTopPos - ldiff.Y;

            if ((lnewTopPos >= 0) &&
                (lnewTopPos <= (lParentCanvas.Height - Height))) {
               Canvas.SetTop(this, lnewTopPos);
            }

            mPrevPoint = lPoint;

            updatePosition();
         }
      }

      #endregion
   }
}
