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

            #region Movies
            routes.MapRoute(
                name: "Movies",
                url: "Movies/{action}/{id}/{title}",
                defaults: new { controller = "Movies", action = "Details", id = UrlParameter.Optional, title= UrlParameter.Optional});
            routes.MapRoute(
                name: "People",
                url: "People/{action}/{id}/{title}",
                defaults: new { controller = "People", action = "Details", id = UrlParameter.Optional, title = UrlParameter.Optional });
            #endregion

            #region Search
            routes.MapRoute(
                name: "Search",
                url: "Search/{action}/{searchterm}",
                defaults: new { controller = "Search", action = "Query", searchterm = UrlParameter.Optional});
            #endregion

            #region Lists
            routes.MapRoute(
                name: "Collection",
                url: "Lists/Collection/{id}/{title}",
                defaults: new { controller = "Lists", action = "Collection", id = UrlParameter.Optional, title = UrlParameter.Optional });
            routes.MapRoute(
                name: "List Links",
                url: "Lists/Links/{id}/{title}",
                defaults: new { controller = "Lists", action = "Links", id = UrlParameter.Optional, title = UrlParameter.Optional });
            //routes.MapRoute(
            //    name: "Lists",
            //    url: "Lists/Add/Title/{movieid}/{movietitle}/{listid}/{listtitle}",
            //    defaults: new { controller = "Lists", list = UrlParameter.Optional, action = "Add", id = UrlParameter.Optional, title = UrlParameter.Optional });
            routes.MapRoute(
                name: "Lists",
                url: "Lists/{action}/{list}/{id}/{title}",
                defaults: new { controller = "Lists", list = UrlParameter.Optional, action = UrlParameter.Optional, id = UrlParameter.Optional, title = UrlParameter.Optional });
            #endregion
        }
    }
}