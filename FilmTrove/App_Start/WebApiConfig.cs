using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace FilmTrove
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //config.Routes.MapHttpRoute(
            //    name: "userservices",
            //    routeTemplate: "api/v1/UserServices/{action}/{type}/{value}",
            //    defaults: new
            //    {
            //        controller = "UserServices",
            //        action = "ListLinks",
            //        type = RouteParameter.Optional,
            //        value = RouteParameter.Optional
            //    }
            //);


            #region Lists
            config.Routes.MapHttpRoute(
                name: "V1ApiLists",
                routeTemplate: "api/v1/Lists/{action}/{id}",
                defaults: new { controller = "ListServices", id = RouteParameter.Optional },
                constraints: new { id = @"^\d+$" }
            );
            #endregion

            #region Movies
            config.Routes.MapHttpRoute(
                name: "V1Api",
                routeTemplate: "api/v1/movies/{action}/{id}",
                defaults: new { id = RouteParameter.Optional, action = "details" },
                constraints: new { id = @"^\d+$" }
            );
            #endregion


            #region Search
            config.Routes.MapHttpRoute(
                name: "V1Search",
                routeTemplate: "api/v1/search/netflix/{action}",
                defaults: new { action = "everything" }
            );
            config.Routes.MapHttpRoute(
                name: "V1Search",
                routeTemplate: "api/v1/search/rottentomatoes/{action}",
                defaults: new { action = "titles" }
            );
            config.Routes.MapHttpRoute(
                name: "V1Search",
                routeTemplate: "api/v1/search/{action}",
                defaults: new { action = "titles" }
            );
            #endregion

            config.Routes.MapHttpRoute(
                name: "V1Api",
                routeTemplate: "api/v1/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { id = @"^\d+$" }
            );
        }
    }
}
