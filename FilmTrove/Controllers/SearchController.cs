using FilmTrove.Code;
using FilmTrove.Filters;
using FlixSharp;
using FlixSharp.Holders;
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
            //String term = form["searchterm"].ToString();
            //Netflix n = new Netflix();
            SearchResults results = await Netflix.Search.Search(searchterm);
            ///need to check if the results are in the database and populate it if not
            ViewBag.MovieResults = await AsyncHelpers.GetDatabaseMovies(results.MovieResults);
            ViewBag.PeopleResults = await AsyncHelpers.GetDatabasePeople(results.PeopleResults);

            //ViewBag.SearchResults = results;

            return View();
        }
        
    }
}
