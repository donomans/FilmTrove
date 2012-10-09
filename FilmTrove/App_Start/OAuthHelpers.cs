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
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString("F0");
        }
        public static String GenerateNonce()
        {
            Int64 nonce = 0;
            //String nonce = "";
            Random r = new Random();
            for (Int32 i = 0; i < 15; i++)
                nonce += (nonce << i) * r.Next(0, 9) + i;//(r.Next(999331, Int32.MaxValue) * (13 + i)) + 47 * i;
            return Math.Abs(nonce).ToString("F0");
            //Byte[] bytes = new byte[32];
            //Byte[] first = Guid.NewGuid().ToByteArray();
            //Byte[] second = Guid.NewGuid().ToByteArray();
            //for (var i = 0; i < 16; i++)
            //    bytes[i] = first[i];
            //for (var i = 16; i < 32; i++)
            //    bytes[i] = second[i - 16];
            //return new String(Convert.ToBase64String(bytes, Base64FormattingOptions.None).ToCharArray().Where(Char.IsLetter).ToArray());
        }

        public static String Encode(String source)
        {
            Func<Char, String> encodeCharacter = c =>
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c == '.' || c == '-' || c == '_' || c == '~'))
                    return new String(c, 1);
                return HttpUtility.UrlEncode(c.ToString()).ToUpper();//EncodeCharacter(c);
            };
            return String.Concat(source.ToCharArray().Select(encodeCharacter));
        }

        public static String CalculateParameterString(Dictionary<String, String> parameters)
        {
            var q = from entry in parameters
                    let encodedKey = entry.Key
                    let encodedValue = entry.Value
                    let encodedEntry = encodedKey + "=" + encodedValue
                    orderby encodedEntry ascending
                    select encodedEntry;
            return String.Join("&", q.ToArray());
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
                Byte[] signedValue = myhmacsha1.ComputeHash(byteArray);//stream);
                return Convert.ToBase64String(signedValue);
            }
        }

        public static String GetOAuthRequestUrl(String consumerSecret, String consumerKey, 
            String uri, String httpMethod, 
            String tokenSecret = null, Dictionary<String, String> extraParameters = null)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("oauth_nonce", GenerateNonce());
            parameters.Add("oauth_consumer_key", consumerKey);
            parameters.Add("oauth_signature_method", "HMAC-SHA1");
            parameters.Add("oauth_timestamp", GenerateTimestamp());
            parameters.Add("oauth_version", "1.0");
            if(extraParameters != null)
                parameters.AddRange(extraParameters);
            ///theBody: sorted list of parameters
            ///  oauth_consumer_key=consumerKey
            ///  oauth_nonce=GenerateNonce()
            ///  oauth_signature_method=HMAC-SHA1
            ///  oauth_timestamp=GenerateTimestamp()
            ///  oauth_version=1.0
            String theBody = CalculateParameterString(parameters); 

            ///theHead: httpMethod + & + HttpUtility.UrlEncode(uri) + &
            String theHead = httpMethod.ToUpper() + "&" + Encode(uri) + "&";
            String sigBase = theHead + Encode(theBody);

            String signingKey = GetSigningKey(consumerSecret, tokenSecret);
            String signature = Sign(sigBase, signingKey);

            return uri + "?" + theBody + "&oauth_signature=" + Encode(signature);
        }

        public static String GetOAuthLoginUrl(String consumerKey, String token, String callback, String loginUrl,
            Dictionary<String, String> extraParameters)
        {
            String url = loginUrl;
            url += "?oauth_token=" + token;
            
            if (extraParameters != null)
                url += "&" + CalculateParameterString(extraParameters);

            url += "&oauth_callback=" + callback + "&oauth_consumer_key=" + consumerKey;

            return url;
        }
    }
}