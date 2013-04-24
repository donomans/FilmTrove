using FilmTrove.Code.Netflix;
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
using FlixSharp.Holders;

namespace FilmTrove.Controllers.Api
{
    public class PeopleController : ApiController
    {
        [HttpGet]
        public async Task<Person> Details([FromUri] Int32 id)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                Person p = ftc.People
                    .Include("Roles.Movie")
                    .Where(person => person.PersonId == id).Single();

                var n = UpdateNetflix(p, ftc);

                await n;

                ftc.SaveChanges();

                return p;
            }
        }

        private async Task UpdateNetflix(Person p, FilmTroveContext ftc)
        {
            Task<FlixSharp.Holders.Netflix.Person> nfp = null;
            Random ran = new Random();

            if (p.Netflix.NeedsUpdate ||
                p.Netflix.LastFullUpdate > DateTime.Now.AddDays(20).AddDays(ran.Next(-5, 5)))
            {
                nfp = Netflix.Fill.People.GetCompletePerson(p.Netflix.IdUrl, true);//Randomized().
            }

            if (nfp != null)
            {
                FlixSharp.Holders.Netflix.Person netflixperson = null;
                List<Movie> ftmoviesfound = null;

                Dictionary<Movie, People> actors = new Dictionary<Movie, People>();
                Dictionary<Movie, People> directors = new Dictionary<Movie, People>();

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

                var ftmoviesfoundids = ftmoviesfound.Select(t => t.Netflix.Id).ToList();
                var netflixmoviestoadd = netflixperson.Filmography.Where(t => !ftmoviesfoundids.Contains(t.Id + (t.SeasonId != "" ? ";" + t.SeasonId : ""))).ToList();
                foreach (FlixSharp.Holders.Netflix.Title title in netflixmoviestoadd)
                {
                    var m = ftc.Movies.Create();
                    NetflixHelpers.FillBasicNetflixTitle(m, title);

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
            }
            p.Netflix.NeedsUpdate = false;

            ftc.SaveChanges();
        }

        // GET api/people
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/people/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/people
        public void Post([FromBody]string value)
        {
        }

        // PUT api/people/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/people/5
        public void Delete(int id)
        {
        }
    }
}
