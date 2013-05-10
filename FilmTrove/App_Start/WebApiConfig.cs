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
            var json = config.Formatters.JsonFormatter;

            json.SerializerSettings.PreserveReferencesHandling = 
                Newtonsoft.Json.PreserveReferencesHandling.Objects;
            
            config.Formatters.Remove(config.Formatters.XmlFormatter);
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
                name: "V1Lists",
                routeTemplate: "api/v1/lists/{action}/{id}",
                defaults: new { controller = "Lists", id = RouteParameter.Optional }//,
                //constraints: new { id = @"^\d+$" }
            );
            #endregion

            #region Movies
            config.Routes.MapHttpRoute(
                name: "V1MovieDetails",
                routeTemplate: "api/v1/movies/{action}/{id}",
                defaults: new { controller = "Movies",  id = RouteParameter.Optional, action = "details" },
                constraints: new { id = @"^\d+$" }
            );
            #endregion


            #region Search
            config.Routes.MapHttpRoute(
                name: "V1SearchNetflix",
                routeTemplate: "api/v1/search/netflix/{action}",
                defaults: new { controller = "NetflixSearch", action = "everything" }
            );
            config.Routes.MapHttpRoute(
                name: "V1SearchRotten",
                routeTemplate: "api/v1/search/rottentomatoes/{action}",
                defaults: new { controller = "RottenTomatoesSearch", action = "titles" }
            );
            config.Routes.MapHttpRoute(
                name: "V1Search",
                routeTemplate: "api/v1/search/{action}",
                defaults: new { controller = "Search", action = "titles" }
            );
            #endregion

            config.Routes.MapHttpRoute(
                name: "V1",
                routeTemplate: "api/v1/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { id = @"^\d+$" }
            );
        }
    }
}
