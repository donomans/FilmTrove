﻿using FilmTrove.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlixSharp;
using FlixSharp.Holders;
using FilmTrove.Filters;
using System.Threading.Tasks;
using FilmTrove.Models;

namespace FilmTrove.Controllers
{
    [InitializeSimpleMembership]
    public class MoviesController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Details(String id, String title)
        {
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                FilmTrove.Models.Movie m = ftc.Movies.Find(Int32.Parse(id));
                
                Task<FlixSharp.Holders.Title> nfm = null;

                if (m.Netflix.NeedsUpdate)
                {
                    nfm = Netflix.Fill.Titles.GetCompleteTitle(m.Netflix.Url, true);
                }
                if (m.Amazon.NeedsUpdate || (m.Amazon.LastPriceUpdate.HasValue && ((m.Amazon.LastPriceUpdate.Value - DateTime.Now) > new TimeSpan(1, 0, 0, 0))))
                {
                    if(m.Amazon.Id == null || m.Amazon.Id == "" || 
                        m.Amazon.LastFullUpdate.HasValue && ((m.Amazon.LastFullUpdate.Value - DateTime.Now) > new TimeSpan(7, 0, 0, 0)))
                    {
                        ///1) do query to match up title and fill in id with priority to blu-ray
                    }
                        ///1) else use Id
                        
                    ///2) fill in found id
                    ///3) purchase url (prioritize blu-ray)
                    ///4) price
                    ///5) update lastpriceupdate
                }
                if (m.Imdb.NeedsUpdate)
                {
                    ///nothing
                }
                if (m.RottenTomatoes.NeedsUpdate ||
                    (m.RottenTomatoes.LastFullUpdate.HasValue && ((m.Amazon.LastFullUpdate.Value - DateTime.Now) > new TimeSpan(7, 0, 0, 0))))
                {
                    if (m.RottenTomatoes.Id == null || m.RottenTomatoes.Id == "" ||
                        m.RottenTomatoes.LastFullUpdate.HasValue && ((m.RottenTomatoes.LastFullUpdate.Value - DateTime.Now) > new TimeSpan(7, 0, 0, 0)))
                    {
                        ///1) title match like with amazon or use Id if present
                    }
                    ///1.5) loop through the cast and match them with the current cast to get the role name filled.
                    ///2) Critic score
                    ///3) critic consensus
                    ///4) poster medium
                    ///5) poster large
                    ///6) theatrical release
                    ///7) dvd release
                    ///8) average rating
                    ///9) studio
                    ///10) synopsis
                }

                if (nfm != null)
                {
                    var netflixtitle = await nfm;
                    
                    ///populate all the netflix information
                    m.Netflix.NeedsUpdate = false;
                    m.Netflix.AvgRating = netflixtitle.AverageRating;
                    m.Netflix.Awards = netflixtitle.Awards.Select(a =>
                        a.AwardName + ";#" + a.PersonIdUrl + ";#" + 
                        a.Type + ";#" + a.Winner + ";#" + a.Year).ToList();
                    m.AltTitle = netflixtitle.ShortTitle;
                    m.Rating = netflixtitle.Rating.RatingType == RatingType.Mpaa ?
                            netflixtitle.Rating.MpaaRating.ToString() : netflixtitle.Rating.TvRating.ToString();
                    m.RatingType = netflixtitle.Rating.RatingType;

                    foreach (FlixSharp.Holders.FormatAvailability f in netflixtitle.Formats)
                    {
                        switch (f.Format)
                        {
                            case "Blu-ray":
                                m.Netflix.Format |= Models.Format.Bluray;
                                break;
                            case "DVD":
                                m.Netflix.Format |= Models.Format.DVD;
                                break;
                            case "instant":
                                m.Netflix.Format |= Models.Format.Digital;
                                break;
                        }
                    }
                    //m.Netflix.RelatedTitles = netflixmovie.RelatedTitles.Select(t => t.Id).ToList();
                    m.Netflix.Synopsis = netflixtitle.Synopsis;
                    m.Netflix.Url = netflixtitle.IdUrl;
                    m.Netflix.OfficialWebsiteUrl = netflixtitle.NetflixSiteUrl;
                    var nfactorids = netflixtitle.Actors.Select(t => t.Id);
                    var matchedactors = ftc.People.Where(t => nfactorids.Contains(t.Netflix.Id));
                    var matchedactorids = matchedactors.Select(p => p.Netflix.Id);
                    var actorsfordatabase = netflixtitle.Actors.Where(t => !matchedactorids.Contains(t.Id));
                    foreach (FlixSharp.Holders.Person p in actorsfordatabase)
                    {
                        ///1) find the cast and loop through and add the people to the database in a similar manner as AsyncHelpers.GetDatabasePeople
                        FilmTrove.Models.Person ftperson = ftc.People.Create();
                        ftperson.Netflix.Id = p.Id;
                        ftperson.Netflix.NeedsUpdate = true;
                        ftperson.Netflix.Url = p.IdUrl;
                        ftperson.Bio = p.Bio;
                        ftperson.Name = p.Name;
                        ftc.People.Add(ftperson);
                        ///2) add this movie as a new role on the movie
                        Role r = ftc.Roles.Create();
                        r.InRole = RoleType.Actor;
                        r.Movie = m;
                        r.Person = ftperson;
                        ftc.Roles.Add(r);
                        
                        ///are these necessary?
                        //ftperson.Roles.Add(r);
                        //m.Roles.Add(r);
                    }
                    if (m.Roles.Count < 1)
                    {
                        ///need to find which roles haven't already been added.
                        foreach (Models.Person p in matchedactors)
                        {
                            ///2) add this movie as a new role on the movie
                            Role r = ftc.Roles.Create();
                            r.InRole = RoleType.Actor;
                            r.Movie = m;
                            r.Person = p;
                            ftc.Roles.Add(r);
                        }
                    }
                        
                        
                    var nfdirectorids = netflixtitle.Directors.Select(t => t.Id);
                    var matcheddirectors = ftc.People.Where(t => nfdirectorids.Contains(t.Netflix.Id));
                    var matcheddirectorids = matcheddirectors.Select(p => p.Netflix.Id);
                    var directorsfordatabase = netflixtitle.Directors.Where(t => !matcheddirectorids.Contains(t.Id));
                    foreach (FlixSharp.Holders.Person p in directorsfordatabase)
                    {
                        ///1) find the cast and loop through and add the people to the database in a similar manner as AsyncHelpers.GetDatabasePeople
                        FilmTrove.Models.Person ftperson = ftc.People.Create();
                        ftperson.Netflix.Id = p.Id;
                        ftperson.Netflix.NeedsUpdate = true;
                        ftperson.Netflix.Url = p.IdUrl;
                        ftperson.Bio = p.Bio;
                        ftperson.Name = p.Name;
                        ftc.People.Add(ftperson);

                        ///2) add this movie as a new role on the movie
                        Role r = ftc.Roles.Create();
                        r.InRole = RoleType.Director;
                        r.Movie = m;
                        r.Person = ftperson;
                        ftc.Roles.Add(r);
                        
                        ///are these necessary?
                        //ftperson.Roles.Add(r);
                        //m.Roles.Add(r);
                    }
                    if (m.Roles.Count < 1)
                    {
                        foreach (Models.Person p in matcheddirectors)
                        {
                            ///2) add this movie as a new role on the movie
                            Role r = ftc.Roles.Create();
                            r.InRole = RoleType.Actor;
                            r.Movie = m;
                            r.Person = p;
                            ftc.Roles.Add(r);
                        }
                    }

                    ///3) add all similar titles to the database similar to step 1 for people
                    var nftitleids = netflixtitle.SimilarTitles.Select(t => t.Id);
                    var matchedtitleids = ftc.Movies.Where(t => nftitleids.Contains(t.Netflix.Id)).Select(t => t.Netflix.Id);
                    var titleidsfordatabase = nftitleids.Where(t => !matchedtitleids.Contains(t));
                    var titlesfordatabase = netflixtitle.SimilarTitles.Where(t => titleidsfordatabase.Contains(t.Id));
                    foreach (FlixSharp.Holders.Title t in titlesfordatabase)
                    {
                        FilmTrove.Models.Movie ftmovie = ftc.Movies.Create();
                        ftmovie.Netflix.Id = t.Id;
                        ftmovie.Netflix.Url = t.IdUrl;
                        ftmovie.Netflix.AvgRating = t.AverageRating;
                        ftmovie.Netflix.OfficialWebsiteUrl = t.OfficialWebsite;
                        ftmovie.Netflix.PosterUrlLarge = t.BoxArtUrlLarge;
                        ftmovie.Netflix.NeedsUpdate = true;
                        ftmovie.Rating = t.Rating.RatingType == RatingType.Mpaa ? 
                            t.Rating.MpaaRating.ToString() : t.Rating.TvRating.ToString();
                        ftmovie.RatingType = t.Rating.RatingType;
                        ftmovie.AltTitle = t.ShortTitle;
                        ftmovie.Title = t.FullTitle;
                        ftmovie.BestPosterUrl = t.BoxArtUrlLarge;
                        ftmovie.Year = t.Year;
                        ftmovie.Genres = t.Genres;
                    }

                    m.Netflix.SimilarTitles = netflixtitle.SimilarTitles.Select(t => t.IdUrl).ToList();
                    m.Netflix.NeedsUpdate = false;
                    m.Genres = netflixtitle.Genres;
                    m.Netflix.Synopsis = netflixtitle.Synopsis;
                    m.Description = netflixtitle.Synopsis;
                    m.ViewCount++;
                    ftc.SaveChanges();
                }
                ViewBag.Movie = m;
            }
            ViewBag.Id = id;
            return View();
        }

    }
}
