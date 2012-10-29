using FilmTrove.Models;
using FlixSharp.Holders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FilmTrove.Code
{
    public class AsyncHelpers
    {
        public async static Task<List<Models.Movie>> GetDatabaseMovies(Titles results)
        {
            var netflixids = results.Select((m) => m.Id);
            using (FilmTroveContext ftc = new FilmTroveContext())
            {

                ///1) find the matching records from the database
                var matchedmovies = ftc.Movies.Where(m => netflixids.Contains(m.Netflix.Id));
                ///2) find the records that don't have a match
                ///select the ids and get the netflix Ids that aren't in the FT database
                var ftnfids = matchedmovies.Select(m => m.Netflix.Id);
                var netflixidsunmatched = netflixids.Where(m => !ftnfids.Contains(m));
                Int32 count = 0;
                foreach (String nid in netflixidsunmatched)
                {
                    ///create FT database records for each of these with the movies basic information for now
                    FilmTrove.Models.Movie newmovie = ftc.Movies.Create();
                    FlixSharp.Holders.Title netflixmovie = results.Find(nid);
                    newmovie.Netflix = new NetflixInfo();
                    newmovie.Netflix.Id = nid;
                    newmovie.Netflix.Url = netflixmovie.IdUrl;
                    newmovie.Netflix.AvgRating = netflixmovie.AverageRating;
                    newmovie.Netflix.PosterUrlLarge = netflixmovie.BoxArtUrlLarge;
                    newmovie.BestPosterUrl = netflixmovie.BoxArtUrlLarge;
                    newmovie.Year = netflixmovie.Year;
                    newmovie.Title = netflixmovie.FullTitle;
                    newmovie.Genres = netflixmovie.Genres;
                    ftc.Movies.Add(newmovie);
                    count++;
                }

                try
                {
                    Int32 changed = ftc.SaveChanges();
                    if (changed < count)
                    {
                        ///for some reason all the movies weren't saved?
                    }
                }
                catch (Exception)
                {
                    ///need to add some sort of logging?
                    
                }
                if (count > 0)
                    matchedmovies = ftc.Movies.Where(m => netflixids.Contains(m.Netflix.Id));
                return matchedmovies.ToList();
            }
        }

        public async static Task<List<Models.Person>> GetDatabasePeople(People results)
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
                Int32 count = 0;
                foreach (String nid in netflixidsunmatched)
                {
                    ///create FT database records for each of these with the movies basic information for now
                    FilmTrove.Models.Person newperson = ftc.People.Create();
                    FlixSharp.Holders.Person netflixperson = results.Find(nid);
                    newperson.Name = netflixperson.Name;
                    newperson.Bio = netflixperson.Bio;
                    newperson.Netflix = new NetflixPersonInfo();
                    newperson.Netflix.Id = netflixperson.Id;
                    newperson.Netflix.Url = netflixperson.IdUrl;

                    ftc.People.Add(newperson);
                    count++;
                }

                try
                {
                    Int32 changed = ftc.SaveChanges();
                    if (changed < count)
                    {
                        ///for some reason all the movies weren't saved?
                    }
                }
                catch (Exception)
                {
                    throw;
                    ///need to add some sort of logging?
                }
                if (count > 0)
                    matchedpeople = ftc.People.Where(m => netflixids.Contains(m.Netflix.Id));
                return matchedpeople.ToList();
            }
        }
    }
}