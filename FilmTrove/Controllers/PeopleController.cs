using FilmTrove.Models;
using FlixSharp;
using FlixSharp.Holders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FilmTrove.Code;

namespace FilmTrove.Controllers
{
    public class PeopleController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Details(String id, String name)
        {
            FilmTroveContext ftc = new FilmTroveContext();
            
            FilmTrove.Models.Person p = ftc.People.Find(Int32.Parse(id));
 
            if (p.DateLastModified > DateTime.Now.AddDays(14))
            {
                p.Netflix.NeedsUpdate = true;
            }

            if (p.RottenTomatoes.NeedsUpdate)
            {
            }
            if (p.Imdb.NeedsUpdate)
            {
            }

            ftc.SaveChanges();
            ViewBag.Person = p;
            ViewBag.NeedFilmography = p.Roles.Count < 1 | p.Netflix.NeedsUpdate;
            
            return View();
        }


        public async Task<ContentResult> Filmography(String id, String name)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                FilmTrove.Models.Person p = ftc.People.Find(Int32.Parse(id));

                Task<FlixSharp.Holders.Person> nfp = null;

                if (p.Netflix.NeedsUpdate) ///how can i make sure it checks filmography occasionally?
                {
                    nfp = Netflix.Fill.People.GetCompletePerson(p.Netflix.IdUrl, true);
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
                    var ftmoviesfound = ftc.Movies.Include("Roles").Where(t => netflixfilmographyids.Contains(t.Netflix.Id));
                    ///4) check each of those to see they have roles defined (if so then ignore it)
                    Dictionary<Movie, Task<People>> actors = new Dictionary<Movie, Task<People>>();
                    Dictionary<Movie, Task<People>> directors = new Dictionary<Movie, Task<People>>();
                    
                    foreach (FilmTrove.Models.Movie ftm in ftmoviesfound)
                    {
                        if (ftm.Roles.Count < 1)
                        {
                            ///7) make call to actors/directors api 
                            
                            directors.Add(ftm, Netflix.Fill.Titles.GetDirectors(ftm.Netflix.IdUrl));
                            actors.Add(ftm, Netflix.Fill.Titles.GetActors(ftm.Netflix.IdUrl));
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
                        m.Netflix.IdUrl = title.IdUrl;
                        m.Netflix.Url = title.NetflixSiteUrl;
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
                        ///7) make call to actors/directors api on each filmography title
                        directors.Add(m, Netflix.Fill.Titles.GetDirectors(m.Netflix.IdUrl));
                        actors.Add(m, Netflix.Fill.Titles.GetActors(m.Netflix.IdUrl));
                    }

                    ///8) add the people that don't exist to the ftdatabase
                    var actorslist = actors.Select(a => a);
                    var directorslist = directors.Select(d => d);
                    foreach (KeyValuePair<Movie, Task<People>> peeps in actorslist)
                    {
                        People people = await peeps.Value;
                        if (people.Find(p.Netflix.Id) != null)
                        {
                            ///9) add roles as needed for the original movie now that database has people
                            Role r = ftc.Roles.Create();
                            r.InRole = RoleType.Actor;
                            r.Movie = peeps.Key;
                            r.Person = p;
                            ftc.Roles.Add(r);
                        }
                        var peopleids = people.Select(t => t.Id);
                        var matchedpeople = ftc.People.Where(t => peopleids.Contains(t.Netflix.Id));
                        var matchedpeopleids = matchedpeople.Select(t=>t.Netflix.Id);
                        var unmatchedpeople = peopleids.Where(t => !matchedpeopleids.Contains(t));
                        foreach (String nid in unmatchedpeople)
                        {
                            FilmTrove.Models.Person newperson = ftc.People.Local.WhereFirstOrCreate(t => t.Netflix.Id == nid);
                            if (newperson.Name == null || newperson.Name == "")
                            {
                                newperson = ftc.People.WhereFirstOrCreate(t => t.Netflix.Id == nid);
                                if (newperson.Name == null || newperson.Name == "")
                                {
                                    FlixSharp.Holders.Person nperson = people.Find(nid);
                                    newperson.Name = nperson.Name;
                                    newperson.Bio = nperson.Bio;
                                    newperson.Netflix.Id = nperson.Id;
                                    newperson.Netflix.IdUrl = nperson.IdUrl;
                                    newperson.Netflix.Url = nperson.NetflixSiteUrl;

                                    ftc.People.Add(newperson);
                                }
                            }
                        }
                    }
                    foreach (KeyValuePair<Movie, Task<People>> peeps in directorslist)
                    {
                        People people = await peeps.Value;
                        if (people.Find(p.Netflix.Id) != null)
                        {
                            ///9) add roles as needed for the original movie now that database has people
                            Role r = ftc.Roles.Create();
                            r.InRole = RoleType.Director;
                            r.Movie = peeps.Key;
                            r.Person = p;
                            ftc.Roles.Add(r);
                        }
                        var peopleids = people.Select(t => t.Id);
                        var matchedpeople = ftc.People.Where(t => peopleids.Contains(t.Netflix.Id));
                        var matchedpeopleids = matchedpeople.Select(t => t.Netflix.Id);
                        var unmatchedpeople = peopleids.Where(t => !matchedpeopleids.Contains(t));
                        foreach (String nid in unmatchedpeople)
                        {
                            FilmTrove.Models.Person newperson = ftc.People.Local.WhereFirstOrCreate(t => t.Netflix.Id == nid);
                            if (newperson.Name == null || newperson.Name == "")
                            {
                                newperson = ftc.People.WhereFirstOrCreate(t => t.Netflix.Id == nid);
                                if (newperson.Name == null || newperson.Name == "")
                                {
                                    FlixSharp.Holders.Person nperson = people.Find(nid);
                                    newperson.Name = nperson.Name;
                                    newperson.Bio = nperson.Bio;
                                    newperson.Netflix.Id = nperson.Id;
                                    newperson.Netflix.Url = nperson.NetflixSiteUrl;
                                    newperson.Netflix.IdUrl = nperson.IdUrl;

                                    ftc.People.Add(newperson);
                                }
                            }
                        }
                    }
                    
                }
                p.Netflix.NeedsUpdate = false;
                ftc.SaveChanges();
                ViewBag.Person = p;
            }
            return new ContentResult() { Content = "yay" };
        }
    }
}
