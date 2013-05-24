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
    }
}
