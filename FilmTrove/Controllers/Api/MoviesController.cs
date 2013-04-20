using FilmTrove.Code;
using FilmTrove.Models;
using FlixSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace FilmTrove.Controllers.Api
{
    public class MoviesController : ApiController
    {

        // GET api/movies
        public async Task<Movie> Details([FromUri] Int32 id)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                Movie m = ftc.Movies
                   .Include("Roles.Person").Include("Genres.Genre")
                   .Where(movie => movie.MovieId == id).Single();

                ///need to:
                ///1) Check if it needs to update netflix
                Task n =  UpdateNetflix(m, ftc);
                ///2) Check if it needs to update rotten tomatoes data
                Task rt = UpdateRottenTomatoes(m, ftc);
                ///3) Check if it needs to update amazon
                Task a = UpdateAmazon(m, ftc);


                await n;
                await rt;
                await a;

                m.ViewCount++;
                ftc.SaveChanges();

                return m;
            }
        }

        private async Task UpdateNetflix(Movie m, FilmTroveContext ftc)
        {
            Random ran = new Random();

            Task<FlixSharp.Holders.Netflix.Title> nfm = null;
            FlixSharp.Holders.Netflix.Title netflixtitle = null;

            if (m.Netflix.NeedsUpdate || (m.DateLastModified.HasValue && m.DateLastModified > DateTime.Now.AddDays(20).AddDays(ran.Next(-5, 5))))
            {
                if (m.Netflix.IdUrl != "")
                    nfm = Netflix.Fill.Titles.GetCompleteTitle(m.Netflix.IdUrl);//Randomized().
                else
                {
                    netflixtitle = await FindNetflixMatch(m);
                    nfm = Netflix.Fill.Titles.GetCompleteTitle(netflixtitle.IdUrl);
                }
            }


            #region Fill Netflix
            if (nfm != null || netflixtitle != null)
            {

                if (nfm != null)
                    netflixtitle = await nfm;
                ///populate all the netflix information
                GeneralHelpers.FillBasicNetflixTitle(m, netflixtitle);

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

                ///need to find the roles that are already added (under the RoleType.None) so i can correct those
                var noneroles = m.Roles.Where(r => r.InRole == RoleType.None).ToList();

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
                            GeneralHelpers.FillBasicNetflixPerson(ftperson, p);
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
                            GeneralHelpers.FillBasicNetflixPerson(ftperson, p);
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
                #region Populate Similar Titles
                ///3) add all similar titles to the database similar to step 1 for people
                var nftitleids = netflixtitle.SimilarTitles.Select(t => t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : "")).ToList();
                var matchedtitleids = ftc.Movies.Where(t => nftitleids.Contains(t.Netflix.Id)).Select(t => t.Netflix.Id).ToList();
                var titleidsfordatabase = nftitleids.Where(t => !matchedtitleids.Contains(t)).ToList();
                var titlesfordatabase = netflixtitle.SimilarTitles.Where(t =>
                {
                    var fullid = t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : "");
                    return titleidsfordatabase.Any(f => f == fullid);//fullid);
                }).ToList();

                foreach (FlixSharp.Holders.Netflix.Title t in titlesfordatabase)
                {
                    FilmTrove.Models.Movie ftmovie = ftc.Movies.Create();
                    GeneralHelpers.FillBasicNetflixTitle(ftmovie, t);
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
                #endregion
                #region Populate Genres
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
                #endregion
                m.Netflix.Synopsis = netflixtitle.Synopsis;
                m.Description = netflixtitle.Synopsis;

            }
            else
            {
                var noneroles = m.Roles.Where(r => r.InRole == RoleType.None).ToList();
                if (noneroles.Count > 0)
                {
                    ///need to find the roles that are already added (under the RoleType.None) so i can correct those
                    ///need to find the noneroles that are actors now
                    var nftitle = new FlixSharp.Holders.Netflix.Title();
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
            #endregion

        }

        private static async Task<FlixSharp.Holders.Netflix.Title> FindNetflixMatch(Movie m)
        {
            var searchtitles = await Netflix.Search.SearchTitles(m.Title);
            return searchtitles
                .Select(mv => mv as FlixSharp.Holders.Netflix.Title)
                .FirstOrDefault(mv =>
                {
                    Int32 maxlength = (Int32)(m.Title.Length * 1.2);
                    Int32 minlength = (Int32)(m.Title.Length * .8);
                    //mv.ShortTitle == m.AltTitle ||
                    ///this might be bad as it's potentilaly comparing a null value to a null value
                    ///or a blank to a blank -- false positives
                    return ((mv.FullTitle == m.Title && mv.FullTitle.Length >= minlength && mv.FullTitle.Length <= maxlength)
                    || (mv.ShortTitle.Contains(m.Title) && mv.FullTitle.Length >= minlength && mv.FullTitle.Length <= maxlength)
                    || (mv.FullTitle.Contains(m.Title) && mv.FullTitle.Length >= minlength && mv.FullTitle.Length <= maxlength)
                    || (m.Title.Contains(mv.ShortTitle) && mv.FullTitle.Length >= minlength && mv.FullTitle.Length <= maxlength))
                    && (mv.Year == m.Year
                        || mv.Year + 1 == m.Year
                        || mv.Year - 1 == m.Year);
                });
        }
        private async Task UpdateRottenTomatoes(Movie m, FilmTroveContext ftc)
        {
            Random ran = new Random();

            Task<FlixSharp.Holders.RottenTomatoes.Title> rtm = null;
            FlixSharp.Holders.RottenTomatoes.Title rottentomatoestitle = null;
        }
        private async Task UpdateAmazon(Movie m, FilmTroveContext ftc)
        {
        }

        // GET api/movies/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/movies
        public void Post([FromBody] Movie movie)
        {
        }

        // PUT api/movies/5
        public void Put(int id, [FromBody]Movie movie)
        {
        }

        // DELETE api/movies/5
        public void Delete(int id)
        {
        }

        [HttpPost]
        public Int64 UpdateCount([FromUri] Int32 Id)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                Movie m = ftc.Movies.Find(Id);
                if (m != null)
                {
                    m.ViewCount++;
                    Int64 currentcount = m.ViewCount;
                    ftc.SaveChanges();
                    return currentcount;
                }
                return 0;
            }
        }
    }
}
