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
            AvgRating = null;
            NeedsUpdate = true;
        }
        public Int32? AvgRating { get; set; }
        public String Synopsis { get; set; }
        public String Studio { get; set; }

        //public String PosterUrlMedium { get; set; }
        public String PosterUrlLarge { get; set; }
        /// <summary>
        /// Netflix Ids of similar titles
        /// </summary>
        public List<String> SimilarTitles
        {
            get { return _SimilarTitles; }
            set { _SimilarTitles = value; }
        }
        private List<String> _SimilarTitles { get; set; }
        public virtual String SimilarTitlesCompact
        {
            get { return _SimilarTitles != null ? String.Join(";#!", _SimilarTitles) : null; }
            set { _SimilarTitles = value != null ? value.Split(new[] { ";#!" }, StringSplitOptions.RemoveEmptyEntries).ToList() : null; }
        }

        public List<String> RelatedTitles
        {
            get { return _RelatedTitles; }
            set { _RelatedTitles = value; }
        }
        private List<String> _RelatedTitles { get; set; }
        public virtual String RelatedTitlesCompact
        {
            get { return _RelatedTitles != null ? String.Join(";#!", _RelatedTitles) : null; }
            set { _RelatedTitles = value != null ? value.Split(new[] { ";#!" }, StringSplitOptions.RemoveEmptyEntries).ToList() : null; }
        }

        public List<String> Awards
        {
            get { return _Awards; }
            set { _Awards = value; }
        }
        private List<String> _Awards { get; set; }
        public virtual String AwardsCompact
        {
            get { return _Awards != null ? String.Join(";#!", _Awards) : null; }
            set { _Awards = value != null ? value.Split(new[] { ";#!" }, StringSplitOptions.RemoveEmptyEntries).ToList() : null; }
        }

        public ScreenFormat ScreenFormat { get; set; }
        public Format Format { get; set; }
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
        public String Studio { get; set; }
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

        public String PosterUrlMedium { get; set; }
        public String PosterUrlLarge { get; set; }
        public Int32? AvgRating { get; set; }
        public String Synopsis { get; set; }
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
        public String PosterUrlMedium { get; set; }
        public String PosterUrlLarge { get; set; }
        public DateTime? TheatricalRelase { get; set; }
        public DateTime? DvdRelease { get; set; }
        public Int32? AvgRating { get; set; }
        public String Synopsis { get; set; }
        public String Studio { get; set; }
    }

    public abstract class ProviderInfo
    {
        [MaxLength(20)]
        public String Id { get; set; }
        public String Url { get; set; }
        public Boolean NeedsUpdate { get; set; }


    }

    [ComplexType]
    public class NetflixPersonInfo : ProviderInfo
    {
    }
    [ComplexType]
    public class ImdbPersonInfo : ProviderInfo
    {
    }
    //public class AmazonPersonInfo : ProviderInfo
    //{
    //}
    [ComplexType]
    public class RottenTomatoesPersonInfo : ProviderInfo
    {
    }


    public enum RoleType
    {
        Actor,
        Director,
        Writer,
        Producer
    }

    public class Dateable
    {
        public DateTime? DateCreated { get; set; }
        public DateTime? DateLastModified { get; set; }
    }


}