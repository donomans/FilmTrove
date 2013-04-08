using FilmTrove.Code;
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


            //var BoxOffice = RottenTomatoes.Fill.Lists.GetBoxOffice();
            //var InTheaters = RottenTomatoes.Fill.Lists.GetInTheaters();
            var OpeningMovies = RottenTomatoes.Fill.Lists.GetOpeningMovies();
            var UpcomingMovies = RottenTomatoes.Fill.Lists.GetUpcomingMovies();

            //var CurrentReleaseDVDs = RottenTomatoes.Fill.Lists.GetCurrentReleaseDVDs();
            var NewReleaseDVDs = RottenTomatoes.Fill.Lists.GetNewReleaseDVDs();
            //var TopRentals = RottenTomatoes.Fill.Lists.GetTopRentals();
            var UpcomingDVDs = RottenTomatoes.Fill.Lists.GetUpcomingDVDs();

            ViewBag.NewReleases = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await NewReleaseDVDs);
            ViewBag.UpcomingReleases = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await UpcomingDVDs);
            ViewBag.OpeningMovies = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await OpeningMovies);
            ViewBag.UpcomingMovies = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await UpcomingMovies);

            return View();
        }
    }
}
