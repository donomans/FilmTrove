using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FilmTrove.Code.OAuthClients;

namespace FilmTrove.Code
{
    public class Netflix
    {
        public static class Login
        {
            public const String ConsumerKey = "7qf3845qydavuucmhj96b6hd";
            public const String SharedSecret = "5jYe5FVhhF";
            public const String RequestUrl = "http://api-public.netflix.com/oauth/request_token";
            public const String AccessUrl = "http://api-public.netflix.com/oauth/access_token";
            public const String LoginUrl = "https://api-user.netflix.com/oauth/login";
            public const String ApplicationName = "FilmTrove";

            public static String GetRequestUrl()
            {
                return CustomOAuthHelpers.GetOAuthRequestUrl(SharedSecret, ConsumerKey, RequestUrl, "GET");
            }

            public static String GetLoginUrl(String token, String callback, Dictionary<String, String> extraParams)
            {
                return CustomOAuthHelpers.GetOAuthLoginUrl(ConsumerKey, token, callback, LoginUrl, extraParams);
            }


            public static String GetAccessUrl(String token, String tokenSecret)
            {
                return CustomOAuthHelpers.GetOAuthAccessUrl(SharedSecret, ConsumerKey, AccessUrl, token, tokenSecret);
            }
        }
        

        public static NetflixMovies SearchTitle(String title, Int32 max = 10)
        {
            return null;
        }
    }
    
    public class NetflixMovies : IEnumerable<NetflixMovie>
    {
        List<NetflixMovie> _movies = new List<NetflixMovie>();

        public IEnumerator<NetflixMovie> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }


    public class NetflixMovie
    {
    }

    public class NetflixPerson
    {
    }
}