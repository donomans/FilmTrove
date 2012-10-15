using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FilmTrove.Controllers.Api
{
    public class NetflixController : ApiController
    {
        // GET api/netflix
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/netflix/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/netflix
        public void Post([FromBody]string value)
        {
        }

        // PUT api/netflix/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/netflix/5
        public void Delete(int id)
        {
        }
    }
}
