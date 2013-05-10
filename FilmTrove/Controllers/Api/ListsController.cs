using FilmTrove.Code;
using FilmTrove.Models;
using FlixSharp;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FilmTrove.Controllers.Api
{
    /// <summary>
    /// Movie lists (from RT) and user lists
    /// </summary>
    public class ListsController : ApiController
    {

        #region RottenTomatoes Lists
        [HttpGet]
        public async Task<List<Movie>> OpeningMovies()
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                ftc.Configuration.ProxyCreationEnabled = false;
                var OpeningMoviesTask = RottenTomatoes.Fill.Lists.GetOpeningMovies(Limit: 20); 
                return await GeneralHelpers.GetDatabaseMoviesRottenTomatoes(OpeningMoviesTask, ftc);
            }
        }
        [HttpGet]
        public async Task<List<Movie>> UpcomingMovies()
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                ftc.Configuration.ProxyCreationEnabled = false;
                var UpcomingMoviesTask = RottenTomatoes.Fill.Lists.GetUpcomingMovies(Limit: 20);
                return await GeneralHelpers.GetDatabaseMoviesRottenTomatoes(UpcomingMoviesTask, ftc);
            }
        }
        [HttpGet]
        public async Task<List<Movie>> NewReleaseDVDs()
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                ftc.Configuration.ProxyCreationEnabled = false;
                var NewReleaseDVDsTask = RottenTomatoes.Fill.Lists.GetNewReleaseDVDs(Limit: 20);
                return await GeneralHelpers.GetDatabaseMoviesRottenTomatoes(NewReleaseDVDsTask, ftc);
            }
        }
        [HttpGet]
        public async Task<List<Movie>> UpcomingDVDs()
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                ftc.Configuration.ProxyCreationEnabled = false;
                var UpcomingDVDsTask = RottenTomatoes.Fill.Lists.GetUpcomingDVDs(Limit: 20);
                return await GeneralHelpers.GetDatabaseMoviesRottenTomatoes(UpcomingDVDsTask, ftc);
            }
        }
        #endregion

        // GET api/lists
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/lists/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/lists
        public void Post([FromBody]string value)
        {
        }

        // PUT api/lists/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/lists/5
        public void Delete(int id)
        {
        }
    }
}
