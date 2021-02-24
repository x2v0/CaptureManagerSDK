﻿using System;
using System.Security.Cryptography;
using System.Text;
using Rtsp.Messages;

namespace Rtsp
{
   // WWW-Authentication and Authorization Headers
   public class Authentication
   {
      #region Constants

      private const char quote = '\"';

      #endregion

      #region Constructors and destructors

      // Constructor
      public Authentication(string username, string password, string realm, Type authentication_type)
      {
         this.username = username;
         this.password = password;
         this.realm = realm;
         this.authentication_type = authentication_type;

         nonce = new Random().Next(100000000, 999999999).ToString(); // random 9 digit number            
      }

      #endregion

      #region  Fields

      private readonly MD5 md5 = MD5.Create();
      private readonly Type authentication_type = Type.Digest;
      private readonly string nonce;
      private readonly string password;
      private readonly string realm;

      private readonly string username;

      #endregion

      #region Enums

      //private static NLog.Logger //_logger = NLog.LogManager.GetCurrentClassLogger();

      public enum Type
      {
         Basic,
         Digest
      }

      #endregion

      #region Public methods

      // Generate Basic or Digest Authorization
      public string GenerateAuthorization(string username, string password, string auth_type, string realm, string nonce, string url, string command)
      {
         if ((username == null) ||
             (username.Length == 0)) {
            return null;
         }

         if ((password == null) ||
             (password.Length == 0)) {
            return null;
         }

         if ((realm == null) ||
             (realm.Length == 0)) {
            return null;
         }

         if (auth_type.Equals("Digest") &&
             ((nonce == null) || (nonce.Length == 0))) {
            return null;
         }

         if (auth_type.Equals("Basic")) {
            var credentials = Encoding.UTF8.GetBytes(username + ":" + password);
            var credentials_base64 = Convert.ToBase64String(credentials);
            var basic_authorization = "Basic " + credentials_base64;
            return basic_authorization;
         }

         if (auth_type.Equals("Digest")) {
            var md5 = MD5.Create();
            var hashA1 = CalculateMD5Hash(md5, username + ":" + realm + ":" + password);
            var hashA2 = CalculateMD5Hash(md5, command + ":" + url);
            var response = CalculateMD5Hash(md5, hashA1 + ":" + nonce + ":" + hashA2);

            const string quote = "\"";
            var digest_authorization = "Digest username=" + quote + username + quote + ", " + "realm=" + quote + realm + quote + ", " + "nonce=" + quote + nonce + quote + ", " + "uri=" + quote + url +
                                       quote + ", " + "response=" + quote + response + quote;

            return digest_authorization;
         }

         return null;
      }

      public string GetHeader()
      {
         if (authentication_type == Type.Basic) {
            return "Basic realm=" + quote + realm + quote;
         }

         if (authentication_type == Type.Digest) {
            return "Digest realm=" + quote + realm + quote + ", nonce=" + quote + nonce + quote;
         }

         return null;
      }


      public bool IsValid(RtspMessage received_message)
      {
         var authorization = received_message.Headers["Authorization"];


         // Check Username and Password
         if ((authentication_type == Type.Basic) &&
             authorization.StartsWith("Basic ")) {
            var base64_str = authorization.Substring(6); // remove 'Basic '
            var data = Convert.FromBase64String(base64_str);
            var decoded = Encoding.UTF8.GetString(data);
            var split_position = decoded.IndexOf(':');
            var decoded_username = decoded.Substring(0, split_position);
            var decoded_password = decoded.Substring(split_position + 1);

            if ((decoded_username == username) &&
                (decoded_password == password)) {
               ////_logger.Debug("Basic Authorization passed");
               return true;
            }

            ////_logger.Debug("Basic Authorization failed");
            return false;
         }

         // Check Username, URI, Nonce and the MD5 hashed Response
         if ((authentication_type == Type.Digest) &&
             authorization.StartsWith("Digest ")) {
            var value_str = authorization.Substring(7); // remove 'Digest '
            var values = value_str.Split(',');
            string auth_header_username = null;
            string auth_header_realm = null;
            string auth_header_nonce = null;
            string auth_header_uri = null;
            string auth_header_response = null;
            string message_method = null;
            string message_uri = null;
            try {
               message_method = received_message.Command.Split(' ')[0];
               message_uri = received_message.Command.Split(' ')[1];
            } catch {
            }

            foreach (var value in values) {
               var tuple = value.Trim().Split(new[] {
                  '='
               }, 2); // split on first '=' 
               if ((tuple.Length == 2) &&
                   tuple[0].Equals("username")) {
                  auth_header_username = tuple[1].Trim(' ', '\"'); // trim space and quotes
               } else if ((tuple.Length == 2) &&
                          tuple[0].Equals("realm")) {
                  auth_header_realm = tuple[1].Trim(' ', '\"'); // trim space and quotes
               } else if ((tuple.Length == 2) &&
                          tuple[0].Equals("nonce")) {
                  auth_header_nonce = tuple[1].Trim(' ', '\"'); // trim space and quotes
               } else if ((tuple.Length == 2) &&
                          tuple[0].Equals("uri")) {
                  auth_header_uri = tuple[1].Trim(' ', '\"'); // trim space and quotes
               } else if ((tuple.Length == 2) &&
                          tuple[0].Equals("response")) {
                  auth_header_response = tuple[1].Trim(' ', '\"'); // trim space and quotes
               }
            }

            // Create the MD5 Hash using all parameters passed in the Auth Header with the 
            // addition of the 'Password'
            var hashA1 = CalculateMD5Hash(md5, auth_header_username + ":" + auth_header_realm + ":" + password);
            var hashA2 = CalculateMD5Hash(md5, message_method + ":" + auth_header_uri);
            var expected_response = CalculateMD5Hash(md5, hashA1 + ":" + auth_header_nonce + ":" + hashA2);

            // Check if everything matches
            // ToDo - extract paths from the URIs (ignoring SETUP's trackID)
            if ((auth_header_username == username) &&
                (auth_header_realm == realm) &&
                (auth_header_nonce == nonce) &&
                (auth_header_response == expected_response)) {
               ////_logger.Debug("Digest Authorization passed");
               return true;
            }

            ////_logger.Debug("Digest Authorization failed");
            return false;
         }

         return false;
      }

      #endregion

      #region Private methods

      // MD5 (lower case)
      private string CalculateMD5Hash(MD5 md5_session, string input)
      {
         var inputBytes = Encoding.UTF8.GetBytes(input);
         var hash = md5_session.ComputeHash(inputBytes);

         var output = new StringBuilder();
         for (var i = 0; i < hash.Length; i++) {
            output.Append(hash[i].ToString("x2"));
         }

         return output.ToString();
      }

      #endregion
   }
}
