﻿using System.Collections.Generic;
using System.Linq;

namespace Rtsp.Sdp
{
   public class Media
   {
      #region Constructors and destructors

      public Media(string mediaString)
      {
         // Example is   'video 0 RTP/AVP 26;
         this.mediaString = mediaString;

         var parts = mediaString.Split(new[] {
            ' '
         }, 4);

         if (parts.Count() >= 1) {
            if (parts[0].Equals("video")) {
               MediaType = MediaTypes.video;
            } else if (parts[0].Equals("audio")) {
               MediaType = MediaTypes.audio;
            } else if (parts[0].Equals("text")) {
               MediaType = MediaTypes.text;
            } else if (parts[0].Equals("application")) {
               MediaType = MediaTypes.application;
            } else if (parts[0].Equals("message")) {
               MediaType = MediaTypes.message;
            } else {
               MediaType = MediaTypes.unknown; // standard does allow for future types to be defined
            }
         }

         int pt;
         if (parts.Count() >= 4) {
            if (int.TryParse(parts[3], out pt)) {
               PayloadType = pt;
            } else {
               PayloadType = 0;
            }
         }
      }

      #endregion

      #region  Fields

      private readonly List<Attribut> attributs = new List<Attribut>();
      private string mediaString;

      #endregion

      #region Enums

      // RFC4566 Media Types
      public enum MediaTypes
      {
         video,
         audio,
         text,
         application,
         message,
         unknown
      }

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

      public EncriptionKey EncriptionKey
      {
         get;
         set;
      }

      public MediaTypes MediaType
      {
         get;
         set;
      }

      public int PayloadType
      {
         get;
         set;
      }

      #endregion
   }
}
