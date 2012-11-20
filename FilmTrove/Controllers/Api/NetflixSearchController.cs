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
            IEnumerable<String> titles = await Netflix.Search.Randomized().AutoCompleteTitle(term, 50);

            return titles.Take(10);
        }
    }
}
