using System.IO;

namespace Rtsp
{
   /// <summary>
   ///    Interface for Transport of Rtsp (TCP, TCP+SSL,..)
   /// </summary>
   public interface IRtspTransport
   {
      #region Public properties

      /// <summary>
      ///    Gets a value indicating whether this <see cref="IRtspTransport" /> is connected.
      /// </summary>
      /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
      bool Connected
      {
         get;
      }

      /// <summary>
      ///    Gets the remote address.
      /// </summary>
      /// <value>The remote address.</value>
      string RemoteAddress
      {
         get;
      }

      #endregion

      #region Public methods

      /// <summary>
      ///    Closes this instance.
      /// </summary>
      void Close();

      /// <summary>
      ///    Gets the stream of the transport.
      /// </summary>
      /// <returns>A stream</returns>
      Stream GetStream();

      /// <summary>
      ///    Reconnect this instance.
      ///    <remarks>Must do nothing if already connected.</remarks>
      /// </summary>
      /// <exception cref="System.Net.Sockets.SocketException">Error during socket </exception>
      void Reconnect();

      #endregion
   }
}
