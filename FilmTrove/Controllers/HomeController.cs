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
                    var openingmovies = (List<Movie>)HttpContext.Cache.Get("OpeningMovies");
                    var upcomingmovies = (List<Movie>)HttpContext.Cache.Get("UpcomingMovies");
                    var newreleasedvds = (List<Movie>)HttpContext.Cache.Get("NewReleaseDVDs");
                    var upcomingdvds = (List<Movie>)HttpContext.Cache.Get("UpcomingDVDs");
                    if (openingmovies == null || upcomingmovies == null || newreleasedvds == null || upcomingdvds == null) 
                    {
                        //var BoxOffice = RottenTomatoes.Fill.Lists.GetBoxOffice();
                        //var InTheaters = RottenTomatoes.Fill.Lists.GetInTheaters();
                        var OpeningMoviesTask = RottenTomatoes.Fill.Lists.GetOpeningMovies(Limit: 20);
                        var UpcomingMoviesTask = RottenTomatoes.Fill.Lists.GetUpcomingMovies(Limit: 20);

                        //var CurrentReleaseDVDs = RottenTomatoes.Fill.Lists.GetCurrentReleaseDVDs();
                        var NewReleaseDVDsTask = RottenTomatoes.Fill.Lists.GetNewReleaseDVDs(Limit: 20);
                        //var TopRentals = RottenTomatoes.Fill.Lists.GetTopRentals();
                        var UpcomingDVDsTask = RottenTomatoes.Fill.Lists.GetUpcomingDVDs(Limit: 20);
                        using (profiler.Step("GetDatabaseMoviesRottenTomatoes and Awaits"))
                        {
                            var NewReleaseDVDs = await GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await NewReleaseDVDsTask, ftc);
                            var UpcomingDVDs = await GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await UpcomingDVDsTask, ftc);
                            var OpeningMovies = await GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await OpeningMoviesTask, ftc);
                            var UpcomingMovies = await GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await UpcomingMoviesTask, ftc);
                            HttpContext.Cache.Insert("OpeningMovies", OpeningMovies, null, DateTime.Now.AddHours(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            HttpContext.Cache.Insert("NewReleaseDVDs", NewReleaseDVDs, null, DateTime.Now.AddHours(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            HttpContext.Cache.Insert("UpcomingDVDs", UpcomingDVDs, null, DateTime.Now.AddHours(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            HttpContext.Cache.Insert("UpcomingMovies", UpcomingMovies, null, DateTime.Now.AddHours(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            ViewBag.NewReleaseDVDs = NewReleaseDVDs;
                            ViewBag.UpcomingDVDs = UpcomingDVDs;
                            ViewBag.OpeningMovies = OpeningMovies;
                            ViewBag.UpcomingMovies = UpcomingMovies;
                        }
                    }
                    else
                    {
                        //var openingmovies = (List<Movie>)HttpContext.Cache.Get("OpeningMovies");
                        ViewBag.NewReleaseDVDs = newreleasedvds;
                        ViewBag.UpcomingDVDs = upcomingdvds;
                        ViewBag.OpeningMovies = openingmovies;
                        ViewBag.UpcomingMovies = upcomingmovies;
                    }
                }
                return View();
            }
        }
    }
}
