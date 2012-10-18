using FilmTrove.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlixSharp;
using FlixSharp.Holders;
using FilmTrove.Filters;

namespace FilmTrove.Controllers
{
    [InitializeSimpleMembership]
    public class MoviesController : Controller
    {
        [HttpPost]
        public ActionResult Search(FormCollection form)
        {
            String term = form["search-form"].ToString();
            Netflix n = new Netflix();
            SearchResults results = n.Search.Search(term).Result;

            ViewBag.SearchResults = results;

            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
