using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace FilmTrove.OAuthClients
{
    public class CustomOAuthHelpers
    {
        
        /*
         * But to make signed requests you need to follow a simple process:

            1)  Collecting request parameters
            2)  Calculating signature
            3)  Making request(signed/protoected)

         */

        public static String GenerateTimestamp()
        {
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
        }
        public static String GenerateNonce()
        {
            Byte[] bytes = new byte[32];
            Byte[] first = Guid.NewGuid().ToByteArray();
            Byte[] second = Guid.NewGuid().ToByteArray();
            for (var i = 0; i < 16; i++)
                bytes[i] = first[i];
            for (var i = 16; i < 32; i++)
                bytes[i] = second[i - 16];
            return new String(Convert.ToBase64String(bytes, Base64FormattingOptions.None).ToCharArray().Where(Char.IsLetter).ToArray());
        }

        //public static String Encode(string source)
        //{
        //    Func<Char, String> encodeCharacter = c =>
        //    {
        //        if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c == '.' || c == '-' || c == '_' || c == '~'))
        //            return new string(c, 1);
        //        return HttpUtility.UrlEncode(c.ToString());//EncodeCharacter(c);
        //    };
        //    return string.Concat(source.ToCharArray().Select(encodeCharacter));
        //}

        public static String CalculateParameterString(KeyValuePair<String, String>[] parameters)
        {
            var q = from entry in parameters
                    let encodedkey = HttpUtility.UrlEncode(entry.Key)
                    let encodedValue = HttpUtility.UrlEncode(entry.Value)
                    let encodedEntry = encodedkey + "=" + encodedValue
                    orderby encodedEntry
                    select encodedEntry;
            return String.Join("&", q.ToArray());
        }

        public static String CalculateSignatureBaseString(String httpMethod, String baseUri, String parametersString)
        {
            return HttpUtility.UrlEncode(httpMethod.ToUpper() + "&" + baseUri + "&" + parametersString);
        }

        public static String GetSigningKey(String ConsumerSecret, String OAuthTokenSecret = null)
        {
            return ConsumerSecret + "&" + (OAuthTokenSecret != null ? OAuthTokenSecret : "");
        }

        public static String Sign(String signatureBaseString, String signingKey)
        {
            Byte[] keyBytes = System.Text.Encoding.ASCII.GetBytes(signingKey);
            using (var myhmacsha1 = new System.Security.Cryptography.HMACSHA1(keyBytes))
            {
                Byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(signatureBaseString);
                using (var stream = new MemoryStream(byteArray))
                {
                    Byte[] signedValue = myhmacsha1.ComputeHash(stream);
                    return Convert.ToBase64String(signedValue, Base64FormattingOptions.None);
                }
            }
        }

        public virtual String GetSignature(String consumerSecret, String tokenSecret, String uri, String method, Dictionary<String, String> parameters)
        {
            String parametersString = CalculateParameterString(parameters.ToArray());
            String signatureBaseString = CalculateSignatureBaseString(method, uri, parametersString);
            String sigingKey = GetSigningKey(consumerSecret, tokenSecret);
            String signature = Sign(signatureBaseString, sigingKey);
            return signature;
        }
    }
}