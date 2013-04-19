using FilmTrove.Models;
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

                return m;
            }
        }

        private async Task UpdateNetflix(Movie m, FilmTroveContext ftc)
        {
        }
        private async Task UpdateRottenTomatoes(Movie m, FilmTroveContext ftc)
        {
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
        public void Post([FromBody]string value)
        {
        }

        // PUT api/movies/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/movies/5
        public void Delete(int id)
        {
        }
    }
}
