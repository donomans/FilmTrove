using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FilmTrove.Controllers.Api
{
    public class HomeController : ApiController
    {
        // GET api/home
        public string Get()
        {
            return "I don't think this is where you want to be.";
        }

        //// GET api/home/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/home
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/home/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/home/5
        //public void Delete(int id)
        //{
        //}
    }
}
