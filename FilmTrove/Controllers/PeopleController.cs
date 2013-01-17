using FilmTrove.Models;
using FlixSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FilmTrove.Code;
using StackExchange.Profiling;
using FlixSharp.Holders.Netflix;

namespace FilmTrove.Controllers
{
    public class PeopleController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Details(String id, String name)
        {
            var profiler = MiniProfiler.Current; 
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            Int32 personid = Int32.Parse(id);
            FilmTrove.Models.Person p = ftc.People.Include("Roles.Movie").Where(person => person.PersonId == personid).Single();

            Task<FlixSharp.Holders.Netflix.Person> nfp = null;

            if (p.Netflix.NeedsUpdate || p.DateLastModified > DateTime.Now.AddDays(28))
            {
                nfp = Netflix.Fill.Randomized().People.GetCompletePerson(p.Netflix.IdUrl, true);//Randomized().
            }
            if (p.RottenTomatoes.NeedsUpdate)
            {
            }
            if (p.Imdb.NeedsUpdate)
            {
            }

            if (nfp != null)
            {
                FlixSharp.Holders.Netflix.Person netflixperson = null;
                List<Movie> ftmoviesfound = null;

                Dictionary<Movie, People> actors = new Dictionary<Movie, People>();
                Dictionary<Movie, People> directors = new Dictionary<Movie, People>();

                using (profiler.Step("Get filmography"))
                {
                    ///1) get filmography
                    netflixperson = await nfp;
                    ///2) get the netflixids of all the films
                    var netflixfilmographyids = netflixperson.Filmography.Select(t => t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : "")).ToList();
                    ///3) look up the movies in ft database by netflixids
                    ftmoviesfound = ftc.Movies.Include("Roles.Person").Where(t => netflixfilmographyids.Contains(t.Netflix.Id)).ToList();

                    ///need to find the netflix titles that aren't in the roles list so i can add them as blank roles
                    var roletitles = ftmoviesfound.Where(m => p.Roles.FirstOrDefault(r => r.Movie.MovieId == m.MovieId) == null).DefaultIfEmpty().ToList();
                    foreach (var m in roletitles)
                    {
                        if (m != null)
                        { ///there's a dumb issue caused by the line that generates roletitles that causes a null value to be put into the list if it's otherwise empty
                            Role r = ftc.Roles.Create();
                            r.Movie = m;
                            r.Person = p;
                            ftc.Roles.Add(r);
                        }
                    }
                    //var roles = p.Roles.Where(r => ftmoviesfound.Find(m => m.MovieId == r.Movie.MovieId) == null).DefaultIfEmpty().ToList();//r.Movie, Comparer<Movie>.Create(m => m.MovieId)));
                    
                    var ftmoviesfoundids = ftmoviesfound.Select(t => t.Netflix.Id).ToList();
                    var netflixmoviestoadd = netflixperson.Filmography.Where(t => !ftmoviesfoundids.Contains(t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : ""))).ToList();
                    foreach (FlixSharp.Holders.Netflix.Title title in netflixmoviestoadd)
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
                        foreach (Genre g in genres)
                        {
                            MovieGenre gi = ftc.GenreItems.Create();
                            gi.Genre = g;
                            gi.Movie = m;
                            ftc.GenreItems.Add(gi);
                        }
                        ftc.Movies.Add(m);
                        
                        Role r = ftc.Roles.Create();
                        r.Movie = m;
                        r.Person = p;
                        ftc.Roles.Add(r);
                    }

                    ///4) check each of those to see they have roles defined (if so then ignore it)
                    //foreach (FilmTrove.Models.Movie ftm in ftmoviesfound)
                    //{
                    //    if (ftm.Roles.Count < 1)
                    //    {
                    //        using (profiler.Step("Start GetActors/Directors"))
                    //        {
                    //            ///7) make call to actors/directors api 
                    //            //http://api-public.netflix.com/catalog/titles/series/60030701/seasons/60035075
                    //            directors.Add(ftm, await Netflix.Fill.Randomized().Titles.GetDirectors(ftm.Netflix.IdUrl));//.Randomized()
                    //            actors.Add(ftm, await Netflix.Fill.Randomized().Titles.GetActors(ftm.Netflix.IdUrl));//Randomized().
                    //        }
                    //    }
                    //}
                }
            //    using (profiler.Step("Add Titles"))
            //    {
            //        ///5) find the netflix ids that aren't in the database
            //        var ftmoviesfoundids = ftmoviesfound.Select(t => t.Netflix.Id).ToList();
            //        var netflixmoviestoadd = netflixperson.Filmography.Where(t => !ftmoviesfoundids.Contains(t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : ""))).ToList();
            //        ///6) add those movies to the database
            //        foreach (FlixSharp.Holders.Title title in netflixmoviestoadd)
            //        {
            //            var m = ftc.Movies.Create();
            //            GeneralHelpers.FillBasicTitle(m, title);

            //            var dbgenreslocal = ftc.Genres.Local.Where(g => title.Genres.Contains(g.Name));
            //            var dbgenres = ftc.Genres.Where(g => title.Genres.Contains(g.Name));
            //            HashSet<Genre> genres = new HashSet<Genre>();
            //            genres.AddRange(dbgenres);
            //            genres.AddRange(dbgenreslocal);

            //            var genrenames = genres.Select(g => g.Name);
            //            var missinggenres = title.Genres.Where(g => !genrenames.Contains(g));
            //            foreach (String genre in missinggenres)
            //            {
            //                ftc.Genres.Add(new Genre() { Name = genre });
            //            }
            //            //newmovie.Genres = netflixmovie.Genres;
            //            foreach (Genre g in genres)
            //            {
            //                MovieGenre gi = ftc.GenreItems.Create();
            //                gi.Genre = g;
            //                gi.Movie = m;
            //                ftc.GenreItems.Add(gi);
            //            }
            //            ftc.Movies.Add(m);
            //            ///7) make call to actors/directors api on each filmography title
            //            directors.Add(m, await Netflix.Fill.Randomized().Titles.GetDirectors(m.Netflix.IdUrl));//Randomized().
            //            actors.Add(m, await Netflix.Fill.Randomized().Titles.GetActors(m.Netflix.IdUrl));//.Randomized()
            //        }
            //    }
            //    using (profiler.Step("Roles Actors"))
            //    {

            //        ///8) add the people that don't exist to the ftdatabase
            //        var actorslist = actors.Select(a => a);

            //        foreach (KeyValuePair<Models.Movie, People> peeps in actorslist)
            //        {
            //            People people = peeps.Value;
            //            //if (people.Find(p.Netflix.Id) != null)///this can't be trusted
            //            //{
            //            ///9) add roles as needed for the original movie now that database has people
            //            Role r = ftc.Roles.Create();
            //            r.InRole = RoleType.Actor;
            //            r.Movie = peeps.Key;
            //            r.Person = p;
            //            ftc.Roles.Add(r);
            //            //}
            //            var peopleids = people.Select(t => t.Id).ToList();
            //            var matchedpeople = ftc.People.Where(t => peopleids.Contains(t.Netflix.Id)).ToList();
            //            var matchedpeopleids = matchedpeople.Select(t => t.Netflix.Id).ToList();
            //            var unmatchedpeople = peopleids.Where(t => !matchedpeopleids.Contains(t)).ToList();
            //            foreach (String nid in unmatchedpeople)
            //            {
            //                FilmTrove.Models.Person newperson = ftc.People.Local.Where(t => t.Netflix.Id == nid).SingleOrDefault();
            //                if (newperson == null || newperson.Name == null || newperson.Name == "")
            //                {
            //                    newperson = ftc.People.Where(t => t.Netflix.Id == nid).SingleOrDefault();
            //                    if (newperson == null || newperson.Name == null || newperson.Name == "")
            //                    {
            //                        FlixSharp.Holders.Person nperson = people.Find(nid);
            //                        newperson = ftc.People.Create();

            //                        GeneralHelpers.FillBasicPerson(newperson, nperson);

            //                        ftc.People.Add(newperson);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    using (profiler.Step("Roles Directors"))
            //    {
            //        var directorslist = directors.ToList();//.Select(d => d);

            //        foreach (KeyValuePair<Movie, People> peeps in directorslist)
            //        {
            //            People people = peeps.Value;
            //            //if (people.Find(p.Netflix.Id) != null)
            //            //{
            //                ///9) add roles as needed for the original movie now that database has people
            //                Role r = ftc.Roles.Create();
            //                r.InRole = RoleType.Director;
            //                r.Movie = peeps.Key;
            //                r.Person = p;
            //                ftc.Roles.Add(r);
            //            //}
            //            var peopleids = people.Select(t => t.Id).ToList();
            //            var matchedpeople = ftc.People.Where(t => peopleids.Contains(t.Netflix.Id)).ToList();
            //            var matchedpeopleids = matchedpeople.Select(t => t.Netflix.Id).ToList();
            //            var unmatchedpeople = peopleids.Where(t => !matchedpeopleids.Contains(t)).ToList();
            //            foreach (String nid in unmatchedpeople)
            //            {
            //                FilmTrove.Models.Person newperson = ftc.People.Local.WhereFirstOrCreate(t => t.Netflix.Id == nid);
            //                if (newperson == null || newperson.Name == null || newperson.Name == "")
            //                {
            //                    newperson = ftc.People.WhereFirstOrCreate(t => t.Netflix.Id == nid);
            //                    if (newperson == null || newperson.Name == null || newperson.Name == "")
            //                    {
            //                        newperson = ftc.People.Create();
            //                        FlixSharp.Holders.Person nperson = people.Find(nid);
            //                        GeneralHelpers.FillBasicPerson(newperson, nperson);

            //                        ftc.People.Add(newperson);
            //                    }
            //                }
            //            }
            //        }
            //    }

            }
            p.Netflix.NeedsUpdate = false;

            ftc.SaveChanges();
            ViewBag.Person = p;
            ViewBag.NeedFilmography = p.Roles.Count < 1 | p.Netflix.NeedsUpdate;
            
            return View();
        }


        public async Task<ContentResult> Filmography(String id, String name)
        {
            //FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            //FilmTrove.Models.Person p = ftc.People.Find(Int32.Parse(id));

            //Task<FlixSharp.Holders.Person> nfp = null;

            //if (p.Netflix.NeedsUpdate) ///how can i make sure it checks filmography occasionally?
            //{
            //    nfp = Netflix.Fill.Randomized().People.GetCompletePerson(p.Netflix.IdUrl, true);//Randomized().
            //}
            //if (p.RottenTomatoes.NeedsUpdate)
            //{
            //}
            //if (p.Imdb.NeedsUpdate)
            //{
            //}

            //if (nfp != null)
            //{
            //    ///1) get filmography
            //    var netflixperson = await nfp;
            //    ///2) get the netflixids of all the films
            //    var netflixfilmographyids = netflixperson.Filmography.Select(t => t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : "")).ToList();
            //    ///3) look up the movies in ft database by netflixids
            //    var ftmoviesfound = ftc.Movies.Include("Roles").Where(t => netflixfilmographyids.Contains(t.Netflix.Id)).ToList();
            //    ///4) check each of those to see they have roles defined (if so then ignore it)
            //    Dictionary<Movie, Task<People>> actors = new Dictionary<Movie, Task<People>>();
            //    Dictionary<Movie, Task<People>> directors = new Dictionary<Movie, Task<People>>();

            //    foreach (FilmTrove.Models.Movie ftm in ftmoviesfound)
            //    {
            //        if (ftm.Roles.Count < 1)
            //        {
            //            ///7) make call to actors/directors api 
            //            //http://api-public.netflix.com/catalog/titles/series/60030701/seasons/60035075
            //            directors.Add(ftm, Netflix.Fill.Randomized().Titles.GetDirectors(ftm.Netflix.IdUrl));//.Randomized()
            //            actors.Add(ftm, Netflix.Fill.Randomized().Titles.GetActors(ftm.Netflix.IdUrl));//Randomized().
            //        }
            //    }
            //    ///5) find the netflix ids that aren't in the database
            //    var ftmoviesfoundids = ftmoviesfound.Select(t => t.Netflix.Id).ToList();
            //    var netflixmoviestoadd = netflixperson.Filmography.Where(t => !ftmoviesfoundids.Contains(t.Id)).ToList();
            //    ///6) add those movies to the database
            //    foreach (FlixSharp.Holders.Title title in netflixmoviestoadd)
            //    {
            //        var m = ftc.Movies.Create();
            //        GeneralHelpers.FillBasicTitle(m, title);

            //        var dbgenreslocal = ftc.Genres.Local.Where(g => title.Genres.Contains(g.Name));
            //        var dbgenres = ftc.Genres.Where(g => title.Genres.Contains(g.Name));
            //        HashSet<Genre> genres = new HashSet<Genre>();
            //        genres.AddRange(dbgenres);
            //        genres.AddRange(dbgenreslocal);

            //        var genrenames = genres.Select(g => g.Name);
            //        var missinggenres = title.Genres.Where(g => !genrenames.Contains(g));
            //        foreach (String genre in missinggenres)
            //        {
            //            ftc.Genres.Add(new Genre() { Name = genre });
            //        }
            //        //newmovie.Genres = netflixmovie.Genres;
            //        foreach (Genre g in genres)
            //        {
            //            MovieGenre gi = ftc.GenreItems.Create();
            //            gi.Genre = g;
            //            gi.Movie = m;
            //            ftc.GenreItems.Add(gi);
            //        }
            //        ftc.Movies.Add(m);
            //        ///7) make call to actors/directors api on each filmography title
            //        directors.Add(m, Netflix.Fill.Randomized().Titles.GetDirectors(m.Netflix.IdUrl));//Randomized().
            //        actors.Add(m, Netflix.Fill.Randomized().Titles.GetActors(m.Netflix.IdUrl));//.Randomized()
            //    }

            //    ///8) add the people that don't exist to the ftdatabase
            //    var actorslist = actors.Select(a => a);
            //    var directorslist = directors.Select(d => d);
            //    Int32 s = ftc.SaveChanges();
            //    foreach (KeyValuePair<Movie, Task<People>> peeps in actorslist)
            //    {
            //        People people = await peeps.Value;
            //        if (people.Find(p.Netflix.Id) != null)
            //        {
            //            ///9) add roles as needed for the original movie now that database has people
            //            Role r = ftc.Roles.Create();
            //            r.InRole = RoleType.Actor;
            //            r.Movie = peeps.Key;
            //            r.Person = p;
            //            ftc.Roles.Add(r);
            //        }
            //        var peopleids = people.Select(t => t.Id).ToList();
            //        var matchedpeople = ftc.People.Where(t => peopleids.Contains(t.Netflix.Id)).ToList();
            //        var matchedpeopleids = matchedpeople.Select(t => t.Netflix.Id).ToList();
            //        var unmatchedpeople = peopleids.Where(t => !matchedpeopleids.Contains(t)).ToList();
            //        foreach (String nid in unmatchedpeople)
            //        {
            //            FilmTrove.Models.Person newperson = ftc.People.Local.Where(t => t.Netflix.Id == nid).SingleOrDefault();
            //            if (newperson == null || newperson.Name == null || newperson.Name == "")
            //            {
            //                newperson = ftc.People.Where(t => t.Netflix.Id == nid).SingleOrDefault();
            //                if (newperson == null || newperson.Name == null || newperson.Name == "")
            //                {
            //                    FlixSharp.Holders.Person nperson = people.Find(nid);
            //                    newperson = ftc.People.Create();

            //                    GeneralHelpers.FillBasicPerson(newperson, nperson);

            //                    ftc.People.Add(newperson);
            //                }
            //            }
            //        }
            //    }
            //    //Int32 c = ftc.SaveChanges();

            //    foreach (KeyValuePair<Movie, Task<People>> peeps in directorslist)
            //    {
            //        People people = await peeps.Value;
            //        if (people.Find(p.Netflix.Id) != null)
            //        {
            //            ///9) add roles as needed for the original movie now that database has people
            //            Role r = ftc.Roles.Create();
            //            r.InRole = RoleType.Director;
            //            r.Movie = peeps.Key;
            //            r.Person = p;
            //            ftc.Roles.Add(r);
            //        }
            //        var peopleids = people.Select(t => t.Id).ToList();
            //        var matchedpeople = ftc.People.Where(t => peopleids.Contains(t.Netflix.Id)).ToList();
            //        var matchedpeopleids = matchedpeople.Select(t => t.Netflix.Id).ToList();
            //        var unmatchedpeople = peopleids.Where(t => !matchedpeopleids.Contains(t)).ToList();
            //        foreach (String nid in unmatchedpeople)
            //        {
            //            FilmTrove.Models.Person newperson = ftc.People.Local.WhereFirstOrCreate(t => t.Netflix.Id == nid);
            //            if (newperson == null || newperson.Name == null || newperson.Name == "")
            //            {
            //                newperson = ftc.People.WhereFirstOrCreate(t => t.Netflix.Id == nid);
            //                if (newperson == null || newperson.Name == null || newperson.Name == "")
            //                {
            //                    newperson = ftc.People.Create();
            //                    FlixSharp.Holders.Person nperson = people.Find(nid);
            //                    GeneralHelpers.FillBasicPerson(newperson, nperson);

            //                    ftc.People.Add(newperson);
            //                }
            //            }
            //        }
            //    }

            //}
            //p.Netflix.NeedsUpdate = false;
            //ftc.SaveChanges();
            //ViewBag.Person = p;

            return new ContentResult() { Content = "yay" };
        }

        
    }
}
