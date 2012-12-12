using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Collections;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.Data;
using FlixSharp.Holders;
using FilmTrove.Code;

namespace FilmTrove.Models
{
    public class Movie : Dateable//, IEquatable<Movie>
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 MovieId { get; set; }
        [MaxLength(250)]
        public String Title { get; set; }
        [MaxLength(100)]
        public String AltTitle { get; set; }

        public String Description { get; set; }
        public String Rating { get; set; }
        public RatingType RatingType { get; set; }
        public String BestPosterUrl { get; set; }
        public Int32 Year { get; set; }
        public Int32? RunTime { get; set; }

        public NetflixInfo Netflix { get; set; }
        public ImdbInfo Imdb { get; set; }
        public AmazonInfo Amazon { get; set; }
        public RottenTomatoesInfo RottenTomatoes { get; set; }
        public RedBoxInfo RedBox { get; set; }

        public virtual ICollection<MovieGenre> Genres { get; set; }

        public virtual ICollection<UserListItem> OnLists { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

        public Int64 ViewCount { get; set; } ///eventual shard
        public Int64 SearchCount { get; set; } ///eventual shard

        public String GetDetailsUrl()
        {
            return "/Movies/Details/" + MovieId + "/" + Title.UrlFriendly(Year);
        }
        public String GetAwards()
        {
            if (Netflix.AwardsCompact != "")
            {
                String s = "<ul>";
                foreach (String award in Netflix.Awards)
                {
                    String[] awards = award.Split(new[] { ";#" }, StringSplitOptions.None);
                    String AwardName = awards[0];
                    String PersonNetflixId = awards[1];
                    String AwardType = awards[2];
                    Boolean Winner = Boolean.Parse(awards[3]);
                    String Year = awards[4];
                    s += "<li>" + Year + " " + AwardName + " " + AwardType + "</li>";
                }
                s += "</ul>";
                return s;
            }
            return "";
        }
        public override String ToString()
        {
            return Title;
        }

        public Movie()
        {
            Netflix = new NetflixInfo();
            Imdb = new ImdbInfo();
            Amazon = new AmazonInfo();
            RottenTomatoes = new RottenTomatoesInfo();
            RedBox = new RedBoxInfo();

            ViewCount = 0;

            OnLists = new List<UserListItem>();
            Roles = new List<Role>();
        }

        //public override bool Equals(Object obj)
        //{
        //    return this.Equals((Movie)obj);
        //}
        //public bool Equals(Movie other)
        //{
        //    return GetHashCode() == other.GetHashCode();
        //}
        //public override int GetHashCode()
        //{
        //    return MovieId == 0 ? base.GetHashCode() : MovieId.GetHashCode();
        //}
    }

    public class Genre// : IEquatable<Genre>
    {
        public Int32 GenreId { get; set; }
        public String Name { get; set; }
        public Genre()
        {
            Name = "";
        }

        //public override bool Equals(Object obj)
        //{
        //    return this.Equals((Genre)obj);
        //}
        //public bool Equals(Genre other)
        //{
        //    return GetHashCode() == other.GetHashCode();
        //}
        //public override int GetHashCode()
        //{
        //    return GenreId == 0 ? base.GetHashCode() : GenreId.GetHashCode();
        //}
    }

    public class MovieGenre// : IEquatable<MovieGenre>
    {
        public Int32 MovieGenreId { get; set; }
        public virtual Genre Genre { get; set; }
        public virtual Movie Movie { get; set; }
        public override String ToString()
        {
            //if (Genre != null)
                return Genre.Name;
            //else
            //    return base.ToString();
        }

        //public override bool Equals(Object obj)
        //{
        //    return this.Equals((MovieGenre)obj);
        //}
        //public bool Equals(MovieGenre other)
        //{
        //    return GetHashCode() == other.GetHashCode();
        //}
        //public override int GetHashCode()
        //{
        //    return MovieGenreId == 0 ? base.GetHashCode() : MovieGenreId.GetHashCode();
        //}
    }
}