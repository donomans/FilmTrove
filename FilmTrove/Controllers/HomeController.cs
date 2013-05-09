﻿using FilmTrove.Code;
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
using Lucene.Net.Store;


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
                        var NewReleaseDVDsTask = RottenTomatoes.Fill.Lists.GetNewReleaseDVDs(Limit: 30);
                        var OpeningMoviesTask = RottenTomatoes.Fill.Lists.GetOpeningMovies(Limit: 30);
                        var UpcomingDVDsTask = RottenTomatoes.Fill.Lists.GetUpcomingDVDs(Limit: 30);
                        var UpcomingMoviesTask = RottenTomatoes.Fill.Lists.GetUpcomingMovies(Limit: 30);
                        
                        using (profiler.Step("GetDatabaseMoviesRottenTomatoes and Awaits"))
                        {
                            //var ramindex = (RAMDirectory)HttpContext.Cache.Get("ftramindex"); 
                            var NewReleaseDVDs = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await NewReleaseDVDsTask, ftc, profiler);
                            var UpcomingDVDs = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await UpcomingDVDsTask, ftc, profiler);
                            var OpeningMovies = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await OpeningMoviesTask, ftc, profiler);
                            var UpcomingMovies = GeneralHelpers.GetDatabaseMoviesRottenTomatoes(await UpcomingMoviesTask, ftc, profiler);
                            HttpContext.Cache.Insert("OpeningMovies", await OpeningMovies, 
                                null, DateTime.Now.AddHours(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            HttpContext.Cache.Insert("NewReleaseDVDs", await NewReleaseDVDs, 
                                null, DateTime.Now.AddHours(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            HttpContext.Cache.Insert("UpcomingDVDs", await UpcomingDVDs, 
                                null, DateTime.Now.AddHours(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            HttpContext.Cache.Insert("UpcomingMovies", await UpcomingMovies, 
                                null, DateTime.Now.AddHours(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            ViewBag.NewReleaseDVDs = NewReleaseDVDs.Result;
                            ViewBag.UpcomingDVDs = UpcomingDVDs.Result;
                            ViewBag.OpeningMovies = OpeningMovies.Result;
                            ViewBag.UpcomingMovies = UpcomingMovies.Result;
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
