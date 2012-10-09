using DotNetOpenAuth.AspNet;
using FilmTrove.Models;
using Microsoft.Web.WebPages.OAuth;
using FilmTrove.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using FilmTrove.OAuthClients;
using System.Net;

namespace FilmTrove.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        public const String ConsumerKey = "7qf3845qydavuucmhj96b6hd";
        public const String SharedSecret = "5jYe5FVhhF";
        public const String RequestUrl = "http://api-public.netflix.com/oauth/request_token";
        public const String AccessUrl = "http://api-public.netflix.com/oauth/access_token";
        public const String LoginUrl = "https://api-user.netflix.com/oauth/login";
        public const String ApplicationName = "FilmTrove";

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Me()
        {
            if (WebSecurity.IsAuthenticated)
            {
                using (FilmTroveContext ftc = new FilmTroveContext())
                {
                    UserProfile up = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
                    if (up.NetflixAccount.UserId != null && up.NetflixAccount.UserId != "")
                        ViewBag.NetflixLinked = true;
                }
                ///do something?
            }
            else
            {
                ///display some alert saying to log in.
            }
            return View();
        }

        [HttpGet]
        public ActionResult NetflixLogin()
        {
            if (WebSecurity.IsAuthenticated)
            {
                String url = CustomOAuthHelpers.GetOAuthRequestUrl(SharedSecret, ConsumerKey, RequestUrl, "GET");
                String oauthstuff = "";
                using (WebClient web = new WebClient())
                {
                    oauthstuff = web.DownloadString(url);
                }
                String oauth_token = "";
                //String oauth_token_secret = "";
                //String application_name = "";
                String login_url = "";
                
                String[] oauthsplits = oauthstuff.Split(new[]{ "&", "="}, StringSplitOptions.None);
                Int32 counter = 0;
                String stash = "";
                
                foreach(String s in oauthsplits)
                {
                    if(counter % 2 == 0)
                    {
                        stash = s;
                    }
                    else
                    {
                        switch(stash)
                        {
                            case "oauth_token":
                                oauth_token = s;
                                break;
                            //case "application_name":
                            //    application_name = s;
                            //    break;
                            //case "login_url":
                            //    login_url = s;
                            //    break;
                            //case "oauth_token_secret":
                            //    oauth_token_secret = s;
                            //    break;
                            default:
                                break;
                        }
                    }
                    counter++;
                }
                
                Dictionary<String,String> extraParams = new Dictionary<String,String>();
                extraParams.Add("application_name", ApplicationName);
                String loginurl = CustomOAuthHelpers.GetOAuthLoginUrl(ConsumerKey, oauth_token,
                    Url.Action("NetflixLoginCallback", "Account", null, "http", "filmtrove.azurewebsites.net"),
                    LoginUrl, extraParams);

                return new RedirectResult(loginurl);

            }
            else
            {
                ViewBag.Success = false;
                ViewBag.Message = "You must be logged in to link your Netflix account to FilmTrove.";
                return View();
            }
        }

        [HttpPost]
        public ActionResult NetflixLoginCallback(String oauth_token)
        {
            if (oauth_token != null && oauth_token != "")
            {
                String accessUrl = CustomOAuthHelpers.GetOAuthRequestUrl(SharedSecret, ConsumerKey, AccessUrl,
                    "GET", oauth_token);

                String oauthstuff = "";
                using (WebClient web = new WebClient())
                {
                    oauthstuff = web.DownloadString(accessUrl);
                }
                ///oauth_token=YourAuthorizedOauthToken&user_id=YourSubscriberId&oauth_token_secret=YourAuthorizedTokenSecret
                String[] oauthsplits = oauthstuff.Split(new []{"&", "="}, StringSplitOptions.None);
                    
                String new_oauth_token = "";
                String oauth_token_secret = "";
                String user_id = "";
                
                Int32 counter = 0;
                String stash = "";
                
                foreach(String s in oauthsplits)
                {
                    if(counter % 2 == 0)
                    {
                        stash = s;
                    }
                    else
                    {
                        switch(stash)
                        {
                            case "oauth_token":
                                new_oauth_token = s;
                                break;
                            case "user_id":
                                user_id = s;
                                break;
                            case "oauth_token_secret":
                                oauth_token_secret = s;
                                break;
                            default:
                                break;
                        }
                    }
                    counter++;
                }
                using(FilmTroveContext ftc = new FilmTroveContext())
                {
                    UserProfile up = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
                    up.NetflixAccount.Token = new_oauth_token;
                    up.NetflixAccount.TokenSecret = oauth_token_secret;
                    up.NetflixAccount.UserId = user_id;

                    ftc.SaveChanges();

                    ViewBag.Success = true;
                    ViewBag.Message = "Successfully linked your Netflix account";
                    return View();
                }
            }
            else
            {
                ViewBag.Success = false;
                ViewBag.Message = "Some part of the login in process failed.  Let's try again.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Me(UserProfile profile)
        {
            ///update the data and return to previous url or something?
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login()
        {
            ///default to Google - 
            ///MVC has some inherent desire to hit the Login action when you use the [Authorize] attribute
            return GoogleLogin();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult GoogleLogin()
        {
            OAuthWebSecurity.RequestAuthentication("Google", Url.Action("LoginCallBack"));
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult FacebookLogin()
        {
            OAuthWebSecurity.RequestAuthentication("Facebook", Url.Action("LoginCallBack"));
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            WebSecurity.Logout();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult LoginCallBack()
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("LoginCallBack"));

            if (result.IsSuccessful)
            {
                if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: true))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                    ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;

                    using (FilmTroveContext db = new FilmTroveContext())
                    {
                        UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == result.UserName.ToLower());
                        // Check if user already exists
                        if (user == null)
                        {
                            // Insert name into the profile table
                            db.UserProfiles.Add(new UserProfile { UserName = result.UserName, Provider = result.Provider });
                            db.SaveChanges();

                            String provider;
                            String providerUserId;

                            OAuthWebSecurity.TryDeserializeProviderUserId(loginData, out provider, out providerUserId);

                            OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, result.UserName);
                            OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        }
                        else
                        {
                            //need to handle some error
                            ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                        }
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
