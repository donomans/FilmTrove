using FilmTrove.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlixSharp;
using FlixSharp.Holders;
using FilmTrove.Filters;
using System.Threading.Tasks;
using FilmTrove.Models;
using System.Text.RegularExpressions;
using StackExchange.Profiling;

namespace FilmTrove.Controllers
{
    [InitializeSimpleMembership]
    public class MoviesController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Details(String id, String title)
        {
            var profiler = MiniProfiler.Current; 
            var ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            Int32 movieid = Int32.Parse(id);
            Models.Movie m = ftc.Movies.Include("Roles.Person").Include("Genres.Genre").Where(movie => movie.MovieId == movieid).Single();//.Include("Roles.Person").Where(movie => movie.MovieId == movieid).Single();
            Task<FlixSharp.Holders.Title> nfm = null;

            if (m.Netflix.NeedsUpdate || m.DateLastModified > DateTime.Now.AddDays(28))
            {
                nfm = Netflix.Fill.Randomized().Titles.GetCompleteTitle(m.Netflix.IdUrl, OnUserBehalf: true);//Randomized().
            }
            using (profiler.Step("Populate Amazon Movie"))
            {
                if (m.Amazon.NeedsUpdate || (m.Amazon.LastPriceUpdate.HasValue && ((m.Amazon.LastPriceUpdate.Value - DateTime.Now) > new TimeSpan(1, 0, 0, 0))))
                {
                    if (m.Amazon.Id == null || m.Amazon.Id == "" ||
                        m.Amazon.LastFullUpdate.HasValue && ((m.Amazon.LastFullUpdate.Value - DateTime.Now) > new TimeSpan(7, 0, 0, 0)))
                    {
                        ///1) do query to match up title and fill in id with priority to blu-ray
                    }
                    ///1) else use Id

                    ///2) fill in found id
                    ///3) purchase url (prioritize blu-ray)
                    ///4) price
                    ///5) update lastpriceupdate
                }
            }
            using (profiler.Step("Populate Imdb Movie"))
            {
                if (m.Imdb.NeedsUpdate)
                {
                    ///nothing
                }
            }
            using (profiler.Step("Populate Rotten Tomatoes Movie"))
            {
                if (m.RottenTomatoes.NeedsUpdate ||
                    (m.RottenTomatoes.LastFullUpdate.HasValue && ((m.Amazon.LastFullUpdate.Value - DateTime.Now) > new TimeSpan(7, 0, 0, 0))))
                {
                    if (m.RottenTomatoes.Id == null || m.RottenTomatoes.Id == "" ||
                        m.RottenTomatoes.LastFullUpdate.HasValue && ((m.RottenTomatoes.LastFullUpdate.Value - DateTime.Now) > new TimeSpan(7, 0, 0, 0)))
                    {
                        ///1) title match like with amazon or use Id if present
                    }
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
                }
            }
            #region Fill Netflix
            if (nfm != null)
            {
                using (profiler.Step("Fill Netflix Movie"))
                {
                    Title netflixtitle = null;
                    using (profiler.Step("Await Netflix Title"))
                    {
                        netflixtitle = await nfm;
                    }
                    using (profiler.Step("Populate basic database 'm' record"))
                    {
                        ///populate all the netflix information
                        m.Netflix.NeedsUpdate = false;
                        m.Netflix.AvgRating = netflixtitle.AverageRating;
                        m.Netflix.Awards = netflixtitle.Awards.Select(a =>
                            a.AwardName + ";#" + a.PersonIdUrl + ";#" +
                            a.Type + ";#" + a.Winner + ";#" + a.Year).DefaultIfEmpty().ToList();
                        m.AltTitle = netflixtitle.ShortTitle;
                        m.Rating = netflixtitle.Rating.RatingType == RatingType.Mpaa ?
                                netflixtitle.Rating.MpaaRating.ToString() : netflixtitle.Rating.TvRating.ToString();
                        m.RatingType = netflixtitle.Rating.RatingType;

                        foreach (FlixSharp.Holders.FormatAvailability f in netflixtitle.Formats)
                        {
                            switch (f.Format)
                            {
                                case "Blu-ray":
                                    m.Netflix.Format |= Models.Format.Bluray;
                                    break;
                                case "DVD":
                                    m.Netflix.Format |= Models.Format.DVD;
                                    break;
                                case "instant":
                                    m.Netflix.Format |= Models.Format.Digital;
                                    break;
                            }
                        }
                        //m.Netflix.RelatedTitles = netflixmovie.RelatedTitles.Select(t => t.Id).ToList();
                        m.Netflix.Synopsis = netflixtitle.Synopsis;
                        m.Netflix.IdUrl = netflixtitle.IdUrl;
                        m.Netflix.Url = netflixtitle.NetflixSiteUrl;
                        m.Netflix.OfficialWebsiteUrl = netflixtitle.NetflixSiteUrl;
                    }

                    ///need to find the roles that are already added (under the RoleType.None) so i can correct those
                    var noneroles = m.Roles.Where(r => r.InRole == RoleType.None).ToList();

                    using (profiler.Step("Populate Actors"))
                    {
                        var nfactorids = netflixtitle.Actors.Select(t => t.Id).ToList();
                        var matchedactors = ftc.People.Where(t => nfactorids.Contains(t.Netflix.Id)).ToList();
                        var matchedactorids = matchedactors.Select(p => p.Netflix.Id).ToList();
                        var actorsfordatabase = netflixtitle.Actors.Where(t => !matchedactorids.Contains(t.Id)).ToList();

                        foreach (FlixSharp.Holders.Person p in actorsfordatabase)
                        {
                            ///1) find the cast and loop through and add the people to the database in a similar manner as AsyncHelpers.GetDatabasePeople
                            FilmTrove.Models.Person ftperson = null;
                            ftperson = ftc.People.Local.Where(t => t.Netflix.Id == p.Id).SingleOrDefault();
                            if (ftperson == null || ftperson.Name == null || ftperson.Name == "")
                            {
                                ftperson = ftc.People.Where(t => t.Netflix.Id == p.Id).SingleOrDefault();
                                if (ftperson == null || ftperson.Name == null || ftperson.Name == "")
                                {
                                    ftperson = ftc.People.Create();
                                    GeneralHelpers.FillBasicPerson(ftperson, p);
                                    ftc.People.Add(ftperson);
                                }
                            }

                            ///2) add this movie as a new role on the movie
                            Role r = ftc.Roles.Create();
                            r.InRole = RoleType.Actor;
                            r.Movie = m;
                            r.Person = ftperson;
                            ftc.Roles.Add(r);
                        }
                        ///need to find the noneroles that are actors now
                        var noneroleactors = noneroles.Where(r => matchedactorids.Contains(r.Person.Netflix.Id.ToString())).ToList();
                        foreach (Role r in noneroleactors)
                        {
                            r.InRole = RoleType.Actor;
                        }
                        ftc.SaveChanges();
                        if (m.Roles.Count(c => c.InRole == RoleType.Actor) < nfactorids.Count)
                        {
                            ///need to find which roles haven't already been added.
                            var currentroles = m.Roles.Where(r => r.InRole == RoleType.Actor).Select(r => r.Person.PersonId).ToList();
                            var actorsfordb = matchedactors.Where(r => !currentroles.Contains(r.PersonId)).ToList();
                            foreach (Models.Person p in actorsfordb)
                            {
                                ///2) add this movie as a new role on the movie
                                Role r = ftc.Roles.Create();
                                r.InRole = RoleType.Actor;
                                r.Movie = m;
                                r.Person = p;
                                ftc.Roles.Add(r);
                            }
                        }
                        //if (m.Roles.Count(c => c.InRole == RoleType.Actor) < netflixtitle.Actors.Count())
                        //{
                        //    ///need to find which roles haven't already been added.
                        //    var currentroles = m.Roles.Where(r => r.InRole == RoleType.Actor).Select(r => r.Person.Netflix.Id).ToList();
                        //    var actorsfordb = netflixtitle.Actors.Where(r => !currentroles.Contains(r.Id)).Select(a => a.Id).ToList();
                        //    var actorsft = ftc.People.Where(p => actorsfordb.Contains(p.Netflix.Id)).ToList();
                        //    foreach (Models.Person p in actorsft)
                        //    {
                        //        ///2) add this movie as a new role on the movie
                        //        Role r = ftc.Roles.Create();
                        //        r.InRole = RoleType.Actor;
                        //        r.Movie = m;
                        //        r.Person = p;
                        //        ftc.Roles.Add(r);
                        //    }
                        //}
                    }
                    using (profiler.Step("Populate Directors"))
                    {
                        var nfdirectorids = netflixtitle.Directors.Select(t => t.Id).ToList();
                        var matcheddirectors = ftc.People.Where(t => nfdirectorids.Contains(t.Netflix.Id)).ToList();
                        var matcheddirectorids = matcheddirectors.Select(p => p.Netflix.Id).ToList();
                        var directorsfordatabase = netflixtitle.Directors.Where(t => !matcheddirectorids.Contains(t.Id)).ToList();
                        foreach (FlixSharp.Holders.Person p in directorsfordatabase)
                        {
                            ///1) find the cast and loop through and add the people to the database in a similar manner as AsyncHelpers.GetDatabasePeople
                            FilmTrove.Models.Person ftperson = ftc.People.Local.Where(t => t.Netflix.Id == p.Id).SingleOrDefault();
                            if (ftperson == null || ftperson.Name == null || ftperson.Name == "")
                            {
                                ftperson = ftc.People.Where(t => t.Netflix.Id == p.Id).SingleOrDefault();
                                if (ftperson == null || ftperson.Name == null || ftperson.Name == "")
                                {
                                    ftperson = ftc.People.Create();
                                    GeneralHelpers.FillBasicPerson(ftperson, p);
                                    ftc.People.Add(ftperson);
                                }
                            }
                            ///2) add this movie as a new role on the movie
                            Role r = ftc.Roles.Create();
                            r.InRole = RoleType.Director;
                            r.Movie = m;
                            r.Person = ftperson;
                            ftc.Roles.Add(r);
                        }
                        ///need to find the noneroles that are directors now
                        var noneroledirectors = noneroles.Where(r => matcheddirectorids.Contains(r.Person.Netflix.Id.ToString())).ToList();
                        foreach (Role r in noneroledirectors)
                        {
                            if (r.InRole == RoleType.None)
                            {
                                r.InRole = RoleType.Director;
                            }
                            else ///have to make this check incase an actor is also the director (actors are checked first)
                            {
                                Role role = ftc.Roles.Create();
                                role.InRole = RoleType.Director;
                                role.Movie = r.Movie;
                                role.Person = r.Person;
                                ftc.Roles.Add(role);
                            }
                        }
                        ftc.SaveChanges();
                        if (m.Roles.Count(c => c.InRole == RoleType.Director) < nfdirectorids.Count)
                        {
                            ///need to find which roles haven't already been added.
                            var currentroles = m.Roles.Where(r => r.InRole == RoleType.Director).Select(r => r.Person.PersonId).ToList();
                            var directorsfordb = matcheddirectors.Where(r => !currentroles.Contains(r.PersonId)).ToList();
                            foreach (Models.Person p in directorsfordb)
                            {
                                ///2) add this movie as a new role on the movie
                                Role r = ftc.Roles.Create();
                                r.InRole = RoleType.Director;
                                r.Movie = m;
                                r.Person = p;
                                ftc.Roles.Add(r);
                            }
                        }
                        //if (m.Roles.Count(c => c.InRole == RoleType.Director) < netflixtitle.Directors.Count())
                        //{
                        //    ///need to find which roles haven't already been added.
                        //    var currentroles = m.Roles.Where(r => r.InRole == RoleType.Director).Select(r => r.Person.Netflix.Id).ToList();
                        //    var directorsfordb = netflixtitle.Directors.Where(r => !currentroles.Contains(r.Id)).Select(a => a.Id).ToList();
                        //    var directorsft = ftc.People.Where(p => directorsfordb.Contains(p.Netflix.Id)).ToList();
                        //    foreach (Models.Person p in directorsft)
                        //    {
                        //        ///2) add this movie as a new role on the movie
                        //        Role r = ftc.Roles.Create();
                        //        r.InRole = RoleType.Director;
                        //        r.Movie = m;
                        //        r.Person = p;
                        //        ftc.Roles.Add(r);
                        //    }
                        //}
                    }
                    #region Populate Similar Titles
                    using (profiler.Step("Populate Similar Titles"))
                    {
                        ///3) add all similar titles to the database similar to step 1 for people
                        var nftitleids = netflixtitle.SimilarTitles.Select(t => t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : "")).ToList();
                        var matchedtitleids = ftc.Movies.Where(t => nftitleids.Contains(t.Netflix.Id)).Select(t => t.Netflix.Id).ToList();
                        var titleidsfordatabase = nftitleids.Where(t => !matchedtitleids.Contains(t)).ToList();
                        var titlesfordatabase = netflixtitle.SimilarTitles.Where(t =>
                            {
                                var fullid = t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : "");
                                return titleidsfordatabase.Any(f => f == fullid);//fullid);
                            }).ToList();

                        foreach (FlixSharp.Holders.Title t in titlesfordatabase)
                        {
                            FilmTrove.Models.Movie ftmovie = ftc.Movies.Create();
                            GeneralHelpers.FillBasicTitle(ftmovie, t);
                            HashSet<Genre> genres = new HashSet<Genre>();
                            IEnumerable<String> missinggenres = null;
                            ///get the genres that exist in the database (local cache and db)
                            var dbgenreslocal = ftc.Genres.Local.Where(g => t.Genres.Contains(g.Name));
                            var dbgenres = ftc.Genres.Where(g => t.Genres.Contains(g.Name));
                            ///add them together into one non duplicated list
                            genres.AddRange(dbgenres);
                            genres.AddRange(dbgenreslocal);
                            ///get the names of the database genres
                            var genrenames = genres.Select(g => g.Name);
                            ///find the genres on the movie that aren't in the database
                            missinggenres = t.Genres.Where(g => !genrenames.Contains(g));
                            foreach (String genre in missinggenres)
                            {
                                Genre g = new Genre() { Name = genre };
                                ///add the genre to the list
                                genres.Add(g);
                                ftc.Genres.Add(g);
                            }
                            ///create all the genre-movie records
                            foreach (Genre g in genres)
                            {
                                MovieGenre gi = ftc.GenreItems.Create();
                                gi.Genre = g;
                                gi.Movie = ftmovie;
                                ftc.GenreItems.Add(gi);
                            }
                            ftc.Movies.Add(ftmovie);
                        }
                        m.Netflix.SimilarTitles = netflixtitle.SimilarTitles.Select(t => t.IdUrl).ToList();
                        m.Netflix.NeedsUpdate = false;
                    }
                    #endregion
                    #region Populate Genres
                    using (profiler.Step("Populate Genres"))
                    {
                        var dbgl = ftc.Genres.Local.Where(g => netflixtitle.Genres.Contains(g.Name));
                        var dbg = ftc.Genres.Where(g => netflixtitle.Genres.Contains(g.Name));
                        HashSet<Genre> gs = new HashSet<Genre>();
                        gs.AddRange(dbg);
                        gs.AddRange(dbgl);

                        var gn = gs.Select(g => g.Name);
                        var mgs = netflixtitle.Genres.Where(g => !gn.Contains(g));
                        foreach (String genre in mgs)
                        {
                            Genre g = new Genre() { Name = genre };
                            gs.Add(g);
                            ftc.Genres.Add(g);
                        }
                        //newmovie.Genres = netflixmovie.Genres;
                        foreach (Genre g in gs)
                        {
                            MovieGenre gi = ftc.GenreItems.Create();
                            gi.Genre = g;
                            gi.Movie = m;
                            ftc.GenreItems.Add(gi);
                        }
                    }
                    #endregion
                    m.Netflix.Synopsis = netflixtitle.Synopsis;
                    m.Description = netflixtitle.Synopsis;
                }
            }
            else
            {
                using (profiler.Step("Correct Role.Nones"))
                {
                    var noneroles = m.Roles.Where(r => r.InRole == RoleType.None).ToList();
                    if (noneroles.Count > 0)
                    {
                        ///need to find the roles that are already added (under the RoleType.None) so i can correct those
                        ///need to find the noneroles that are actors now
                        Title netflixtitle = new Title();
                        netflixtitle.Actors = await Netflix.Fill.Titles.GetActors(m.Netflix.IdUrl);
                        netflixtitle.Directors = await Netflix.Fill.Titles.GetDirectors(m.Netflix.IdUrl);

                        foreach (var nonerole in noneroles)
                        {
                            FlixSharp.Holders.Person nfactor = netflixtitle.Actors.Where(p => p.Id == nonerole.Person.Netflix.Id).SingleOrDefault();
                            if (nfactor != null)
                            {
                                nonerole.InRole = RoleType.Actor;
                            }
                            FlixSharp.Holders.Person nfdirector = netflixtitle.Directors.Where(p => p.Id == nonerole.Person.Netflix.Id).SingleOrDefault();
                            if (nfdirector != null)
                            {
                                if (nonerole.InRole != RoleType.None)
                                {
                                    ///if this was an actor also then duplicate it for a director role.
                                    Role r = ftc.Roles.Create();
                                    r.InRole = RoleType.Director;
                                    r.Movie = nonerole.Movie;
                                    r.Person = nonerole.Person;
                                    ftc.Roles.Add(r);
                                }
                                else
                                    nonerole.InRole = RoleType.Director;
                            }
                        }
                    }
                }
            }
            using (profiler.Step("Save Changes"))
            {
                m.ViewCount++;
                ftc.SaveChanges();
            }
            #endregion
            using (profiler.Step("Get Similar titles ready"))
            {
                if (m.Netflix.SimilarTitles.Count > 0)
                {
                    var similars = m.Netflix.SimilarTitles.Take(20).Select(f =>
                        {
                            MatchCollection match = Regex.Matches(f, "[0-9]{3,10}");
                            var r = match.Cast<Match>().Select(t => t.Value).Take(2);
                            return r.First() + (r.Count() > 1 ? ";" + r.LastOrDefault() : "");
                        }).ToList();
                    ViewBag.Similars = ftc.Movies.Where(t => similars.Contains(t.Netflix.Id)).ToArray();
                }
                ViewBag.Roles = m.Roles;
                ViewBag.Movie = m;

                ViewBag.Id = id;
            }
            return View();
        }
    }
}
