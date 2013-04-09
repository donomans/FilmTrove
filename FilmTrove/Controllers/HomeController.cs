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
using StackExchange.Profiling;


namespace FilmTrove.Controllers
{
    public class HomeController : Controller
    {   
        public async Task<ActionResult> Index()
        {
            var profiler = MiniProfiler.Current;
            using (profiler.Step("Index"))
            {
                FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
                ViewBag.Movies = (from m in ftc.Movies
                                  select m).Take(50).ToList();

                using (profiler.Step("Rotten Tomatoes Fill Lists setup"))
                {
                    //var BoxOffice = RottenTomatoes.Fill.Lists.GetBoxOffice();
                    //var InTheaters = RottenTomatoes.Fill.Lists.GetInTheaters();
                    var OpeningMovies = RottenTomatoes.Fill.Lists.GetOpeningMovies(Limit: 20);
                    var UpcomingMovies = RottenTomatoes.Fill.Lists.GetUpcomingMovies(Limit: 20);

                    //var CurrentReleaseDVDs = RottenTomatoes.Fill.Lists.GetCurrentReleaseDVDs();
                    var NewReleaseDVDs = RottenTomatoes.Fill.Lists.GetNewReleaseDVDs(Limit: 20);
                    //var TopRentals = RottenTomatoes.Fill.Lists.GetTopRentals();
                    var UpcomingDVDs = RottenTomatoes.Fill.Lists.GetUpcomingDVDs(Limit: 20);
                    using (profiler.Step("GetDatabaseMoviesRottenTomatoes and Awaits"))
                    {
                        ViewBag.NewReleases = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await NewReleaseDVDs, ftc);
                        ViewBag.UpcomingReleases = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await UpcomingDVDs, ftc);
                        ViewBag.OpeningMovies = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await OpeningMovies, ftc);
                        ViewBag.UpcomingMovies = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await UpcomingMovies, ftc);
                        ///cache these requests 
                        ///put list of 
                    }
                }
                return View();
            }
        }
    }
}
