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


            config.Routes.MapHttpRoute(
                name: "V1ApiLists",
                routeTemplate: "api/v1/Lists/{action}/{id}",
                defaults: new { controller = "ListServices", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "V1Api",
                routeTemplate: "api/v1/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
