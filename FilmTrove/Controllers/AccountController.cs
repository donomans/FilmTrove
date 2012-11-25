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
using System.Net;
using FilmTrove.Code;
using FlixSharp;

namespace FilmTrove.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Me()
        {
            if (WebSecurity.IsAuthenticated)
            {
                FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
                UserProfile up = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
                if (up.NetflixAccount.UserId != null && up.NetflixAccount.UserId != "")
                    ViewBag.NetflixLinked = true;
                UserUpdate userupdate = new UserUpdate(up);

                return View(userupdate);

            }
            return View();
        }

        [HttpGet]
        public ActionResult NetflixLogin()
        {
            if (WebSecurity.IsAuthenticated)
            {
                String url = Netflix.Login.SetCredentials(///because of my Randomized() function, i need to reset this to be sure.
                "7qf3845qydavuucmhj96b6hd",
                "5jYe5FVhhF",
                "FilmTrove").GetRequestUrl();
                String oauthstuff = "";
                using (WebClient web = new WebClient())
                {
                    oauthstuff = web.DownloadString(url);
                }
                String oauth_token = "";
                String oauth_token_secret = "";
                
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
                            case "oauth_token_secret":
                                oauth_token_secret = s;
                                break;
                            default:
                                break;
                        }
                    }
                    counter++;
                }

                ///store it in the session and hope its still there?
                Session.Add("oauth_token_secret", oauth_token_secret);
                
                //String loginurl = CustomOAuthHelpers.GetOAuthLoginUrl(Netflix.ConsumerKey, oauth_token,
                //    Url.Action("NetflixLoginCallback","Account", null, Request.Url.Scheme),
                //    Netflix.LoginUrl, extraParams);

                String loginurl = Netflix.Login.SetCredentials(
                "7qf3845qydavuucmhj96b6hd",
                "5jYe5FVhhF",
                "FilmTrove").GetLoginUrl(oauth_token,
                    Url.Action("NetflixLoginCallback", 
                    "Account", null, Request.Url.Scheme));

                return new RedirectResult(loginurl);

            }
            else
            {
                ViewBag.NetflixLinked = null;
                ViewBag.Success = false;
                ViewBag.Message = "You must be logged in to link your Netflix account to FilmTrove.";
                return View();
            }
        }

        [HttpGet]
        public ActionResult NetflixLoginCallback(String oauth_token)
        {
            if (oauth_token != null && oauth_token != "")
            {
                Object oauth_token_secret = Session["oauth_token_secret"];
                if (oauth_token_secret == null)
                {
                    ViewBag.Success = false;
                    ViewBag.Message = "Well, this ended poorly.  Sorry.  Let's try again.";
                    return View();
                }
                //Netflix.Login n = new Netflix();
                String accessUrl = Netflix.Login.SetCredentials(
                "7qf3845qydavuucmhj96b6hd",
                "5jYe5FVhhF",
                "FilmTrove").GetAccessUrl(oauth_token, oauth_token_secret.ToString());

                String oauthstuff = "";
                using (WebClient web = new WebClient())
                {
                    oauthstuff = web.DownloadString(accessUrl);
                }
                String[] oauthsplits = oauthstuff.Split(new[] { "&", "=" }, StringSplitOptions.None);

                String new_oauth_token = "";
                String new_oauth_token_secret = "";
                String user_id = "";

                Int32 counter = 0;
                String stash = "";

                foreach (String s in oauthsplits)
                {
                    if (counter % 2 == 0)
                        stash = s;
                    else
                    {
                        switch (stash)
                        {
                            case "oauth_token":
                                new_oauth_token = s;
                                break;
                            case "user_id":
                                user_id = s;
                                break;
                            case "oauth_token_secret":
                                new_oauth_token_secret = s;
                                break;
                            default:
                                break;
                        }
                    }
                    counter++;
                }
                FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
                UserProfile up = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
                up.NetflixAccount.Token = new_oauth_token;
                up.NetflixAccount.TokenSecret = new_oauth_token_secret;
                up.NetflixAccount.UserId = user_id;

                ftc.SaveChanges();
                ViewBag.NetflixLinked = true;
                ViewBag.Success = true;
                ViewBag.Message = "Successfully linked your Netflix account";
                return View("Me");

            }
            else
            {
                ViewBag.NetflixLinked = null;
                ViewBag.Success = false;
                ViewBag.Message = "We screwed some part of the login process.  Sorry.  Let's try again.";
                return View();
            }
        }

        [HttpGet]
        public ActionResult NetflixUnlink()
        {
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];

            UserProfile up = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
            up.NetflixAccount.Token = "";
            up.NetflixAccount.TokenSecret = "";
            up.NetflixAccount.UserId = "";
            Int32 changed = ftc.SaveChanges();

            UserUpdate userupdate = new UserUpdate(up);

            ViewBag.NetflixLinked = null;
            return View("Me", userupdate);

        }

        [HttpPost]
        public ActionResult Me(UserUpdate changes)
        {
            ///update the data and return to previous url or something?
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            UserProfile up = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
            up.Name = changes.Name;
            up.Email = changes.Email;

            ftc.SaveChanges();

            return View("Me", changes);
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

                    FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
                    UserProfile user = ftc.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == result.ProviderUserId.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        UserProfile prof = new UserProfile() { UserName = result.ProviderUserId, Provider = result.Provider, NetflixAccount = new NetflixAccount() };
                        if (result.UserName.Contains("@"))
                            prof.Email = result.UserName;
                        else
                            prof.Name = result.UserName;

                        ftc.UserProfiles.Add(prof);
                        ftc.SaveChanges();

                        String provider;
                        String providerUserId;

                        OAuthWebSecurity.TryDeserializeProviderUserId(loginData, out provider, out providerUserId);
                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, result.ProviderUserId);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);
                    }
                    else
                    {
                        //need to handle some error
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }

                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
