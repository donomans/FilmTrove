﻿using FilmTrove.Code;
using FilmTrove.Filters;
using FilmTrove.Models;
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
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            //String term = form["searchterm"].ToString();
            //Netflix n = new Netflix();
            SearchResults results = await Netflix.Search.SearchEverything(searchterm, Limit: 40);//Randomized().

            Titles rtresults = await RottenTomatoes.Search.SearchTitles(searchterm);
            ///need to check if the results are in the database and populate it if not
            ViewBag.MovieResults = GeneralHelpers.GetDatabaseMoviesNetflix(results.MovieResults, ftc);
            ViewBag.PeopleResults = GeneralHelpers.GetDatabasePeopleNetflix(results.PeopleResults, ftc);

            //ViewBag.SearchResults = results;
            ViewBag.Term = searchterm;
            return View();
        }
        
    }
}
