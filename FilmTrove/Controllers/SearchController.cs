using FilmTrove.Code;
using FilmTrove.Filters;
using FilmTrove.Models;
using FlixSharp;
using FlixSharp.Holders;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FilmTrove.Controllers
{
    [InitializeSimpleMembership]
    public class SearchController : Controller
    {
        [HttpPost]
        public async Task<ActionResult> Query(String searchterm)
        {
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            //String term = form["searchterm"].ToString();
            //Netflix n = new Netflix();
            var results = Netflix.Search.SearchEverything(searchterm, Limit: 30);//Randomized().
            //var ramindex = (RAMDirectory)HttpContext.Cache.Get("ftramindex"); 
            var rtresults = RottenTomatoes.Search.SearchTitles(searchterm, Limit: 30);
            ///need to check if the results are in the database and populate it if not

            var rtmovies = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(rtresults, ftc);
            var nfmovies = GeneralHelpers.GetDatabaseMoviesNetflix((await results).MovieResults, ftc);
            ViewBag.MovieResults = (await nfmovies).Uniques(await rtmovies);
            ViewBag.PeopleResults = GeneralHelpers.GetDatabasePeopleNetflix(results.Result.PeopleResults, ftc);

            //ViewBag.SearchResults = results;
            ViewBag.Term = searchterm;
            return View();
        }
        
    }
}