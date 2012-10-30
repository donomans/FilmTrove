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
using System.Threading.Tasks;
using FlixSharp;
using FilmTrove.Code;

namespace FilmTrove.Controllers.Api
{
    public class NetflixSearchController : ApiController
    {
        // Get api/netflixsearch
        public async Task<IEnumerable<String>> Get([FromUri] String term)
        {
            //Netflix n = new Netflix();
            IEnumerable<String> titles = await Netflix.Search.AutoCompleteTitle(term, 50);

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
