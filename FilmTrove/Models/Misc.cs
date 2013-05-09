using FlixSharp.Holders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FilmTrove.Models
{

    [ComplexType]
    public class NetflixInfo : ProviderInfo
    {
        public NetflixInfo()
        {
            Id = "";
            Url = "";
            IdUrl = "";
            AvgRating = null;
            NeedsUpdate = true;
            SeasonId = "";
            TitleId = "";
        }
        public Single? AvgRating { get; set; }
        public String Synopsis { get; set; }
        [MaxLength(150)]
        public String IdUrl { get; set; }

        [NotMapped]
        public String SeasonId { get; set; }
        [NotMapped]
        public String TitleId { get; set; }

        public new String Id
        {
            get
            {
                return TitleId + (SeasonId != "" ? ";" + SeasonId : "");
            }
            set
            {
                if (value != null && value != "")
                {
                    String[] val = value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (val.Length > 1)
                    {
                        TitleId = val[0];
                        SeasonId = val[1];
                    }
                    else
                        TitleId = val[0];
                }
            }
        }

        //public String PosterUrlMedium { get; set; }
        [MaxLength(250)]
        public String PosterUrlLarge { get; set; }
        /// <summary>
        /// Netflix Ids of similar titles
        /// </summary>
        public List<String> SimilarTitles
        {
            get { return _SimilarTitles; }
            set { _SimilarTitles = value; }
        }
        private List<String> _SimilarTitles = new List<String>();
        public String SimilarTitlesCompact
        {
            get { return _SimilarTitles != null && _SimilarTitles.Count > 0 ? String.Join(";#!", _SimilarTitles) : null; }
            set { _SimilarTitles = value != null ? value.Split(new[] { ";#!" }, StringSplitOptions.RemoveEmptyEntries).ToList() : null; }
        }
        public List<String> Awards
        {
            get { return _Awards; }
            set { _Awards = value; }
        }
        private List<String> _Awards = new List<String>();
        public String AwardsCompact
        {
            get { return _Awards != null && _Awards.Count > 0 ? String.Join(";#!", _Awards) : null; }
            set { _Awards = value != null ? value.Split(new[] { ";#!" }, StringSplitOptions.RemoveEmptyEntries).ToList() : null; }
        }

        public Format Format { get; set; }

        [MaxLength(250)]
        public String OfficialWebsiteUrl { get; set; }

    }
    [ComplexType]
    public class ImdbInfo : ProviderInfo
    {
        public ImdbInfo()
        {
            Id = "";
            Url = "";
            AvgRating = null;
            NeedsUpdate = true;
        }
        public Int32? AvgRating { get; set; }
        public String Synopsis { get; set; }
        [MaxLength(100)]
        public String Studio { get; set; }
    }
    [ComplexType]
    public class RedBoxInfo : ProviderInfo
    {
        public RedBoxInfo()
        {
            Id = "";
            Url = "";
            NeedsUpdate = true;
        }
        [MaxLength(36)]
        public new String Id { get; set; }
    }
    [ComplexType]
    public class AmazonInfo : ProviderInfo
    {
        public AmazonInfo()
        {
            Id = "";
            Url = "";
            AvgRating = null;
            NeedsUpdate = true;
        }

        public Double? LastPrice { get; set; }
        public DateTime? LastPriceUpdate { get; set; }
        [MaxLength(250)]
        public String PosterUrlMedium { get; set; }
        [MaxLength(250)]
        public String PosterUrlLarge { get; set; }
        public Int32? AvgRating { get; set; }
        public String Synopsis { get; set; }
        [MaxLength(100)]
        public String Studio { get; set; }
    }
    [ComplexType]
    public class RottenTomatoesInfo : ProviderInfo
    {
        public RottenTomatoesInfo()
        {
            Id = "";
            Url = "";
            AvgRating = null;
            NeedsUpdate = true;
        }

        public Int32? CriticScore { get; set; }
        public String CriticConsensus { get; set; }
        [MaxLength(250)]
        public String PosterUrlMedium { get; set; }
        [MaxLength(250)]
        public String PosterUrlLarge { get; set; }
        public DateTime? TheatricalRelase { get; set; }
        public DateTime? DvdRelease { get; set; }
        public Int32? AvgRating { get; set; }
        public String Synopsis { get; set; }
        [MaxLength(100)]
        public String Studio { get; set; }
        [MaxLength(250)]
        public String TrailerUrl { get; set; }

        public Int32? Year { get; set; }
    }

    public abstract class ProviderInfo
    {
        [MaxLength(20)]
        public String Id { get; set; }
        [MaxLength(250)]
        public String Url { get; set; }
        public Boolean NeedsUpdate { get; set; }
        public DateTime? LastFullUpdate { get; set; }
    }

    [ComplexType]
    public class NetflixPersonInfo : ProviderInfo
    {
        public NetflixPersonInfo()
        {
            Id = "";
            Url = "";
            NeedsUpdate = true;
            IdUrl = "";
        }
        public String IdUrl { get; set; }
    }
    [ComplexType]
    public class ImdbPersonInfo : ProviderInfo
    {
        public ImdbPersonInfo()
        {
            Id = "";
            Url = "";
            NeedsUpdate = true;
        }
    }
    [ComplexType]
    public class RottenTomatoesPersonInfo : ProviderInfo
    {
        public RottenTomatoesPersonInfo()
        {
            Id = "";
            Url = "";
            NeedsUpdate = true;
        }
    }


    public enum RoleType
    {
        Actor,
        Director,
        Writer,
        Producer,
        None = 99
    }

    public class Dateable
    {
        public DateTime? DateCreated { get; set; }
        public DateTime? DateLastModified { get; set; }
    }


}