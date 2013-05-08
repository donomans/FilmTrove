using FilmTrove.Code;
using FilmTrove.Models;
using FlixSharp;
using FlixSharp.Holders;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FilmTrove.Controllers.Api
{
    public class SearchController : ApiController
    {
        public async Task<List<Movie>> Titles([FromUri] String term, [FromUri] Int32 limit = 10)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                var nfresults = Netflix.Search.SearchTitles(term, limit);

                var rtresults = RottenTomatoes.Search.SearchTitles(term, limit);
                //var ramindex = (RAMDirectory)HttpContext.Current.Cache.Get("ftramindex"); 
                ///need to check if the results are in the database and populate it if not
                List<Movie> rtmovies = await GeneralHelpers.GetDatabaseMoviesRottenTomatoes(rtresults, ftc);
                List<Movie> nfmovies = await GeneralHelpers.GetDatabaseMoviesNetflix(await nfresults, ftc);
                return nfmovies.Uniques(rtmovies);
            }
        }

        public async Task<List<Person>> People([FromUri] String name, [FromUri] Int32 limit = 10)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                People nfresults = await Netflix.Search.SearchPeople(name, limit);
                return GeneralHelpers.GetDatabasePeopleNetflix(nfresults, ftc);
            }
        }
    }

    public class NetflixSearchController : ApiController
    {
        [HttpGet]
        public async Task<IEnumerable<String>> AutoComplete([FromUri] String term, [FromUri] Int32 limit = 10)
        {
            return await Netflix.Search.AutoCompleteTitle(term, limit);
        }

        [HttpGet]
        public async Task<Titles> Titles([FromUri] String term, [FromUri] Int32 limit = 10)
        {
            return await Netflix.Search.SearchTitles(term, limit);
        }

        [HttpGet]
        public async Task<People> People([FromUri] String name, [FromUri] Int32 limit = 10)
        {
            return await Netflix.Search.SearchPeople(name, limit);
        }

        [HttpGet]
        public async Task<SearchResults> Everything([FromUri] String term, [FromUri] Int32 limit = 10)
        {
            return await Netflix.Search.SearchEverything(term, limit);
        }
    }


    public class RottenTomatoesSearchController : ApiController
    {
        [HttpGet]
        public async Task<Titles> Titles([FromUri] String term, [FromUri] Int32 limit = 10)
        {
            return await RottenTomatoes.Search.SearchTitles(term, limit);
        }
    }
}
