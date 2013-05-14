using FilmTrove.Models;
using FlixSharp.Holders;
using FlixSharp.Holders.RottenTomatoes;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FilmTrove.Code.RottenTomatoes
{
    public class RottenTomatoesHelpers
    {
        public static void FillRottenTomatoesTitle(FilmTrove.Models.Movie movie, FlixSharp.Holders.RottenTomatoes.Title rtitle)
        {
            movie.RottenTomatoes.Id = rtitle.Id;
            FillRottenTomatoesRatings(movie, rtitle);

            FillRottenTomatoesReleaseDates(movie, rtitle);

            FillRottenTomatoesPosters(movie, rtitle);

            movie.RottenTomatoes.Studio = rtitle.Studio;
            movie.RottenTomatoes.Url = rtitle.RottenTomatoesSiteUrl;

            var imdbid = rtitle.AlternateIds
                .FirstOrDefault(i => i.Type == AlternateIdType.Imdb);
            movie.Imdb.Id = imdbid != null ? imdbid.Id : "";

            ///consider not overwriting these if they have values???
            movie.RunTime = rtitle.RunTime * 60;
            movie.Rating = rtitle.Rating.ToString();
            movie.RatingType = RatingType.Mpaa;
            if (movie.Title == null || movie.AltTitle == "")
                movie.AltTitle = rtitle.FullTitle;
            if (movie.Title == null || movie.Title == "")
                movie.Title = rtitle.FullTitle;
            
            if (movie.Year < 1900)
                movie.Year = rtitle.Year;
            movie.RottenTomatoes.Year = rtitle.Year;
        }

        private static void FillRottenTomatoesRatings(FilmTrove.Models.Movie movie, Title rtitle)
        {
            var avgrating = rtitle.Ratings
                .FirstOrDefault(r => r.Type == RottenRatingType.Audience);
            if (avgrating != null)
                movie.RottenTomatoes.AvgRating = avgrating.Score;
            var criticrating = rtitle.Ratings
                .FirstOrDefault(r => r.Type == RottenRatingType.Critic);
            if (criticrating != null)
                movie.RottenTomatoes.CriticScore = criticrating.Score;

            movie.RottenTomatoes.CriticConsensus = rtitle.CriticsConsensus;
            movie.RottenTomatoes.Synopsis = rtitle.Synopsis;
        }

        private static void FillRottenTomatoesPosters(FilmTrove.Models.Movie movie, Title rtitle)
        {
            var largeposter = rtitle.Posters
                .FirstOrDefault(r => r.Type == PosterType.Detailed);
            if (largeposter != null)
                movie.RottenTomatoes.PosterUrlLarge = largeposter.Url;
            var mediumposter = rtitle.Posters
                .FirstOrDefault(r => r.Type == PosterType.Detailed);
            if (mediumposter != null)
                movie.RottenTomatoes.PosterUrlMedium = mediumposter.Url;

            //need to not overwrite a netflix url if the default rotten tomatoes image is all i have
            if (!movie.RottenTomatoes.PosterUrlLarge.EndsWith("poster_default.gif"))
                movie.BestPosterUrl = movie.RottenTomatoes.PosterUrlLarge;
        }

        private static void FillRottenTomatoesReleaseDates(FilmTrove.Models.Movie movie, Title rtitle)
        {
            var dvdrelease = rtitle.ReleaseDates
                .FirstOrDefault(r => r.ReleaseType == ReleaseDateType.DVD);
            if (dvdrelease != null)
                movie.RottenTomatoes.DvdRelease = dvdrelease.Date;
            var theatricalrelease = rtitle.ReleaseDates
                .FirstOrDefault(r => r.ReleaseType == ReleaseDateType.Theater);
            if (theatricalrelease != null)
                movie.RottenTomatoes.TheatricalRelase = theatricalrelease.Date;
        }

        public static void AddRottenTomatoesGenres(Movie movie, FilmTroveContext ftc, FlixSharp.Holders.RottenTomatoes.Title unmatched)
        {
            if (unmatched.Genres.Count > 0)
            {
                var dbgenreslocal = ftc.Genres.Local.Where(g => unmatched.Genres.Contains(g.Name));
                var dbgenres = ftc.Genres.Where(g => unmatched.Genres.Contains(g.Name));
                HashSet<Genre> genres = new HashSet<Genre>();
                genres.AddRange(dbgenres);
                genres.AddRange(dbgenreslocal);

                var genrenames = genres.Select(g => g.Name);
                var missinggenres = unmatched.Genres.Where(g => !genrenames.Contains(g));
                foreach (String genre in missinggenres)
                {
                    Genre g = new Genre() { Name = genre };
                    genres.Add(g);
                    ftc.Genres.Add(g);
                }
                //newmovie.Genres = netflixmovie.Genres;
                foreach (Genre g in genres)
                {
                    MovieGenre gi = ftc.GenreItems.Create();
                    gi.Genre = g;
                    gi.Movie = movie;
                    ftc.GenreItems.Add(gi);
                }
            }
        }

        public static async Task<Title> FindRottenTomatoesMatch(Movie m, MiniProfiler profiler = null)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                var searchtitles = await FlixSharp.RottenTomatoes.Search.SearchTitles(m.Title);

                return (Title)GeneralHelpers.FindTitleMatch(m, searchtitles, ftc, profiler);
            }
        }

       //public static async Task<Movie> FindRottenTomatoesMatch(ITitle t, FilmTroveContext ftc)
        //{
        //    var searchtitles = await FlixSharp.RottenTomatoes.Search.SearchTitles(t.FullTitle);
        //    Boolean pars = t.FullTitle.Contains("(") && t.FullTitle.Contains(")");
        //    Int32 maxlength = pars ? 200 : (Int32)(t.FullTitle.Length * 1.2);
        //    Int32 minlength = pars ? 0 : (Int32)(t.FullTitle.Length * .8);
        //    String tFullTitle = t.FullTitle.ToLower();

        //    var match = searchtitles
        //        .Select(mv => mv as FlixSharp.Holders.RottenTomatoes.Title)
        //        .FirstOrDefault(mv =>
        //        {
        //            String mvFullTitle = mv.FullTitle.ToLower();
        //            return ((mvFullTitle == tFullTitle)
        //            || (mvFullTitle.Contains(tFullTitle) && mvFullTitle.Length > minlength && mvFullTitle.Length < maxlength)
        //            || (tFullTitle.Contains(mv.FullTitle) && mvFullTitle.Length > minlength && mvFullTitle.Length < maxlength)
        //            && (mv.Year == t.Year
        //            || mv.Year + 1 == t.Year
        //            || mv.Year - 1 == t.Year));
        //        });
        //    if (match == null)
        //        throw new Exception("crap.");
        //    return ftc.Movies.Where(m => m.RottenTomatoes.Id == match.Id).FirstOrDefault();

            
        //}
    }
}