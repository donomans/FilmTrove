using FilmTrove.Models;
using FlixSharp.Holders;
using FlixSharp.Holders.Netflix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FilmTrove.Code
{
    public class GeneralHelpers
    {
        #region Netflix
        public static List<Models.Movie> GetDatabaseMoviesNetflix(Titles results)
        {
            var netflixids = results.Select((m) => (m as FlixSharp.Holders.Netflix.Title).FullId);

            using (FilmTroveContext ftc = (FilmTroveContext)HttpContext.Current.Items["ftcontext"])
            {

                ///1) find the matching records from the database
                var matchedmovies = ftc.Movies.Where(m => netflixids.Contains(m.Netflix.Id));
                ///2) find the records that don't have a match
                ///select the ids and get the netflix Ids that aren't in the FT database
                var ftnfids = matchedmovies.Select(m => m.Netflix.Id).ToList();
                var netflixidsunmatched = netflixids.Where(m => !ftnfids.Contains(m)).ToList();
                //Int32 count = 0;
                var unmatchedresults = results
                    .Select(m => (m as FlixSharp.Holders.Netflix.Title))
                    .Where(m => netflixidsunmatched.Contains(m.FullId))
                    .ToList();

                foreach (var unmatched in unmatchedresults)
                {
                    //String nid = unmatched.FullId;
                    ///check for FT record incase it was added using a different data source (like RT)
                    Models.Movie movie = GetExistingMovie(unmatched);
                    ///create FT database records for each of these with the movies basic information for now
                    if(movie == null)
                        movie = ftc.Movies.Create();
                    FlixSharp.Holders.Netflix.Title netflixmovie = results.Find(movie.Netflix.Id) as FlixSharp.Holders.Netflix.Title;
                    FillBasicNetflixTitle(movie, netflixmovie);
                    
                    var dbgenreslocal = ftc.Genres.Local.Where(g => netflixmovie.Genres.Contains(g.Name));
                    var dbgenres = ftc.Genres.Where(g => netflixmovie.Genres.Contains(g.Name));
                    HashSet<Genre> genres = new HashSet<Genre>();
                    genres.AddRange(dbgenres);
                    genres.AddRange(dbgenreslocal);
                    
                    var genrenames = genres.Select(g => g.Name);
                    var missinggenres = netflixmovie.Genres.Where(g => !genrenames.Contains(g));
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
                    ftc.Movies.Add(movie);
                }

                ftc.SaveChanges();
                
                if (matchedmovies.Count() < results.Count())
                    matchedmovies = ftc.Movies.Where(m => netflixids.Contains(m.Netflix.Id));

                return results.Select(m => 
                    matchedmovies.First(f =>
                        f.Netflix.Id == (m.Id + ((m as FlixSharp.Holders.Netflix.Title).SeasonId != "" ? ";" + (m as FlixSharp.Holders.Netflix.Title).SeasonId : "")))).ToList();
            }
        }
        public static List<Models.Person> GetDatabasePeopleNetflix(People results)
        {
            var netflixids = results.Select(p => p.Id);
            using (FilmTroveContext ftc = (FilmTroveContext)HttpContext.Current.Items["ftcontext"])
            {
                ///1) find the matching records from the database
                var matchedpeople = ftc.People.Where(m => netflixids.Contains(m.Netflix.Id));
                ///2) find the records that don't have a match
                ///select the ids and get the netflix Ids that aren't in the FT database
                var ftnfids = matchedpeople.Select(m => m.Netflix.Id).ToList();
                var netflixidsunmatched = netflixids.Where(m => !ftnfids.Contains(m)).ToList();
                //Int32 count = 0;
                foreach (String nid in netflixidsunmatched)
                {
                    ///create FT database records for each of these with the movies basic information for now
                    FilmTrove.Models.Person newperson = ftc.People.Create();
                    FlixSharp.Holders.Netflix.Person netflixperson = results.Find(nid) as FlixSharp.Holders.Netflix.Person;
                    FillBasicNetflixPerson(newperson, netflixperson);

                    ftc.People.Add(newperson);
                }

                try
                {
                    //count = 
                    ftc.SaveChanges();
                }
                catch (Exception)
                {
                    throw;
                    ///need to add some sort of logging?
                }
                //if (count > 0)
                if (matchedpeople.Count() < results.Count())
                    matchedpeople = ftc.People.Where(m => netflixids.Contains(m.Netflix.Id));
                //else
                //    return matchedpeople.ToList();

                return results.Select(p =>
                    matchedpeople.First(f =>
                        f.Netflix.Id == p.Id)).ToList();
            }
        }

        public static void FillBasicNetflixTitle(FilmTrove.Models.Movie movie, FlixSharp.Holders.Netflix.Title ntitle)
        {
            movie.Netflix.Id = ntitle.Id;
            movie.Netflix.IdUrl = ntitle.IdUrl;
            movie.Netflix.SeasonId = ntitle.SeasonId;
            movie.Netflix.Url = ntitle.NetflixSiteUrl;
            movie.Netflix.AvgRating = ntitle.AverageRating;
            movie.Netflix.OfficialWebsiteUrl = ntitle.OfficialWebsite;
            movie.Netflix.PosterUrlLarge = ntitle.BoxArtUrlLarge;
            movie.Netflix.NeedsUpdate = true;
            movie.Rating = ntitle.Rating.RatingType == RatingType.Mpaa ?
                ntitle.Rating.MpaaRating.ToString() : ntitle.Rating.TvRating.ToString();
            movie.RatingType = ntitle.Rating.RatingType;
            movie.AltTitle = ntitle.ShortTitle;
            movie.Title = ntitle.FullTitle;
            movie.BestPosterUrl = ntitle.BoxArtUrlLarge;
            movie.Year = ntitle.Year;
        }
        public static void FillBasicNetflixPerson(FilmTrove.Models.Person person, FlixSharp.Holders.Netflix.Person nperson)
        {
            person.Netflix.Id = nperson.Id;
            person.Netflix.NeedsUpdate = true;
            person.Netflix.IdUrl = nperson.IdUrl;
            person.Netflix.Url = nperson.NetflixSiteUrl;
            person.Bio = nperson.Bio;
            person.Name = nperson.Name;
        }
        #endregion
        #region Rotten Tomatoes
        public static List<Models.Movie> GetDatabaseMoviesRottenTomatoes(Titles results)
        {
            var rottentomatoesids = results.Select(m => m.Id);

            using (FilmTroveContext ftc = (FilmTroveContext)HttpContext.Current.Items["ftcontext"])
            {
                ///1) find the matching records from the database
                var matchedmovies = ftc.Movies.Where(m => rottentomatoesids.Contains(m.RottenTomatoes.Id));
                ///2) find the records that don't have a match
                ///select the ids and get the rotten tomato Ids that aren't in the FT database
                var ftrtids = matchedmovies.Select(m => m.Netflix.Id).ToList();
                var rottentomatoesidsunmatched = rottentomatoesids.Where(m => !ftrtids.Contains(m)).ToList();
                //Int32 count = 0;
                var unmatchedresults = results
                    .Select(m => (m as FlixSharp.Holders.RottenTomatoes.Title))
                    .Where(m => rottentomatoesidsunmatched.Contains(m.Id))
                    .ToList();

                foreach (var unmatched in unmatchedresults)
                {
                    //String nid = unmatched.FullId;
                    ///check for FT record incase it was added using a different data source (like RT)
                    Models.Movie movie = GetExistingMovie(unmatched);
                    ///create FT database records for each of these with the movies basic information for now
                    if (movie == null)
                        movie = ftc.Movies.Create();
                    FlixSharp.Holders.RottenTomatoes.Title rottentomatoemovie = results.Find(movie.RottenTomatoes.Id) as FlixSharp.Holders.RottenTomatoes.Title;
                    FillBasicRottenTomatoesTitle(movie, rottentomatoemovie);

                    var dbgenreslocal = ftc.Genres.Local.Where(g => rottentomatoemovie.Genres.Contains(g.Name));
                    var dbgenres = ftc.Genres.Where(g => rottentomatoemovie.Genres.Contains(g.Name));
                    HashSet<Genre> genres = new HashSet<Genre>();
                    genres.AddRange(dbgenres);
                    genres.AddRange(dbgenreslocal);

                    var genrenames = genres.Select(g => g.Name);
                    var missinggenres = rottentomatoemovie.Genres.Where(g => !genrenames.Contains(g));
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
                    ftc.Movies.Add(movie);
                }

                ftc.SaveChanges();

                if (matchedmovies.Count() < results.Count())
                    matchedmovies = ftc.Movies.Where(m => rottentomatoesids.Contains(m.Netflix.Id));

                return results.Select(m =>
                    matchedmovies.First(f =>
                        f.Netflix.Id == (m.Id + ((m as FlixSharp.Holders.Netflix.Title).SeasonId != "" ? ";" 
                        + (m as FlixSharp.Holders.Netflix.Title).SeasonId : "")))).ToList();
            }
        }
        public static List<Models.Person> GetDatabasePeopleRottenTomatoes(Titles results)
        {
            return null;
        }

        public static void FillBasicRottenTomatoesTitle(FilmTrove.Models.Movie movie, FlixSharp.Holders.RottenTomatoes.Title rtitle)
        {
            movie.RottenTomatoes.Id = rtitle.Id;
            var avgrating = rtitle.Ratings
                .FirstOrDefault(r => r.Type == FlixSharp.Holders.RottenTomatoes.RatingType.Audience);
            if(avgrating != null)
                movie.RottenTomatoes.AvgRating = avgrating.Score;
            var criticrating = rtitle.Ratings
                .FirstOrDefault(r => r.Type == FlixSharp.Holders.RottenTomatoes.RatingType.Critic);
            if (criticrating != null)
                movie.RottenTomatoes.CriticScore = criticrating.Score;

            var dvdrelease = rtitle.ReleaseDates
                .FirstOrDefault(r => r.ReleaseType == FlixSharp.Holders.RottenTomatoes.ReleaseDateType.DVD);
            if (dvdrelease != null)
                movie.RottenTomatoes.DvdRelease = dvdrelease.Date;
            var theatricalrelease = rtitle.ReleaseDates
                .FirstOrDefault(r => r.ReleaseType == FlixSharp.Holders.RottenTomatoes.ReleaseDateType.Theatrical);
            if (theatricalrelease != null)
                movie.RottenTomatoes.TheatricalRelase = theatricalrelease.Date;

            movie.RottenTomatoes.LastFullUpdate = DateTime.Now;
            movie.RottenTomatoes.NeedsUpdate = false;

            var largeposter = rtitle.Posters
                .FirstOrDefault(r => r.Type == FlixSharp.Holders.RottenTomatoes.PosterType.Detailed);
            if (largeposter != null)
                movie.RottenTomatoes.PosterUrlLarge = largeposter.Url;
            var mediumposter = rtitle.Posters
                .FirstOrDefault(r => r.Type == FlixSharp.Holders.RottenTomatoes.PosterType.Detailed);
            if (mediumposter != null)
                movie.RottenTomatoes.PosterUrlMedium = mediumposter.Url;

            movie.RottenTomatoes.Studio = rtitle.Studio;
            movie.RottenTomatoes.Synopsis = rtitle.Synopsis;
            movie.RottenTomatoes.Url = rtitle.RottenTomatoesSiteUrl;
            movie.RottenTomatoes.CriticConsensus = rtitle.CriticsConsensus;

            movie.Rating = rtitle.Rating.ToString();
            movie.RatingType = RatingType.Mpaa;
            movie.AltTitle = rtitle.FullTitle;
            movie.Title = rtitle.FullTitle;
            movie.BestPosterUrl = movie.RottenTomatoes.PosterUrlLarge;
            movie.Year = rtitle.Year;
        }
        #endregion

        public static Models.Movie GetExistingMovie(ITitle title)
        {
            using (FilmTroveContext ftc = (FilmTroveContext)HttpContext.Current.Items["ftcontext"])
            {
                ///make a best effort to find the movie
                return ftc.Movies.FirstOrDefault(m=> (m.AltTitle == title.FullTitle
                    || m.AltTitle.Contains(title.FullTitle)
                    || title.FullTitle.Contains(m.AltTitle))
                    && (m.Year == title.Year 
                        || title.Year + 1 == m.Year 
                        || title.Year - 1 == m.Year));
            }
        }

    }
}