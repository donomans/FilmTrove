﻿using FilmTrove.Code.Netflix;
using FilmTrove.Code.RottenTomatoes;
using FilmTrove.Models;
using FlixSharp.Holders;
using FlixSharp.Holders.Netflix;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
namespace FilmTrove.Code
{
    public class GeneralHelpers
    {
        #region Netflix
        public static async Task<List<Models.Movie>> GetDatabaseMoviesNetflix(Titles results, FilmTroveContext ftc)
        {
            var netflixids = results.Select((m) => (m as FlixSharp.Holders.Netflix.Title).FullId);

            //using (FilmTroveContext ftc = (FilmTroveContext)HttpContext.Current.Items["ftcontext"])
            //{

            ///1) find the matching records from the database
            var matchedmovies = ftc.Movies.Where(m => netflixids.Contains(m.Netflix.Id));
            ///2) find the records that don't have a match
            ///select the ids and get the netflix Ids that aren't in the FT database
            var ftnfids = matchedmovies.Select(m => m.Netflix.Id).ToList();
            var netflixidsunmatched = netflixids.Where(m => !ftnfids.Contains(m)).ToList();
            //Int32 count = 0;
            var unmatchedresults = results
                .Select(m => (m as FlixSharp.Holders.Netflix.Title))
                .Where(m => netflixidsunmatched.Contains(m.FullId))
                .ToList();
            if (unmatchedresults.Count > 0)
            {
                RAMDirectory ramindex = (RAMDirectory)HttpContext.Current.Items["ftramindex"];
                if (ramindex == null)
                    throw new MissingMemberException("ramindex was null");

                IndexWriter iw = new IndexWriter(ramindex,
                    new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                    IndexWriter.MaxFieldLength.LIMITED);
                
                foreach (var netflixmovie in unmatchedresults)
                {
                    //String nid = unmatched.FullId;
                    ///check for FT record incase it was added using a different data source (like RT)
                    Models.Movie movie = await GetExistingMovie(netflixmovie, ftc);
                    ///create FT database records for each of these with the movies basic information for now
                    Boolean wasempty = false;
                    if (movie == null)
                    {
                        movie = ftc.Movies.Create();
                        wasempty = true;
                    }
                    //FlixSharp.Holders.Netflix.Title netflixmovie = results.Find(movie.Netflix.Id) as FlixSharp.Holders.Netflix.Title;
                    NetflixHelpers.FillBasicNetflixTitle(movie, netflixmovie);
                    NetflixHelpers.FillNetflixGenres(movie, ftc, netflixmovie);
                    //var dbgenreslocal = ftc.Genres.Local.Where(g => netflixmovie.Genres.Contains(g.Name));
                    //var dbgenres = ftc.Genres.Where(g => netflixmovie.Genres.Contains(g.Name));
                    //HashSet<Genre> genres = new HashSet<Genre>();
                    //genres.AddRange(dbgenres);
                    //genres.AddRange(dbgenreslocal);

                    //var genrenames = genres.Select(g => g.Name);
                    //var missinggenres = netflixmovie.Genres.Where(g => !genrenames.Contains(g));
                    //foreach (String genre in missinggenres)
                    //{
                    //    Genre g = new Genre() { Name = genre };
                    //    genres.Add(g);
                    //    ftc.Genres.Add(g);
                    //}
                    ////newmovie.Genres = netflixmovie.Genres;
                    //foreach (Genre g in genres)
                    //{
                    //    MovieGenre gi = ftc.GenreItems.Create();
                    //    gi.Genre = g;
                    //    gi.Movie = movie;
                    //    ftc.GenreItems.Add(gi);
                    //}
                    Document d = new Document();
                    d.Add(new Field("NetflixId", netflixmovie.FullId,
                        Field.Store.YES, Field.Index.NO));
                    d.Add(new Field("Title", netflixmovie.FullTitle,
                        Field.Store.YES, Field.Index.ANALYZED));
                    d.Add(new Field("AltTitle", netflixmovie.ShortTitle,
                        Field.Store.YES, Field.Index.ANALYZED));
                    iw.AddDocument(d);
                    
                    if(wasempty)
                        ftc.Movies.Add(movie);
                }
                iw.Optimize();
                iw.Close();
            }
            ftc.SaveChanges();

            if (matchedmovies.Count() < results.Count())
                matchedmovies = ftc.Movies.Where(m => netflixids.Contains(m.Netflix.Id));

            return results.Select(m =>
                matchedmovies.First(f =>
                    f.Netflix.Id == m.FullId)).ToList();
            //}
        }
        public static List<Models.Person> GetDatabasePeopleNetflix(People results, FilmTroveContext ftc)
        {
            var netflixids = results.Select(p => p.Id);
            //using (FilmTroveContext ftc = (FilmTroveContext)HttpContext.Current.Items["ftcontext"])
            //{
                ///1) find the matching records from the database
                var matchedpeople = ftc.People.Where(m => netflixids.Contains(m.Netflix.Id));
                ///2) find the records that don't have a match
                ///select the ids and get the netflix Ids that aren't in the FT database
                var ftnfids = matchedpeople.Select(m => m.Netflix.Id).ToList();
                var netflixidsunmatched = netflixids.Where(m => !ftnfids.Contains(m)).ToList();
                //Int32 count = 0;
                foreach (String nid in netflixidsunmatched)
                {
                    ///create FT database records for each of these with the movies basic information for now
                    FilmTrove.Models.Person newperson = ftc.People.Create();
                    FlixSharp.Holders.Netflix.Person netflixperson = results.Find(nid) as FlixSharp.Holders.Netflix.Person;
                    NetflixHelpers.FillBasicNetflixPerson(newperson, netflixperson);

                    ftc.People.Add(newperson);
                }

                try
                {
                    //count = 
                    ftc.SaveChanges();
                }
                catch (Exception)
                {
                    throw;
                    ///need to add some sort of logging?
                }
                //if (count > 0)
                if (matchedpeople.Count() < results.Count())
                    matchedpeople = ftc.People.Where(m => netflixids.Contains(m.Netflix.Id));
                //else
                //    return matchedpeople.ToList();

                return results.Select(p =>
                    matchedpeople.First(f =>
                        f.Netflix.Id == p.Id)).ToList();
            //}
        }
        #endregion
        #region Rotten Tomatoes
        public static async Task<List<Models.Movie>> GetDatabaseMoviesRottenTomatoes(Titles results, FilmTroveContext ftc)
        {
            var rottentomatoesids = results.Select(m => m.Id);

            //using (FilmTroveContext ftc = (FilmTroveContext)HttpContext.Current.Items["ftcontext"])
            //{
            ///1) find the matching records from the database
            var matchedmovies = ftc.Movies.Where(m => rottentomatoesids.Contains(m.RottenTomatoes.Id));
            ///2) find the records that don't have a match
            ///select the ids and get the rotten tomato Ids that aren't in the FT database
            var ftrtids = matchedmovies.Select(m => m.RottenTomatoes.Id).ToList();
            var rottentomatoesidsunmatched = rottentomatoesids.Where(m => !ftrtids.Contains(m)).ToList();
            //Int32 count = 0;
            var unmatchedresults = results
                .Select(m => (m as FlixSharp.Holders.RottenTomatoes.Title))
                .Where(m => rottentomatoesidsunmatched.Contains(m.Id))
                .ToList();

            if (unmatchedresults.Count > 0)
            {
                RAMDirectory ramindex = (RAMDirectory)HttpContext.Current.Items["ftramindex"];
                if (ramindex == null)
                    throw new MissingMemberException("ramindex was null");

                IndexWriter iw = new IndexWriter(ramindex,
                    new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                    IndexWriter.MaxFieldLength.LIMITED);
                
                foreach (var unmatched in unmatchedresults)
                {
                    //String nid = unmatched.FullId;
                    ///check for FT record incase it was added using a different data source (like RT)
                    Models.Movie movie = await GetExistingMovie(unmatched, ftc);
                    ///create FT database records for each of these with the movies basic information for now
                    Boolean wasempty = false;
                    if (movie == null)
                    {
                        movie = ftc.Movies.Create();
                        wasempty = true;
                    }

                    //FlixSharp.Holders.RottenTomatoes.Title rottentomatoemovie = results.Find(movie.RottenTomatoes.Id) as FlixSharp.Holders.RottenTomatoes.Title;
                    //if (rottentomatoemovie != null)
                    //{
                    RottenTomatoesHelpers.FillRottenTomatoesTitle(movie, unmatched);

                    if (unmatched.Genres.Count > 0)
                    {
                        var dbgenreslocal = ftc.Genres.Local.Where(g => unmatched.Genres.Contains(g.Name));
                        var dbgenres = ftc.Genres.Where(g => unmatched.Genres.Contains(g.Name));
                        HashSet<Genre> genres = new HashSet<Genre>();
                        genres.AddRange(dbgenres);
                        genres.AddRange(dbgenreslocal);

                        var genrenames = genres.Select(g => g.Name);
                        var missinggenres = unmatched.Genres.Where(g => !genrenames.Contains(g));
                        foreach (String genre in missinggenres)
                        {
                            Genre g = new Genre() { Name = genre };
                            genres.Add(g);
                            ftc.Genres.Add(g);
                        }
                        //newmovie.Genres = netflixmovie.Genres;
                        foreach (Genre g in genres)
                        {
                            MovieGenre gi = ftc.GenreItems.Create();
                            gi.Genre = g;
                            gi.Movie = movie;
                            ftc.GenreItems.Add(gi);
                        }
                    }

                    Document d = new Document();
                    d.Add(new Field("RottenTomatoesId", unmatched.FullId,
                        Field.Store.YES, Field.Index.NO));
                    d.Add(new Field("Title", unmatched.FullTitle,
                        Field.Store.YES, Field.Index.ANALYZED));
                    d.Add(new Field("AltTitle", unmatched.FullTitle,
                        Field.Store.YES, Field.Index.ANALYZED));
                    iw.AddDocument(d);
                    
                    if (wasempty)
                        ftc.Movies.Add(movie);
                }
                iw.Optimize();
                iw.Close();
            }

            ftc.SaveChanges();

            if (matchedmovies.Count() < results.Count())
                matchedmovies = ftc.Movies
                    .Where(m => rottentomatoesids.Contains(m.RottenTomatoes.Id));

            return results.Select(m =>
                matchedmovies.First(r =>
                    r.RottenTomatoes.Id == m.FullId)).ToList();
            //}
        }
        public static List<Models.Person> GetDatabasePeopleRottenTomatoes(Titles results, FilmTroveContext ftc)
        {
            return null;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="ftc"></param>
        /// <returns></returns>
        public static async Task<Models.Movie> GetExistingMovie(ITitle title, FilmTroveContext ftc)
        {
            return LuceneSearch(title.FullTitle, ftc);

            Int32 maxlength = (Int32)(title.FullTitle.Length * 1.2);
            Int32 minlength = (Int32)(title.FullTitle.Length * .8);
            var match =
                (from m
                in ftc.Movies
                let mAltTitle = m.AltTitle.ToLower()
                let mTitle = m.Title.ToLower()
                let tFullTitle = title.FullTitle.ToLower()
                where (m.Netflix.Id == title.FullId 
                || m.RottenTomatoes.Id == title.FullId
                || (mAltTitle == tFullTitle)
                || (mTitle == tFullTitle)
                || (mAltTitle.Contains(tFullTitle) && mAltTitle.Length > minlength && mAltTitle.Length < maxlength)
                || (mTitle.Contains(tFullTitle) && mTitle.Length > minlength && mTitle.Length < maxlength)
                || (tFullTitle.Contains(mAltTitle) && mAltTitle.Length > minlength && mAltTitle.Length < maxlength)
                || (tFullTitle.Contains(mTitle) && mTitle.Length > minlength && mTitle.Length < maxlength))
                && (m.Year == title.Year
                    || title.Year + 1 == m.Year
                    || title.Year - 1 == m.Year)
                select m).FirstOrDefault();

            if (match != null)
                return match;
            else
            {
                switch (title.Source)
                {
                    case TitleSource.Netflix:
                        return await RottenTomatoesHelpers.FindRottenTomatoesMatch(title, ftc);
                    case TitleSource.RottenTomatoes:
                        return await NetflixHelpers.FindNetflixMatch(title, ftc);
                    default:
                        throw new NotImplementedException("Title type of: " + title.Source.ToString() + " is not implemented");
                }
            }
        }

        private static Movie LuceneSearch(String queryString, FilmTroveContext ftc)
        {
            RAMDirectory ramindex = (RAMDirectory)HttpContext.Current.Items["ftramindex"];
            IndexReader reader = IndexReader.Open(ramindex, true);
            
            Searcher searcher = new IndexSearcher(reader);

            MultiFieldQueryParser parser = 
                new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                new[] { "Title", "AltTitle" },
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
            
            Query query = parser.Parse(queryString);
            
            TopDocs td = searcher.Search(query, 5);

            if (td.MaxScore > 80)
            {
                Document doc = searcher.Doc(td.ScoreDocs[0].Doc);
                //doc.Get("Title");
                //doc.Get("AltTitle");
                //doc.Get("NetflixId");
                //doc.Get("Id");
                //doc.Get("RottenTomatoesId");
                String id = doc.Get("Id");
                if (id != null && id != "")
                    return ftc.Movies.Find(Int32.Parse(id));
                
                String netflixid = doc.Get("NetflixId");
                if (netflixid != null && netflixid != "")
                    return ftc.Movies.FirstOrDefault(m => m.Netflix.Id == netflixid);
                String rottentomatoesid = doc.Get("RottenTomatoesId");
                if (rottentomatoesid != null && rottentomatoesid != "")
                    return ftc.Movies.FirstOrDefault(m => m.RottenTomatoes.Id == rottentomatoesid);

                return null;
            }
            else
                return null;
        }
    }
}