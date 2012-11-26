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
        public ActionResult Details(String id, String name)
        {
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            
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
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            FilmTrove.Models.Person p = ftc.People.Find(Int32.Parse(id));

            Task<FlixSharp.Holders.Person> nfp = null;

            if (p.Netflix.NeedsUpdate) ///how can i make sure it checks filmography occasionally?
            {
                nfp = Netflix.Fill.People.GetCompletePerson(p.Netflix.IdUrl, true);//Randomized().
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
                var netflixfilmographyids = netflixperson.Filmography.Select(t => t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : ""));
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
                        //http://api-public.netflix.com/catalog/titles/series/60030701/seasons/60035075
                        directors.Add(ftm, Netflix.Fill.Titles.GetDirectors(ftm.Netflix.IdUrl));//.Randomized()
                        actors.Add(ftm, Netflix.Fill.Titles.GetActors(ftm.Netflix.IdUrl));//Randomized().
                    }
                }
                ///5) find the netflix ids that aren't in the database
                var ftmoviesfoundids = ftmoviesfound.Select(t => t.Netflix.Id);
                var netflixmoviestoadd = netflixperson.Filmography.Where(t => !ftmoviesfoundids.Contains(t.Id));
                ///6) add those movies to the database
                foreach (FlixSharp.Holders.Title title in netflixmoviestoadd)
                {
                    var m = ftc.Movies.Create();
                    GeneralHelpers.FillBasicTitle(m, title);

                    var dbgenreslocal = ftc.Genres.Local.Where(g => title.Genres.Contains(g.Name));
                    var dbgenres = ftc.Genres.Where(g => title.Genres.Contains(g.Name));
                    HashSet<Genre> genres = new HashSet<Genre>();
                    genres.AddRange(dbgenres);
                    genres.AddRange(dbgenreslocal);

                    var genrenames = genres.Select(g => g.Name);
                    var missinggenres = title.Genres.Where(g => !genrenames.Contains(g));
                    foreach (String genre in missinggenres)
                    {
                        ftc.Genres.Add(new Genre() { Name = genre });
                    }
                    //newmovie.Genres = netflixmovie.Genres;
                    foreach (Genre g in genres)
                    {
                        MovieGenre gi = ftc.GenreItems.Create();
                        gi.Genre = g;
                        gi.Movie = m;
                        ftc.GenreItems.Add(gi);
                    }
                    ftc.Movies.Add(m);
                    ///7) make call to actors/directors api on each filmography title
                    directors.Add(m, Netflix.Fill.Titles.GetDirectors(m.Netflix.IdUrl));//Randomized().
                    actors.Add(m, Netflix.Fill.Titles.GetActors(m.Netflix.IdUrl));//.Randomized()
                }

                ///8) add the people that don't exist to the ftdatabase
                var actorslist = actors.Select(a => a);
                var directorslist = directors.Select(d => d);
                Int32 s = ftc.SaveChanges();
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
                                GeneralHelpers.FillBasicPerson(newperson, nperson);

                                ftc.People.Add(newperson);
                            }
                        }
                    }
                }
                //Int32 c = ftc.SaveChanges();

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
                                GeneralHelpers.FillBasicPerson(newperson, nperson);

                                ftc.People.Add(newperson);
                            }
                        }
                    }
                }

            }
            p.Netflix.NeedsUpdate = false;
            ftc.SaveChanges();
            ViewBag.Person = p;

            return new ContentResult() { Content = "yay" };
        }

        
    }
}
