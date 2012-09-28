using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Collections;
using System.Linq;
using System.Web;

namespace FilmTrove.Models
{
    public class Movie
    {
        [Key]
        public String MovieId { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public MpaaRating MpaaRating { get; set; }
        public String PosterId { get; set; }
        public Int32 Year { get; set; }
        public Int32 RunTime { get; set; }

        public String NetflixId { get; set; }
        public String ImdbId { get; set; }

        public HashSet<String> Genres { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DateAdded { get; set; }

        public virtual HashSet<String> Director { get; set; }
        public virtual HashSet<String> Actors { get; set; }
        public virtual HashSet<String> Writer { get; set; }

        public Int64 Count { get; set; } ///need to shard this somehow?

        public override string ToString()
        {
            return Title;
        }

        public Movie()
        {
            Genres = new HashSet<String>();
            Director = new HashSet<String>();
            Actors = new HashSet<String>();
            Writer = new HashSet<String>();
        }
    }

    public class Person
    {
        [Key]
        public String PersonId { get; set; }
        public String Name { get; set; }

        public virtual HashSet<String> Acted { get; set; }
        public virtual HashSet<String> Directed { get; set; }
        public virtual HashSet<String> Wrote { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public Person()
        {
            Acted = new HashSet<String>();
            Directed = new HashSet<String>();
            Wrote = new HashSet<String>();
        }
    }

    public enum MpaaRating
    {
        Unrated,
        R,
        PG13,
        PG,
        G
    }

    public class MoviesContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Person> People { get; set; }
        
        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Movie>().
        //      HasMany(c => c.Director).
        //      WithMany(p => p.Directed).
        //      Map(
        //       m =>
        //       {
        //           m.MapLeftKey("MovieId");
        //           m.MapRightKey("PersonId");
        //           m.ToTable("MovieDirected");
        //       });

        //    modelBuilder.Entity<Movie>().
        //      HasMany(c => c.Actors).
        //      WithMany(p => p.Acted).
        //      Map(
        //       m =>
        //       {
        //           m.MapLeftKey("MovieId");
        //           m.MapRightKey("PersonId");
        //           m.ToTable("MovieActed");
        //       });

        //    modelBuilder.Entity<Movie>().
        //      HasMany(c => c.Writer).
        //      WithMany(p => p.Wrote).
        //      Map(
        //       m =>
        //       {
        //           m.MapLeftKey("MovieId");
        //           m.MapRightKey("PersonId");
        //           m.ToTable("MovieWrote");
        //       });
        //}
    }
}