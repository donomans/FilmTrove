using FilmTrove.Models;
using FlixSharp.Holders;
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
                .FirstOrDefault(i => i.Type == FlixSharp.Holders.RottenTomatoes.AlternateIdType.Imdb);
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

        private static void FillRottenTomatoesRatings(FilmTrove.Models.Movie movie, FlixSharp.Holders.RottenTomatoes.Title rtitle)
        {
            var avgrating = rtitle.Ratings
                .FirstOrDefault(r => r.Type == FlixSharp.Holders.RottenTomatoes.RottenRatingType.Audience);
            if (avgrating != null)
                movie.RottenTomatoes.AvgRating = avgrating.Score;
            var criticrating = rtitle.Ratings
                .FirstOrDefault(r => r.Type == FlixSharp.Holders.RottenTomatoes.RottenRatingType.Critic);
            if (criticrating != null)
                movie.RottenTomatoes.CriticScore = criticrating.Score;

            movie.RottenTomatoes.CriticConsensus = rtitle.CriticsConsensus;
            movie.RottenTomatoes.Synopsis = rtitle.Synopsis;
        }

        private static void FillRottenTomatoesPosters(FilmTrove.Models.Movie movie, FlixSharp.Holders.RottenTomatoes.Title rtitle)
        {
            var largeposter = rtitle.Posters
                .FirstOrDefault(r => r.Type == FlixSharp.Holders.RottenTomatoes.PosterType.Detailed);
            if (largeposter != null)
                movie.RottenTomatoes.PosterUrlLarge = largeposter.Url;
            var mediumposter = rtitle.Posters
                .FirstOrDefault(r => r.Type == FlixSharp.Holders.RottenTomatoes.PosterType.Detailed);
            if (mediumposter != null)
                movie.RottenTomatoes.PosterUrlMedium = mediumposter.Url;
        }

        private static void FillRottenTomatoesReleaseDates(FilmTrove.Models.Movie movie, FlixSharp.Holders.RottenTomatoes.Title rtitle)
        {
            var dvdrelease = rtitle.ReleaseDates
                .FirstOrDefault(r => r.ReleaseType == FlixSharp.Holders.RottenTomatoes.ReleaseDateType.DVD);
            if (dvdrelease != null)
                movie.RottenTomatoes.DvdRelease = dvdrelease.Date;
            var theatricalrelease = rtitle.ReleaseDates
                .FirstOrDefault(r => r.ReleaseType == FlixSharp.Holders.RottenTomatoes.ReleaseDateType.Theater);
            if (theatricalrelease != null)
                movie.RottenTomatoes.TheatricalRelase = theatricalrelease.Date;

            //need to not overwrite a netflix url if the default rotten tomatoes image is all i have
            if (!movie.RottenTomatoes.PosterUrlLarge.EndsWith("poster_default.gif"))
                movie.BestPosterUrl = movie.RottenTomatoes.PosterUrlLarge;
        }

        public static async Task<FlixSharp.Holders.RottenTomatoes.Title> FindRottenTomatoesMatch(Movie m)
        {
            var searchtitles = await FlixSharp.RottenTomatoes.Search.SearchTitles(m.Title);

            return searchtitles
                .Select(mv => mv as FlixSharp.Holders.RottenTomatoes.Title)
                .FirstOrDefault(mv =>
                {
                    Int32 maxlength = (Int32)(m.Title.Length * 1.2);
                    Int32 minlength = (Int32)(m.Title.Length * .8);
                    return ((mv.FullTitle == m.AltTitle && mv.FullTitle.Length > minlength && mv.FullTitle.Length < maxlength)
                    || (mv.FullTitle == m.Title && mv.FullTitle.Length > minlength && mv.FullTitle.Length < maxlength)
                    || (mv.FullTitle.Contains(m.Title) && mv.FullTitle.Length > minlength && mv.FullTitle.Length < maxlength)
                    || (m.Title.Contains(mv.FullTitle) && mv.FullTitle.Length > minlength && mv.FullTitle.Length < maxlength)
                    || (m.AltTitle.Contains(mv.FullTitle) && mv.FullTitle.Length > minlength && mv.FullTitle.Length < maxlength))
                    && (mv.Year == m.Year
                    || mv.Year + 1 == m.Year
                    || mv.Year - 1 == m.Year);
                });
        }
    }
}