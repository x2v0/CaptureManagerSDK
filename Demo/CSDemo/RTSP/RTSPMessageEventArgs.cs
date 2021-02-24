using System;
using Rtsp.Messages;

namespace Rtsp
{
   /// <summary>
   ///    Event args containing information for message events.
   /// </summary>
   public class RtspChunkEventArgs : EventArgs
   {
      #region Constructors and destructors

      /// <summary>
      ///    Initializes a new instance of the <see cref="RtspChunkEventArgs" /> class.
      /// </summary>
      /// <param name="aMessage">A message.</param>
      public RtspChunkEventArgs(RtspChunk aMessage)
      {
         Message = aMessage;
      }

      #endregion

      #region Public properties

      /// <summary>
      ///    Gets or sets the message.
      /// </summary>
      /// <value>The message.</value>
      public RtspChunk Message
      {
         get;
         set;
      }

      #endregion
   }
}
