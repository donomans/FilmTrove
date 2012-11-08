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
    public class Movie : Dateable
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 MovieId { get; set; }
        public String Title { get; set; }
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

        public List<String> Genres
        {
            get { return _Genres; }
            set { _Genres = value; }
        }
        private List<String> _Genres { get; set; }
        public String GenresCompact
        {
            get { return _Genres != null ? String.Join(";#!", _Genres) : null; }
            set { _Genres = value != null ? value.Split(new[] { ";#!" }, StringSplitOptions.RemoveEmptyEntries).ToList() : null; }
        }

        public virtual ICollection<UserList> OnLists { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

        //public FlixSharp.Holders.NetflixType Type { get; set; }

        public Int64 ViewCount { get; set; } ///eventual shard
        public Int64 SearchCount { get; set; } ///eventual shard

        public String GetDetailsUrl()
        {
            return "/Movies/Details/" + MovieId + "/" + Title.UrlFriendly();
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
            
            ViewCount = 0;

            OnLists = new List<UserList>();
            Roles = new List<Role>();
        }
    }


}