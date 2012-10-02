using DotNetOpenAuth.AspNet;
using FilmTrove.Models;
using Microsoft.Web.WebPages.OAuth;
using FilmTrove.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FilmTrove.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        public ActionResult Me()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult GoogleLogin()
        {
            OAuthWebSecurity.RequestAuthentication("Google", Url.Action("GoogleLoginCallBack"));
            return View();
        }

        [AllowAnonymous]
        public ActionResult GoogleLoginCallBack()
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("GoogleLoginCallBack"));

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

                    using (MoviesContext db = new MoviesContext())
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
