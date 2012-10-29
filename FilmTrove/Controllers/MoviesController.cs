﻿using FilmTrove.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlixSharp;
using FlixSharp.Holders;
using FilmTrove.Filters;
using System.Threading.Tasks;
using FilmTrove.Models;

namespace FilmTrove.Controllers
{
    [InitializeSimpleMembership]
    public class MoviesController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Details(String id, String title)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                FilmTrove.Models.Movie m = ftc.Movies.Find(Int32.Parse(id));
                Task<FlixSharp.Holders.Title> nfm = null;

                if (m.Netflix.NeedsUpdate)
                {
                    nfm = FillNetflix(m.Netflix.Url);
                }
                if (m.Amazon.NeedsUpdate || (m.Amazon.LastPriceUpdate.HasValue && ((m.Amazon.LastPriceUpdate.Value - DateTime.Now) > new TimeSpan(1, 0, 0, 0))))
                {
                    if(m.Amazon.Id == null || m.Amazon.Id == "" || 
                        m.Amazon.LastFullUpdate.HasValue && ((m.Amazon.LastFullUpdate.Value - DateTime.Now) > new TimeSpan(7, 0, 0, 0)))
                    {
                        ///1) do query to match up title and fill in id with priority to blu-ray
                    }
                        ///1) else use Id
                        
                    ///2) fill in found id
                    ///3) purchase url (prioritize blu-ray)
                    ///4) price
                    ///5) update lastpriceupdate
                }
                if (m.Imdb.NeedsUpdate)
                {
                    ///nothing
                }
                if (m.RottenTomatoes.NeedsUpdate ||
                    (m.RottenTomatoes.LastFullUpdate.HasValue && ((m.Amazon.LastFullUpdate.Value - DateTime.Now) > new TimeSpan(7, 0, 0, 0))))
                {
                    if (m.RottenTomatoes.Id == null || m.RottenTomatoes.Id == "" ||
                        m.RottenTomatoes.LastFullUpdate.HasValue && ((m.RottenTomatoes.LastFullUpdate.Value - DateTime.Now) > new TimeSpan(7, 0, 0, 0)))
                    {
                        ///1) title match like with amazon or use Id if present
                    }
                    ///1.5) loop through the cast and match them with the current cast to get the role name filled.
                    ///2) Critic score
                    ///3) critic consensus
                    ///4) poster medium
                    ///5) poster large
                    ///6) theatrical release
                    ///7) dvd release
                    ///8) average rating
                    ///9) studio
                    ///10) synopsis
                }

                if (nfm != null)
                {
                    var netflixmovie = await nfm;
                    ///populate all the netflix information
                    ///1) find the cast and loop through and add the people to the database in a similar manner as AsyncHelpers.GetDatabasePeople
                    ///2) add this movie as a new role on the movie
                    ///3) add all similar titles to the database similar to step 1 for people
                }
                ViewBag.Movie = m;
            }
            ViewBag.Id = id;
            return View();
        }

        private static async Task<FlixSharp.Holders.Title> FillNetflix(String id)
        {
            Netflix n = new Netflix();
            FlixSharp.Holders.Title nfm = await n.Fill.GetBaseTitle(id);
            
            ///4) get synopsis
            var synopsis = n.Fill.GetSynopsis(id);
            
            ///6) get similar titles (add those to database in basic format, similar to AsyncHelpers.GetDatabaseMovies
            var similartitles = n.Fill.GetSimilarTitles(id, 20);
            
            ///8) get awards
            var awards = n.Fill.GetAwards(id);

            ///9) screen format / title format
            var screenformats = n.Fill.GetScreenFormats(id);

            ///10) format availability
            var formatavailability = n.Fill.GetFormatAvailability(id);

            ///12) Actors
            var actors = n.Fill.GetActors(id);
            
            ///13) Directors
            var directors = n.Fill.GetDirectors(id);


            nfm.Synopsis = await synopsis;
            nfm.SimilarTitles = await similartitles;
            nfm.Awards = await awards;
            nfm.ScreenFormats = await screenformats;
            nfm.Formats = await formatavailability;
            nfm.Actors = await actors;
            nfm.Directors = await directors;
            return nfm;
        }
    }
}
