using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace FilmTrove.Code.OAuthClients
{
    public class CustomOAuthHelpers
    {
        public static String GenerateTimestamp()
        {
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString("F0");
        }
        public static String GenerateNonce()
        {
            Int64 nonce = 0;
            Random r = new Random();
            for (Int32 i = 0; i < 15; i++)
                nonce += (nonce << i) * r.Next(0, 9) + i;
            return Math.Abs(nonce).ToString("F0");
        }
        public static String Encode(String source)
        {
            Func<Char, String> encodeCharacter = c =>
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c == '.' || c == '-' || c == '_' || c == '~'))
                    return new String(c, 1);
                return HttpUtility.UrlEncode(c.ToString()).ToUpper();
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
            String uri, String httpMethod, String tokenSecret = null,
            Dictionary<String, String> extraParameters = null)
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

            url += "&oauth_callback=" + Encode(callback) + "&oauth_consumer_key=" + consumerKey;

            return url;
        }

        public static String GetOAuthAccessUrl(String consumerSecret, String consumerKey, 
            String uri, String token, String tokenSecret)
        {
            Dictionary<String, String> extraParams = new Dictionary<String, String>();
            extraParams.Add("oauth_token", token);

            return GetOAuthRequestUrl(consumerSecret, consumerKey, uri, "GET", tokenSecret, extraParams);
        }
    }
}