using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Collections;
using System.Linq;
using System.Web;
//using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Data;

namespace FilmTrove.Models
{
    public class Movie : Dateable
    {
        [Key]
        public String MovieId { get; set; }
        public String Title { get; set; }
        public String AltTitle { get; set; }

        public String Description { get; set; }
        public MpaaRating MpaaRating { get; set; }
        public String PosterId { get; set; }
        public Int32 Year { get; set; }
        public Int32 RunTime { get; set; }

        public NetflixInfo Netflix { get; set; }
        public ImdbInfo Imdb { get; set; }
        public AmazonInfo Amazon { get; set; }

        public HashSet<String> Genres { get; set; }

        public virtual HashSet<Movie> SimilarTitles { get; set; }

        public virtual HashSet<Person> Director { get; set; }
        public virtual HashSet<Person> Actors { get; set; }
        public virtual HashSet<Person> Writer { get; set; }

        //public Int64 Count { get; set; } ///need to shard this somehow?

        public override string ToString()
        {
            return Title;
        }

        public Movie()
        {
            Netflix = new NetflixInfo();
            Imdb = new ImdbInfo();
            Amazon = new AmazonInfo();

            Genres = new HashSet<String>();

            SimilarTitles = new HashSet<Movie>();

            Director = new HashSet<Person>();
            Actors = new HashSet<Person>();
            Writer = new HashSet<Person>();
        }
    }


    [ComplexType]
    public class NetflixInfo : ProviderInfo
    {
        public NetflixInfo()
        {
            Id = "";
            Url = "";
            AvgRating = "";
            NeedsUpdate = true;
        }
    }
    [ComplexType]
    public class ImdbInfo : ProviderInfo
    {
        public ImdbInfo()
        {
            Id = "";
            Url = "";
            AvgRating = "";
            NeedsUpdate = true;
        }
    }
    [ComplexType]
    public class AmazonInfo : ProviderInfo
    {
        public AmazonInfo()
        {
            Id = "";
            Url = "";
            AvgRating = "";
            NeedsUpdate = true;
        }
    }

    public abstract class ProviderInfo
    {
        public String Id { get; set; }
        public String Url { get; set; }
        public String AvgRating { get; set; }
        public Boolean NeedsUpdate { get; set; }
    }

    public class Person : Dateable
    {
        [Key]
        public String PersonId { get; set; }
        public String Name { get; set; }

        public virtual HashSet<Movie> Acted { get; set; }
        public virtual HashSet<Movie> Directed { get; set; }
        public virtual HashSet<Movie> Wrote { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public Person()
        {
            Acted = new HashSet<Movie>();
            Directed = new HashSet<Movie>();
            Wrote = new HashSet<Movie>();
        }
    }

    public class Dateable
    {
        public DateTime? DateCreated { get; set; }
        public DateTime? DateLastModified { get; set; }
    }

    public enum MpaaRating
    {
        Unrated,
        R,
        PG13,
        PG,
        G
    }
}