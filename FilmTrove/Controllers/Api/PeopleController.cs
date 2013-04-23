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
