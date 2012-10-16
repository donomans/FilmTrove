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
        private String consumerkey;// = "7qf3845qydavuucmhj96b6hd";
        private String sharedsecret;// = "5jYe5FVhhF";
        private String applicationname;// = "FilmTrove";

        [HttpPost]
        public ActionResult Search(FormCollection form)
        {


            String term = form["search-form"].ToString();
            Netflix n = new Netflix(FilmTrove.Models.NetflixAccount.GetCurrentUserNetflixUserInfo);
            NetflixMovies titles = n.SearchTitle(term);
            
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
