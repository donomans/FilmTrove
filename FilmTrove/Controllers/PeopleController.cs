using FilmTrove.Models;
using FlixSharp;
using FlixSharp.Holders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FilmTrove.Controllers
{
    public class PeopleController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Details(String id, String name)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                FilmTrove.Models.Person p = ftc.People.Find(Int32.Parse(id));

                Task<FlixSharp.Holders.Person> nfp = null;

                if (p.Netflix.NeedsUpdate)
                {
                    nfp = Netflix.Fill.People.GetCompletePerson(p.Netflix.Url, true);
                }
                if (p.RottenTomatoes.NeedsUpdate)
                {
                }
                if (p.Imdb.NeedsUpdate)
                {
                }

                if (nfp != null)
                {
                    ///1) get filmography
                    var netflixperson = await nfp;
                    ///2) get the netflixids of all the films
                    var netflixfilmographyids = netflixperson.Filmography.Select(t => t.Id);
                    ///3) look up the movies in ft database by netflixids
                    var ftmoviesfound = ftc.Movies.Where(t => netflixfilmographyids.Contains(t.Netflix.Id));
                    ///4) check each of those to see they have roles defined (if so then ignore it)
                    foreach (FilmTrove.Models.Movie ftm in ftmoviesfound)
                    {
                        if (ftm.Roles.Count < 1)
                        {
                            ///7) make call to actors/directors api 
                            ///8) add the people that don't exist to the ftdatabase 
                            ///9) add roles as needed
                        }
                    }
                    ///5) find the netflix ids that aren't in the database
                    var ftmoviesfoundids = ftmoviesfound.Select(t => t.Netflix.Id);
                    var netflixmoviestoadd = netflixperson.Filmography.Where(t => !ftmoviesfoundids.Contains(t.Id));
                    ///6) add those movies to the database
                    foreach (FlixSharp.Holders.Title title in netflixmoviestoadd)
                    {
                        var m = ftc.Movies.Create();
                        m.Netflix.Id = title.Id;
                        m.Netflix.Url = title.IdUrl;
                        m.Netflix.AvgRating = title.AverageRating;
                        m.Netflix.OfficialWebsiteUrl = title.OfficialWebsite;
                        m.Netflix.PosterUrlLarge = title.BoxArtUrlLarge;
                        m.Netflix.NeedsUpdate = true;
                        m.Rating = title.Rating.RatingType == RatingType.Mpaa ?
                            title.Rating.MpaaRating.ToString() : title.Rating.TvRating.ToString();
                        m.RatingType = title.Rating.RatingType;
                        m.AltTitle = title.ShortTitle;
                        m.Title = title.FullTitle;
                        m.BestPosterUrl = title.BoxArtUrlLarge;
                        m.Year = title.Year;
                        m.Genres = title.Genres;
                        ftc.Movies.Add(m);
                    }
                    ///7) make call to actors/directors api 
                    ///8) add the people that don't exist to the ftdatabase 
                    ///9) add roles as needed
                    ///
                    
                    //foreach(FlixSharp.Holders.Title title in netflixperson.Filmography)
                    //{
                    //    //await Netflix.Fill.Titles.GetDirectors(title.IdUrl, true);
                    //    var m = ftc.Movies.Where(t => t.Netflix.Id == title.Id).FirstOrDefault();
                    //    if (m == null)
                    //    {
                    //        ///add movie to database and add a role on the movie for this person
                    //        m = ftc.Movies.Create();
                    //        m.Netflix.Id = title.Id;
                    //        m.Netflix.Url = title.IdUrl;
                    //        m.Netflix.AvgRating = title.AverageRating;
                    //        m.Netflix.OfficialWebsiteUrl = title.OfficialWebsite;
                    //        m.Netflix.PosterUrlLarge = title.BoxArtUrlLarge;
                    //        m.Netflix.NeedsUpdate = true;
                    //        m.Rating = title.Rating.RatingType == RatingType.Mpaa ?
                    //            title.Rating.MpaaRating.ToString() : title.Rating.TvRating.ToString();
                    //        m.RatingType = title.Rating.RatingType;
                    //        m.AltTitle = title.ShortTitle;
                    //        m.Title = title.FullTitle;
                    //        m.BestPosterUrl = title.BoxArtUrlLarge;
                    //        m.Year = title.Year;
                    //        m.Genres = title.Genres;

                    //        Role r = ftc.Roles.Create();
                    //        ///how do i match up the roles now?
                    //    }
                    //}
                }
            }
            return View();
        }

    }
}
