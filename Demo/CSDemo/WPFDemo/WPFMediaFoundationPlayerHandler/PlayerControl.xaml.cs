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
using System.Windows.Controls;
using CaptureManagerToCSharpProxy.Interfaces;
using MediaFoundation;

namespace WPFMediaFoundationPlayerHandler
{
   /// <summary>
   ///    Interaction logic for PlayerControl.xaml
   /// </summary>
   public partial class PlayerControl : UserControl
   {
      #region Constructors and destructors

      public PlayerControl()
      {
         InitializeComponent();
      }

      #endregion

      #region  Fields

      private readonly List<Player> lPlayerList = new List<Player>();

      #endregion

      #region Public properties

      public IntPtr Handle => mEVRDisplay.Handle;

      #endregion

      #region Public methods

      public void setRenderList(List<IMFTopologyNode> aEVRList, IEVRStreamControl aIEVRStreamControl, uint aMaxVideoRenderStreamCount)
      {
         m_PlayerCanvas.Children.Clear();

         foreach (var item in lPlayerList) {
            item.Stop();
         }

         lPlayerList.Clear();

         var lColumnCount = 2;

         var lRowCount = Math.Ceiling((aEVRList.Count / (lColumnCount + 1.0)) + 1.0);

         var lRowHeight = m_PlayerCanvas.Height / lRowCount;

         var lColumnWidth = m_PlayerCanvas.Width / lColumnCount;

         for (var i = 0; i < aEVRList.Count; i++) {
            var lPlayer = new Player();

            m_PlayerCanvas.Children.Add(lPlayer);

            lPlayerList.Add(lPlayer);

            lPlayer.Width = lColumnWidth;

            lPlayer.Height = lRowHeight;

            var lTopPos = i / lColumnCount;

            Canvas.SetTop(lPlayer, lTopPos * lRowHeight);

            double lLeftPos = i - (lTopPos * lColumnCount);

            Canvas.SetLeft(lPlayer, lLeftPos * lColumnWidth);

            lPlayer.mIMFTopologyNode = aEVRList[i];

            lPlayer.mIEVRStreamControl = aIEVRStreamControl;

            lPlayer.mMaxVideoRenderStreamCount = aMaxVideoRenderStreamCount;

            aIEVRStreamControl.setPosition(lPlayer.mIMFTopologyNode, (float) ((lLeftPos * lColumnWidth) / m_PlayerCanvas.Width),
                                           (float) (((lLeftPos * lColumnWidth) + lColumnWidth) / m_PlayerCanvas.Width), (float) ((lTopPos * lRowHeight) / m_PlayerCanvas.Height),
                                           (float) (((lTopPos * lRowHeight) + lRowHeight) / m_PlayerCanvas.Height));

            Panel.SetZIndex(lPlayer, i);
         }
      }

      #endregion
   }
}
