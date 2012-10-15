using FilmTrove.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FilmTrove.Controllers
{
    public class MoviesController : Controller
    {
        [HttpPost]
        public ActionResult Search(FormCollection form)
        {
            String term = form["search-form"].ToString();

            NetflixMovies titles = Netflix.SearchTitle(term);

            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
