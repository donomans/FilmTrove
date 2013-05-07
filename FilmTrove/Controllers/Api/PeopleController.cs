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
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using System.Web;
using System.Web.Hosting;

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
                nfp = Netflix.Fill.People.GetCompletePerson(p.Netflix.IdUrl, true);
            }

            if (nfp != null)
            {
                FlixSharp.Holders.Netflix.Person netflixperson = await nfp;

                List<Movie> ftmoviesfound = 
                    NetflixHelpers.GetNetflixPeopleMoviesFound(p, ftc, netflixperson);

                var ftmoviesfoundids = ftmoviesfound.Select(t => t.Netflix.Id).ToList();
                var netflixmoviestoadd = netflixperson.Filmography
                    .Where(t => !ftmoviesfoundids.Contains(t.FullId))
                    .ToList();

                if (netflixmoviestoadd.Count > 0)
                {
                    //var Cache = new System.Web.Caching.Cache();
                    //var ramindex = (RAMDirectory)Cache.Get("ftramindex");
                    //RAMDirectory ramindex = (RAMDirectory)HttpContext.Current.Items["ftramindex"];
                    //if (ramindex == null)
                    //    throw new MissingMemberException("ramindex was null");
                    using (var index = FSDirectory.Open(HostingEnvironment.MapPath("/App_Data/index")))
                    {
                        using (IndexWriter iw = new IndexWriter(index,
                        new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                        IndexWriter.MaxFieldLength.LIMITED))
                        {
                            foreach (FlixSharp.Holders.Netflix.Title title in netflixmoviestoadd)
                            {
                                var m = ftc.Movies.Create();
                                NetflixHelpers.FillBasicNetflixTitle(m, title);
                                NetflixHelpers.FillNetflixGenres(m, ftc, title);

                                Document d = new Document();
                                d.Add(new Field("NetflixId", title.FullId,
                                    Field.Store.YES, Field.Index.NO));
                                d.Add(new Field("Title", title.FullTitle,
                                    Field.Store.YES, Field.Index.ANALYZED));
                                d.Add(new Field("AltTitle", title.ShortTitle,
                                    Field.Store.YES, Field.Index.ANALYZED));
                                iw.AddDocument(d);

                                ftc.Movies.Add(m);

                                Role r = ftc.Roles.Create();
                                r.Movie = m;
                                r.Person = p;
                                ftc.Roles.Add(r);
                            }

                            iw.Optimize();
                        }
                    }
                }
            }
            p.Netflix.NeedsUpdate = false;
            p.Netflix.LastFullUpdate = DateTime.Now;

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
