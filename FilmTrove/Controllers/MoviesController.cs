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
using FlixSharp.Holders.Netflix;
using FilmTrove.Code.Netflix;
using FilmTrove.Code.RottenTomatoes;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using System.Web.Hosting;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

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
            Models.Movie m = ftc.Movies
                .Include("Roles.Person").Include("Genres.Genre")
                .Where(movie => movie.MovieId == movieid).Single();//.Include("Roles.Person").Where(movie => movie.MovieId == movieid).Single();
            
            Task<FlixSharp.Holders.Netflix.Title> nfm = null;
            FlixSharp.Holders.Netflix.Title netflixtitle = null;

            Task<FlixSharp.Holders.RottenTomatoes.Title> rtm = null;
            FlixSharp.Holders.RottenTomatoes.Title rottentomatoestitle = null;

            Random ran = new Random();

            if (m.Netflix.NeedsUpdate || (m.DateLastModified.HasValue && m.DateLastModified > DateTime.Now.AddDays(20).AddDays(ran.Next(-5, 5))))
            {
                if (m.Netflix.IdUrl != "")
                    nfm = Netflix.Fill.Titles.GetCompleteTitle(m.Netflix.IdUrl, OnUserBehalf: true);//Randomized().
                else
                {
                    ////need to find the best match
                    netflixtitle = await NetflixHelpers.FindNetflixMatch(m);

                    nfm = Netflix.Fill.Titles.GetCompleteTitle(netflixtitle.IdUrl, OnUserBehalf: true);
                }
            }
            using (profiler.Step("Populate Amazon Movie"))
            {
                if (m.Amazon.NeedsUpdate || (m.Amazon.LastPriceUpdate.HasValue && 
                    (m.Amazon.LastPriceUpdate.Value  > DateTime.Now.AddDays(1))))
                {
                    if (m.Amazon.Id == null || m.Amazon.Id == "" ||
                        m.Amazon.LastFullUpdate.HasValue &&
                        (m.Amazon.LastFullUpdate > DateTime.Now.AddDays(28).AddDays(ran.Next(-5, 5))))
                    {
                        ///1) do a full lookup query to match up title and fill in id with priority to blu-ray
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
                //if (m.RottenTomatoes.NeedsUpdate ||
                //    (m.RottenTomatoes.LastFullUpdate.HasValue &&
                //    (m.RottenTomatoes.LastFullUpdate > DateTime.Now.AddDays(20).AddDays(ran.Next(-5, 5)))))
                //{
                    if (m.RottenTomatoes.Id != "")
                    {
                        ///1) title match like with amazon or use Id if present
                        rtm = FlixSharp.RottenTomatoes.Fill.Titles.GetMoviesInfo(m.RottenTomatoes.Id);
                    }
                    else
                    {
                        ////need to find the best match
                        var searchtitles = await RottenTomatoes.Search.SearchTitles(m.Title);
                        
                        rottentomatoestitle = searchtitles
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
                        m.RottenTomatoes.NeedsUpdate = false; //gave it a try, wait a while
                    }
                //}
            }
            #region Fill Netflix
            if (nfm != null || netflixtitle != null)
            {
                using (profiler.Step("Fill Netflix Movie"))
                {
                    using (profiler.Step("Await Netflix Title"))
                    {
                        if(nfm != null)
                            netflixtitle = await nfm;
                    }
                    using (profiler.Step("Populate basic database 'm' record"))
                    {
                        ///populate all the netflix information
                        NetflixHelpers.FillBasicNetflixTitle(m, netflixtitle);

                        m.Netflix.NeedsUpdate = false;
                        m.Netflix.Awards = netflixtitle.Awards.Select(a =>
                            a.AwardName + ";#" + a.PersonIdUrl + ";#" +
                            a.Type + ";#" + a.Winner + ";#" + a.Year).DefaultIfEmpty().ToList();
                        m.RunTime = netflixtitle.RunTime;

                        foreach (FlixSharp.Holders.Netflix.FormatAvailability f in netflixtitle.Formats)
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
                        m.Netflix.SimilarTitles = netflixtitle.SimilarTitles.Select(t => t.FullId).ToList();
                        m.Netflix.Synopsis = netflixtitle.Synopsis;
                    }

                    ///need to find the roles that are already added (under the RoleType.None) so i can correct those
                    var noneroles = m.Roles.Where(r => r.InRole == RoleType.None).ToList();

                    using (profiler.Step("Populate Actors"))
                    {
                        var nfactorids = netflixtitle.Actors.Select(t => t.Id).ToList();
                        var matchedactors = ftc.People.Where(t => nfactorids.Contains(t.Netflix.Id)).ToList();
                        var matchedactorids = matchedactors.Select(p => p.Netflix.Id).ToList();
                        var actorsfordatabase = netflixtitle.Actors.Where(t => !matchedactorids.Contains(t.Id)).ToList();

                        foreach (FlixSharp.Holders.Netflix.Person p in actorsfordatabase)
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
                                    NetflixHelpers.FillBasicNetflixPerson(ftperson, p);
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
                        foreach (FlixSharp.Holders.Netflix.Person p in directorsfordatabase)
                        {
                            ///1) find the cast and loop through and add the people to the database in a similar manner as AsyncHelpers.GetDatabasePeople
                            FilmTrove.Models.Person ftperson = ftc.People.Local.Where(t => t.Netflix.Id == p.Id).SingleOrDefault();
                            if (ftperson == null || ftperson.Name == null || ftperson.Name == "")
                            {
                                ftperson = ftc.People.Where(t => t.Netflix.Id == p.Id).SingleOrDefault();
                                if (ftperson == null || ftperson.Name == null || ftperson.Name == "")
                                {
                                    ftperson = ftc.People.Create();
                                    NetflixHelpers.FillBasicNetflixPerson(ftperson, p);
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
                        var nftitleids = netflixtitle.SimilarTitles.Select(t => t.FullId).ToList();
                        var matchedtitleids = ftc.Movies.Where(t => nftitleids.Contains(t.Netflix.Id)).Select(t => t.Netflix.Id).ToList();
                        var titleidsfordatabase = nftitleids.Where(t => !matchedtitleids.Contains(t)).ToList();
                        var titlesfordatabase = netflixtitle.SimilarTitles.Where(t =>
                            {
                                var fullid = t.FullId;
                                return titleidsfordatabase.Any(f => f == fullid);//fullid);
                            }).ToList();
                        if (titlesfordatabase.Count > 0)
                        {
                            using (var index = FSDirectory.Open(HostingEnvironment.MapPath("/App_Data/index")))
                            {
                                using (IndexWriter iw = new IndexWriter(index,
                                    new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                                    IndexWriter.MaxFieldLength.LIMITED))
                                {
                                    foreach (FlixSharp.Holders.Netflix.Title t in titlesfordatabase)
                                    {
                                        FilmTrove.Models.Movie ftmovie = ftc.Movies.Create();
                                        NetflixHelpers.FillBasicNetflixTitle(ftmovie, t);
                                        NetflixHelpers.FillNetflixGenres(ftmovie, ftc, t);

                                        Document d = new Document();
                                        d.Add(new Field("NetflixId", t.FullId,
                                            Field.Store.YES, Field.Index.NO));
                                        d.Add(new Field("Title", t.FullTitle,
                                            Field.Store.YES, Field.Index.ANALYZED));
                                        d.Add(new Field("AltTitle", t.ShortTitle,
                                            Field.Store.YES, Field.Index.ANALYZED));
                                        iw.AddDocument(d);

                                        ftc.Movies.Add(ftmovie);
                                    }

                                    iw.Optimize();
                                }
                            }
                        }
                        m.Netflix.SimilarTitles = netflixtitle.SimilarTitles.Select(t => t.IdUrl).ToList();
                        m.Netflix.NeedsUpdate = false;
                    }
                    #endregion
                    #region Populate Genres
                    using (profiler.Step("Populate Genres"))
                    {
                        NetflixHelpers.FillNetflixGenres(m, ftc, netflixtitle);
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
                        var nftitle = new Title();
                        nftitle.Actors = await Netflix.Fill.Titles.GetActors(m.Netflix.IdUrl);
                        nftitle.Directors = await Netflix.Fill.Titles.GetDirectors(m.Netflix.IdUrl);

                        foreach (var nonerole in noneroles)
                        {
                            FlixSharp.Holders.Netflix.Person nfactor = nftitle.Actors.Where(p => p.Id == nonerole.Person.Netflix.Id).SingleOrDefault() as FlixSharp.Holders.Netflix.Person;
                            if (nfactor != null)
                            {
                                nonerole.InRole = RoleType.Actor;
                            }
                            FlixSharp.Holders.Netflix.Person nfdirector = nftitle.Directors.Where(p => p.Id == nonerole.Person.Netflix.Id).SingleOrDefault() as FlixSharp.Holders.Netflix.Person;
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
            #region Fill RottenTomatoes
            if (rtm != null || rottentomatoestitle != null)
            {
                using (profiler.Step("Fill Rotten Tomatoes Movie"))
                {
                    using (profiler.Step("Await Rotten Tomatoes Title"))
                    {
                        if(rtm != null)
                            rottentomatoestitle = await rtm;
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
                    RottenTomatoesHelpers.FillRottenTomatoesTitle(m, rottentomatoestitle);
                    m.RottenTomatoes.NeedsUpdate = false;
                    ftc.SaveChanges();
                }
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
