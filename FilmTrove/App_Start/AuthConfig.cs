using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using FilmTrove.Models;
using DotNetOpenAuth.AspNet;
using System.Web;

namespace FilmTrove
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            OAuthWebSecurity.RegisterFacebookClient(
                appId: "265950490105673",
                appSecret: "a7caeb3736c59684d943d776b2d519a2");
          
            //OAuthWebSecurity.RegisterClient(new NetflixClient("7qf3845qydavuucmhj96b6hd", "5jYe5FVhhF"));
          
            OAuthWebSecurity.RegisterGoogleClient();

        }
    }

    
}
