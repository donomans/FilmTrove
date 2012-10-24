using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FilmTrove.Controllers.Api
{
    public class UserServicesController : ApiController
    {
        [HttpPost]
        public Boolean Add(String movieid, String movietitle, String listid, String listtitle)
        {
            return true;
        }


        public String New(String title)
        {
            return "";
        }
    }
}
