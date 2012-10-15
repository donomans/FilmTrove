using FilmTrove.Models;
using System;
using System.Collections.Generic;
//using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FilmTrove.Controllers
{
    public class HomeController : Controller
    {
        private FilmTroveContext db = new FilmTroveContext();
        
        public ActionResult Index()
        {
            ViewBag.Movies = (from m in db.Movies
                              select m).Take(10);
            return View();
        }
    }
}
