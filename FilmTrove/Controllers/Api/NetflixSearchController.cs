using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using System.Web.Mvc;
using System.Web;
using System.Web.Caching;
using FilmTrove.App_Code;

namespace FilmTrove.Controllers.Api
{
    public class NetflixSearchController : ApiController
    {
        // Get api/netflixsearch
        const String SearchUrl = "http://api-public.netflix.com/catalog/titles/autocomplete?oauth_consumer_key=7qf3845qydavuucmhj96b6hd&term=";
        public IEnumerable<String> Get([FromUri] String term)
        {
            String url = SearchUrl + term;
            
            XDocument doc = XDocument.Load(url);
            
            var titles = from someelement
                                   in doc.Descendants("title")
                               select someelement.Attribute("short").Value;

            ///toss the full list into another service that will churn 
            ///through the records and populate the database.
            Object seedtitles = HttpContext.Current.Cache.Get("seedtitles");
            if (seedtitles != null &&  seedtitles is List<String>)
                ((List<String>)seedtitles).AddRange(titles);
            else
                seedtitles = new List<String>(titles);

            HttpContext.Current.Cache.Insert("seedtitles", seedtitles,
                null, DateTime.Now.AddMinutes(25), Cache.NoSlidingExpiration,
                CacheItemPriority.Default, SeedHelpers.SeederCacheCallback);

            return titles.Take(10);
        }
    }
}
