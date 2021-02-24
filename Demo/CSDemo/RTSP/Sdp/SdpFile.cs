using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Rtsp.Sdp
{
   public class SdpFile
   {
      #region  Fields

      private readonly List<Attribut> attributs = new List<Attribut>();

      private readonly List<Media> medias = new List<Media>();

      private readonly List<Timing> timingList = new List<Timing>();

      #endregion

      #region Public properties

      public IList<Attribut> Attributs => attributs;

      public Bandwidth Bandwidth
      {
         get;
         set;
      }

      public Connection Connection
      {
         get;
         set;
      }

      public string Email
      {
         get;
         set;
      }

      public EncriptionKey EncriptionKey
      {
         get;
         set;
      }

      public IList<Media> Medias => medias;


      public Origin Origin
      {
         get;
         set;
      }

      public string Phone
      {
         get;
         set;
      }

      public string Session
      {
         get;
         set;
      }

      public string SessionInformation
      {
         get;
         set;
      }

      public SdpTimeZone TimeZone
      {
         get;
         set;
      }

      public IList<Timing> Timings => timingList;

      public Uri Url
      {
         get;
         set;
      }


      public int Version
      {
         get;
         set;
      }

      #endregion

      #region Public methods

      /// <summary>
      ///    Reads the specified SDP stream.
      ///    As define in RFC 4566
      /// </summary>
      /// <param name="sdpStream">The SDP stream.</param>
      /// <returns></returns>
      public static SdpFile Read(TextReader sdpStream)
      {
         var returnValue = new SdpFile();
         var value = GetKeyValue(sdpStream);

         // Version mandatory
         if (value.Key == "v") {
            returnValue.Version = int.Parse(value.Value, CultureInfo.InvariantCulture);
            value = GetKeyValue(sdpStream);
         } else {
            throw new InvalidDataException();
         }

         // Origin mandatory
         if (value.Key == "o") {
            returnValue.Origin = Origin.Parse(value.Value);
            value = GetKeyValue(sdpStream);
         } else {
            throw new InvalidDataException();
         }

         // Session mandatory.
         // However the MuxLab HDMI Encoder (TX-500762) Firmware 1.0.6
         // does not include the 'Session' so supress InvalidDatarException
         if (value.Key == "s") {
            returnValue.Session = value.Value;
            value = GetKeyValue(sdpStream);
         }

         // Session Information optional
         if (value.Key == "i") {
            returnValue.SessionInformation = value.Value;
            value = GetKeyValue(sdpStream);
         }

         // Uri optional
         if (value.Key == "u") {
            returnValue.Url = new Uri(value.Value);
            value = GetKeyValue(sdpStream);
         }

         // Email optional
         if (value.Key == "e") {
            returnValue.Email = value.Value;
            value = GetKeyValue(sdpStream);
         }

         // Phone optional
         if (value.Key == "p") {
            returnValue.Phone = value.Value;
            value = GetKeyValue(sdpStream);
         }

         // Connection optional
         if (value.Key == "c") {
            returnValue.Connection = Connection.Parse(value.Value);
            value = GetKeyValue(sdpStream);
         }

         // bandwidth optional
         if (value.Key == "b") {
            returnValue.Bandwidth = Bandwidth.Parse(value.Value);
            value = GetKeyValue(sdpStream);
         }

         // Timing mandatory
         while (value.Key == "t") {
            var timing = value.Value;
            var repeat = string.Empty;
            value = GetKeyValue(sdpStream);
            if (value.Key == "r") {
               repeat = value.Value;
               value = GetKeyValue(sdpStream);
            }

            returnValue.Timings.Add(new Timing(timing, repeat));
         }

         // timezone optional
         if (value.Key == "z") {
            returnValue.TimeZone = SdpTimeZone.ParseInvariant(value.Value);
            value = GetKeyValue(sdpStream);
         }

         // encryption key optional
         if (value.Key == "k") {
            returnValue.EncriptionKey = EncriptionKey.ParseInvariant(value.Value);
            value = GetKeyValue(sdpStream);
         }

         //Attribute optional multiple
         while (value.Key == "a") {
            returnValue.Attributs.Add(Attribut.ParseInvariant(value.Value));
            value = GetKeyValue(sdpStream);
         }

         // Hack for MuxLab HDMI Encoder (TX-500762) Firmware 1.0.6
         // Skip over all other Key/Value pairs until the 'm=' key
         while (value.Key != "m") {
            value = GetKeyValue(sdpStream);
         }

         // Media
         while (value.Key == "m") {
            var newMedia = ReadMedia(sdpStream, ref value);
            returnValue.Medias.Add(newMedia);
         }


         return returnValue;
      }

      #endregion

      #region Private methods

      private static KeyValuePair<string, string> GetKeyValue(TextReader sdpStream)
      {
         var line = sdpStream.ReadLine();

         // end of file ?
         if (string.IsNullOrEmpty(line)) {
            return new KeyValuePair<string, string>(null, null);
         }


         var parts = line.Split(new[] {
            '='
         }, 2);
         if (parts.Length != 2) {
            throw new InvalidDataException();
         }

         if (parts[0].Length != 1) {
            throw new InvalidDataException();
         }

         var value = new KeyValuePair<string, string>(parts[0], parts[1]);
         return value;
      }

      private static Media ReadMedia(TextReader sdpStream, ref KeyValuePair<string, string> value)
      {
         var returnValue = new Media(value.Value);
         value = GetKeyValue(sdpStream);

         // Media title
         if (value.Key == "i") {
            value = GetKeyValue(sdpStream);
         }

         // Connexion optional
         if (value.Key == "c") {
            returnValue.Connection = Connection.Parse(value.Value);
            value = GetKeyValue(sdpStream);
         }

         // bandwidth optional
         if (value.Key == "b") {
            returnValue.Bandwidth = Bandwidth.Parse(value.Value);
            value = GetKeyValue(sdpStream);
         }

         // enkription key optional
         if (value.Key == "k") {
            returnValue.EncriptionKey = EncriptionKey.ParseInvariant(value.Value);
            value = GetKeyValue(sdpStream);
         }

         //Attribut optional multiple
         while (value.Key == "a") {
            returnValue.Attributs.Add(Attribut.ParseInvariant(value.Value));
            value = GetKeyValue(sdpStream);
         }

         return returnValue;
      }

      #endregion
   }
}
