using FilmTrove.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlixSharp;
using FlixSharp.Holders;
using FilmTrove.Filters;
using System.Threading.Tasks;
using FilmTrove.Models;

namespace FilmTrove.Controllers
{
    [InitializeSimpleMembership]
    public class MoviesController : Controller
    {
        //[HttpPost]
        //public async Task<ActionResult> Search(FormCollection form)
        //{
        //    String term = form["searchterm"].ToString();
        //    Netflix n = new Netflix();
        //    SearchResults results = await n.Search.Search(term);
        //    ///need to check if the results are in the database and populate it if not
        //    ViewBag.MovieResults = await AsyncHelpers.GetDatabaseMovies(results.MovieResults);
        //    //ViewBag.PeopleResults = await AsyncHelpers.GetDatabasePeople(results.PeopleResults);
            
        //    //ViewBag.SearchResults = results;

        //    return View();
        //}


        [HttpGet]
        public ActionResult Details(String id, String title)
        {
            ViewBag.Id = id;
            return View();
        }
    }
}
