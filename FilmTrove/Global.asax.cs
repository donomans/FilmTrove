﻿using FlixSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FilmTrove
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            Netflix.Login.SetCredentials(
                "7qf3845qydavuucmhj96b6hd",
                "5jYe5FVhhF",
                "FilmTrove");
            
            Netflix.SetMethodForGettingCurrentUserAccount(FilmTrove.Models.NetflixAccount.GetCurrentUserNetflixUserInfo);
        }
    }
}