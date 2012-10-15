using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FilmTrove
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Account", ///AccountController
                url: "Me/{action}",
                defaults: new { controller = "Account", action = "Me" });

            routes.MapRoute(
                name: "Home",
                url: "",
                defaults: new { controller = "Home", action = "Index" });

            routes.MapRoute(
                name: "Movies",
                url: "Movies/{action}/{id}",
                defaults: new { controller = "Movies", action = "Index", id = UrlParameter.Optional});
        }
    }
}