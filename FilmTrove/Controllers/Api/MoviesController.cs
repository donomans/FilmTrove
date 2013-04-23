using FilmTrove.Code;
using FilmTrove.Code.Netflix;
using FilmTrove.Code.RottenTomatoes;
using FilmTrove.Models;
using FlixSharp;
using FlixSharp.Holders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace FilmTrove.Controllers.Api
{
    public class MoviesController : ApiController
    {
        [HttpGet]
        public async Task<Movie> Details([FromUri] Int32 id)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                Movie m = ftc.Movies
                   .Include("Roles.Person").Include("Genres.Genre")
                   .Where(movie => movie.MovieId == id).Single();

                ///need to:
                ///1) Check if it needs to update netflix
                Task n =  UpdateNetflix(m, ftc);
                ///2) Check if it needs to update rotten tomatoes data
                Task rt = UpdateRottenTomatoes(m, ftc);
                ///3) Check if it needs to update amazon
                Task a = UpdateAmazon(m, ftc);

                await n;
                await rt;
                await a;

                ftc.SaveChanges();

                return m;
            }
        }

        [HttpGet]
        public List<Movie> Similars([FromUri] Int32 id)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                Movie m = ftc.Movies.Find(id);
                if (m.Netflix.SimilarTitles.Count > 0)
                {
                    var similars = m.Netflix.SimilarTitles.Take(20).Select(f =>
                    {
                        MatchCollection match = Regex.Matches(f, "[0-9]{3,10}");
                        var r = match.Cast<Match>().Select(t => t.Value).Take(2);
                        return r.First() + (r.Count() > 1 ? ";" + r.LastOrDefault() : "");
                    }).ToList();
                    return ftc.Movies.Where(t => similars.Contains(t.Netflix.Id)).ToList();
                }
                else
                    return null;
            }
        }

        private async Task UpdateNetflix(Movie m, FilmTroveContext ftc)
        {
            Random ran = new Random();

            Task<FlixSharp.Holders.Netflix.Title> nfm = null;
            FlixSharp.Holders.Netflix.Title netflixtitle = null;

            if (m.Netflix.NeedsUpdate || (m.DateLastModified.HasValue && m.DateLastModified > DateTime.Now.AddDays(20).AddDays(ran.Next(-5, 5))))
            {
                if (m.Netflix.IdUrl != "")
                    nfm = Netflix.Fill.Titles.GetCompleteTitle(m.Netflix.IdUrl);//Randomized().
                else
                {
                    netflixtitle = await NetflixHelpers.FindNetflixMatch(m);
                    nfm = Netflix.Fill.Titles.GetCompleteTitle(netflixtitle.IdUrl);
                }

                if (nfm != null)
                    netflixtitle = await nfm;

                NetflixHelpers.FillBasicNetflixTitle(m, netflixtitle);
                NetflixHelpers.FillAdvancedNetflix(m, netflixtitle);
                NetflixHelpers.FillNetflixRoles(m, ftc, netflixtitle);
                NetflixHelpers.FillNetflixSimilars(m, ftc, netflixtitle);
                NetflixHelpers.FillNetflixGenres(m, ftc, netflixtitle);

                m.Netflix.LastFullUpdate = DateTime.Now;
                m.Netflix.NeedsUpdate = false;
            }
            else
                await NetflixHelpers.CorrectNoneRoles(m, ftc);
        }

        
        private async Task UpdateRottenTomatoes(Movie m, FilmTroveContext ftc)
        {
            Random ran = new Random();

            Task<FlixSharp.Holders.RottenTomatoes.Title> rtm = null;
            FlixSharp.Holders.RottenTomatoes.Title rottentomatoestitle = null;

            if (m.RottenTomatoes.Id != "")
            {
                ///1) title match like with amazon or use Id if present
                rtm = FlixSharp.RottenTomatoes.Fill.Titles.GetMoviesInfo(m.RottenTomatoes.Id);
            }
            else
            {
                ////need to find the best match
                rottentomatoestitle = await RottenTomatoesHelpers.FindRottenTomatoesMatch(m);
                rtm = FlixSharp.RottenTomatoes.Fill.Titles.GetMoviesInfo(rottentomatoestitle.Id);
            }

            if (rtm != null || rottentomatoestitle != null)
            {
                if (rtm != null)
                    rottentomatoestitle = await rtm;
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
                RottenTomatoesHelpers.FillRottenTomatoesTitle(m, rottentomatoestitle);
                
                m.RottenTomatoes.LastFullUpdate = DateTime.Now;
                m.RottenTomatoes.NeedsUpdate = false;
            }
        }

        
        private async Task UpdateAmazon(Movie m, FilmTroveContext ftc)
        {
        }


        [HttpPost]
        public Int64 UpdateCount([FromUri] Int32 Id)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                Movie m = ftc.Movies.Find(Id);
                if (m != null)
                {
                    m.ViewCount++;
                    Int64 currentcount = m.ViewCount;
                    ftc.SaveChanges();
                    return currentcount;
                }
                return 0;
            }
        }
    }
}
