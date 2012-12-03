using FilmTrove.Models;
using FlixSharp.Holders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FilmTrove.Code
{
    public class GeneralHelpers
    {
        public static List<Models.Movie> GetDatabaseMovies(Titles results)
        {
            var netflixids = results.Select((m) => m.Id + (m.SeasonId != "" ? ";" + m.SeasonId : ""));
            using (FilmTroveContext ftc = new FilmTroveContext())
            {

                ///1) find the matching records from the database
                var matchedmovies = ftc.Movies.Where(m => netflixids.Contains(m.Netflix.Id));
                ///2) find the records that don't have a match
                ///select the ids and get the netflix Ids that aren't in the FT database
                var ftnfids = matchedmovies.Select(m => m.Netflix.Id);
                var netflixidsunmatched = netflixids.Where(m => !ftnfids.Contains(m));
                //Int32 count = 0;
                foreach (String nid in netflixidsunmatched)
                {
                    ///create FT database records for each of these with the movies basic information for now
                    FilmTrove.Models.Movie newmovie = ftc.Movies.Create();
                    FlixSharp.Holders.Title netflixmovie = results.Find(nid);
                    FillBasicTitle(newmovie, netflixmovie);


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
                        gi.Movie = newmovie;
                        ftc.GenreItems.Add(gi);
                    }
                    ftc.Movies.Add(newmovie);
                }

                //try
                //{
                    //count = 
                ftc.SaveChanges();
                //}
                //catch (Exception ex)
                //{
                //    ///need to add some sort of logging?

                //}
                //if (count > 0)
                if (matchedmovies.Count() < results.Count())
                    return ftc.Movies.Where(m => netflixids.Contains(m.Netflix.Id)).ToList();
                else
                    return matchedmovies.ToList();
            }
        }

        public static List<Models.Person> GetDatabasePeople(People results)
        {
            var netflixids = results.Select(p => p.Id);
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                ///1) find the matching records from the database
                var matchedpeople = ftc.People.Where(m => netflixids.Contains(m.Netflix.Id));
                ///2) find the records that don't have a match
                ///select the ids and get the netflix Ids that aren't in the FT database
                var ftnfids = matchedpeople.Select(m => m.Netflix.Id);
                var netflixidsunmatched = netflixids.Where(m => !ftnfids.Contains(m));
                //Int32 count = 0;
                foreach (String nid in netflixidsunmatched)
                {
                    ///create FT database records for each of these with the movies basic information for now
                    FilmTrove.Models.Person newperson = ftc.People.Create();
                    FlixSharp.Holders.Person netflixperson = results.Find(nid);
                    FillBasicPerson(newperson, netflixperson);
                    //newperson.Name = netflixperson.Name;
                    //newperson.Bio = netflixperson.Bio;
                    //newperson.Netflix = new NetflixPersonInfo();
                    //newperson.Netflix.Id = netflixperson.Id;
                    //newperson.Netflix.IdUrl = netflixperson.IdUrl;
                    //newperson.Netflix.Url = netflixperson.NetflixSiteUrl;

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
                    return ftc.People.Where(m => netflixids.Contains(m.Netflix.Id)).ToList();
                else
                    return matchedpeople.ToList();
            }
        }

        public static void FillBasicTitle(FilmTrove.Models.Movie movie, FlixSharp.Holders.Title ntitle)
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
        public static void FillBasicPerson(FilmTrove.Models.Person person, FlixSharp.Holders.Person nperson)
        {
            person.Netflix.Id = nperson.Id;
            person.Netflix.NeedsUpdate = true;
            person.Netflix.IdUrl = nperson.IdUrl;
            person.Netflix.Url = nperson.NetflixSiteUrl;
            person.Bio = nperson.Bio;
            person.Name = nperson.Name;
        }


    }
}