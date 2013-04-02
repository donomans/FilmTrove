using FilmTrove.Models;
using FlixSharp;
using System;
using System.Collections.Generic;
//using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FilmTrove.Controllers
{
    public class HomeController : Controller
    {   
        public async Task<ActionResult> Index()
        {
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            ViewBag.Movies = (from m in ftc.Movies
                              select m).Take(50).ToList();

            var BoxOffice = await RottenTomatoes.Fill.Lists.GetBoxOffice();
            var InTheaters = await RottenTomatoes.Fill.Lists.GetInTheaters();
            var OpeningMovies = await RottenTomatoes.Fill.Lists.GetOpeningMovies();
            var UpcomingMovies = await RottenTomatoes.Fill.Lists.GetUpcomingMovies();

            var CurrentReleaseDVDs = await RottenTomatoes.Fill.Lists.GetCurrentReleaseDVDs();
            var NewReleaseDVDs = await RottenTomatoes.Fill.Lists.GetNewReleaseDVDs();
            var TopRentals = await RottenTomatoes.Fill.Lists.GetTopRentals();
            var UpcomingDVDs = await RottenTomatoes.Fill.Lists.GetUpcomingDVDs();
            return View();
        }
    }
}
