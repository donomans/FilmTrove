using FilmTrove.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlixSharp;
using FlixSharp.Holders;

namespace FilmTrove.Controllers
{
    public class MoviesController : Controller
    {
        [HttpPost]
        public ActionResult Search(FormCollection form)
        {
            String term = form["search-form"].ToString();
            Netflix n = new Netflix(FilmTrove.Models.NetflixAccount.GetCurrentUserNetflixUserInfo);
            Movies titles = n.Search.SearchTitle(term);
            
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
