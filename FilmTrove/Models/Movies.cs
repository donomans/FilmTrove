using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FilmTrove.Models
{
    public class Movie
    {
        public String MovieId { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public MpaaRating MpaaRating { get; set; }
        public String PosterId { get; set; }
        public Int32 Year { get; set; }

        public List<String> Director { get; set; } ///this needs ot be an id eventually
        public List<String> Actors { get; set; }
        public List<String> Writer { get; set; }

        public Int64 Count { get; set; } ///need to shard this somehow?
    }

    public enum MpaaRating
    {
        R,
        PG13,
        PG,
        G
    }

    public class MoviesContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
    }
}