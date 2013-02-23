using FilmTrove.Models;
using System;
using System.Collections.Generic;
//using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace FilmTrove.Controllers
{
    public class HomeController : Controller
    {   
        public ActionResult Index()
        {
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            ViewBag.Movies = (from m in ftc.Movies
                              select m).Take(50).ToList();
            return View();
        }
    }
}
