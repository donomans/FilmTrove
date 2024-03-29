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
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
namespace FilmTrove.Code
{
    public class GeneralHelpers
    {
        #region Netflix
        public static async Task<List<Models.Movie>> GetDatabaseMoviesNetflix(Titles results,
            FilmTroveContext ftc, MiniProfiler profiler = null)
        {
            using (profiler.Step("N DB query and fitlering for unmatched"))
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
                    //var Cache = new System.Web.Caching.Cache();
                    //var ramindex = (RAMDirectory)Cache.Get("ftramindex");
                    //RAMDirectory ramindex = (RAMDirectory)HttpContext.Current.Items["ftramindex"];
                    //if (ramindex == null)
                    //    throw new MissingMemberException("ramindex was null");
                    using (var index = FSDirectory.Open(HostingEnvironment.MapPath("/App_Data/index")))
                    {
                        using (IndexWriter iw = new IndexWriter(index,
                            new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                            IndexWriter.MaxFieldLength.LIMITED))
                        {
                            foreach (var netflixmovie in unmatchedresults)
                            {
                                //String nid = unmatched.FullId;
                                ///check for FT record incase it was added using a different data source (like RT)
                                Models.Movie movie = GetExistingMovie(netflixmovie, ftc, index, profiler);
                                ///create FT database records for each of these with the movies basic information for now
                                Boolean wasempty = false;
                                if (movie == null)
                                {
                                    movie = ftc.Movies.Create();
                                    wasempty = true;
                                }
                                //FlixSharp.Holders.Netflix.Title netflixmovie = results.Find(movie.Netflix.Id) as FlixSharp.Holders.Netflix.Title;
                                NetflixHelpers.FillBasicNetflixTitle(movie, netflixmovie);
                                NetflixHelpers.AddNetflixGenres(movie, ftc, netflixmovie);

                                Document d = new Document();
                                d.Add(new Field("NetflixId", netflixmovie.FullId,
                                    Field.Store.YES, Field.Index.NO));
                                d.Add(new Field("Title", netflixmovie.FullTitle,
                                    Field.Store.YES, Field.Index.ANALYZED));
                                //if(netflixmovie.Year > 0)
                                //    d.Add(new Field("Year", netflixmovie.Year.ToString(),
                                //        Field.Store.YES, Field.Index.ANALYZED));
                                iw.AddDocument(d);

                                if (wasempty)
                                    ftc.Movies.Add(movie);
                            }
                            iw.Optimize();
                        }
                    }
                }
                ftc.SaveChanges();

                if (matchedmovies.Count() < results.Count())
                    matchedmovies = ftc.Movies.Where(m => netflixids.Contains(m.Netflix.Id));

                return results.Select(m =>
                    matchedmovies.First(f =>
                        f.Netflix.Id == m.FullId)).ToList();
            }
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
        public static async Task<List<Models.Movie>> GetDatabaseMoviesRottenTomatoes(Task<Titles> results,
            FilmTroveContext ftc, MiniProfiler profiler = null)
        {
            Titles r = null;
            using (profiler.Step("Awaiting results"))
            {
                r = await results;
            }
            return await GetDatabaseMoviesRottenTomatoes(r, ftc, profiler);

        }
        public static async Task<List<Models.Movie>> GetDatabaseMoviesRottenTomatoes(Titles results,
            FilmTroveContext ftc, MiniProfiler profiler = null)
        {
            //var profiler = MiniProfiler.Current;
            using (profiler.Step("Rt DB query and filtering for unmatched"))
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
                    //var Cache = new System.Web.Caching.Cache();
                    //var ramindex = (RAMDirectory)HttpContext.Current.Cache.Get("ftramindex"); 
                    //RAMDirectory ramindex = (RAMDirectory)HttpContext.Current.Items["ftramindex"];
                    //if (ramindex == null)
                    //    throw new MissingMemberException("ramindex was null");
                    using (profiler.Step("Lucene fs index search"))
                    {
                        using (var index = FSDirectory.Open(HostingEnvironment.MapPath("/App_Data/index")))
                        {
                            using (IndexWriter iw = new IndexWriter(index,
                                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                                IndexWriter.MaxFieldLength.LIMITED))
                            {
                                foreach (var unmatched in unmatchedresults)
                                {
                                    //String nid = unmatched.FullId;
                                    ///check for FT record incase it was added using a different data source (like RT)
                                    Models.Movie movie = GetExistingMovie(unmatched, ftc, index, profiler);
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

                                    RottenTomatoesHelpers.AddRottenTomatoesGenres(movie, ftc, unmatched);

                                    Document d = new Document();
                                    d.Add(new Field("RottenTomatoesId", unmatched.FullId,
                                        Field.Store.YES, Field.Index.NO));
                                    d.Add(new Field("Title", unmatched.FullTitle,
                                        Field.Store.YES, Field.Index.ANALYZED));
                                    //if(unmatched.Year > 0)
                                    //    d.Add(new Field("Year", unmatched.Year.ToString(),
                                    //        Field.Store.YES, Field.Index.ANALYZED));
                                    iw.AddDocument(d);

                                    if (wasempty)
                                        ftc.Movies.Add(movie);
                                }
                                iw.Optimize();
                            }
                        }
                    }
                }
                using (profiler.Step("save changes and return"))
                {
                    ftc.SaveChanges();

                    if (matchedmovies.Count() < results.Count())
                        matchedmovies = ftc.Movies
                            .Where(m => rottentomatoesids.Contains(m.RottenTomatoes.Id));

                    return results.Select(m =>
                        matchedmovies.First(r =>
                            r.RottenTomatoes.Id == m.FullId)).ToList();
                }
            }
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
        public static Models.Movie GetExistingMovie(ITitle title, 
            FilmTroveContext ftc, FSDirectory index, MiniProfiler profiler = null)
        {
            using (profiler.Step("GetExistingMovie"))
            {
                return TitleSearch(title, ftc, index, title.Year);
            }
        }

        public static Movie TitleSearch(ITitle ititle,
            FilmTroveContext ftc, FSDirectory index, Int32 year = -1)
        {
            using (IndexReader reader = IndexReader.Open(index, true))
            {
                using (Searcher searcher = new IndexSearcher(reader))
                {
                    QueryParser parser = new QueryParser(
                        Lucene.Net.Util.Version.LUCENE_30,
                        "Title",
                        new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
                    
                    //var querytext = title + (year > 0 ? " " + year.ToString() : "");
                    Query query = parser.Parse(QueryParser.Escape(ititle.FullTitle));

                    TopDocs td = searcher.Search(query, 5);
                    var docs = td.ScoreDocs
                        .Where(d => d.Score > .9f)
                        .ToList();
                    var mergedocs = td.ScoreDocs
                                 .Where(d => d.Score > .5f) //&& d.Score <= .9f)
                                 .ToList();

                    foreach (var mergedoc in mergedocs)
                    {
                        Document doc = searcher.Doc(mergedoc.Doc);
                        Movie m = null;

                        String id = doc.Get("Id");
                        if (id != null && id != "")
                            m = ftc.Movies.Find(Int32.Parse(id));
                        if (m == null)
                        {
                            String netflixid = doc.Get("NetflixId");
                            if (netflixid != null && netflixid != "")
                                m = ftc.Movies.FirstOrDefault(movie => movie.Netflix.Id == netflixid);
                            if (m == null)
                            {
                                String rottentomatoesid = doc.Get("RottenTomatoesId");
                                if (rottentomatoesid != null && rottentomatoesid != "")
                                    m = ftc.Movies.FirstOrDefault(movie => movie.RottenTomatoes.Id == rottentomatoesid);
                            }
                        }
                        if (m != null)
                        {
                            MergeCandidate mc = ftc.MergeCandidates.Create();

                            mc.PrimaryId = ititle.FullId;
                            mc.PrimaryType = (MovieType)Enum.Parse(typeof(MovieType), ititle.Source.ToString(), true);
                            mc.SecondaryId = m.MovieId.ToString();
                            mc.SecondaryType = MovieType.FilmTrove;

                            ftc.MergeCandidates.Add(mc);
                        }
                    }
                    ftc.SaveChanges();
                    if (docs.Count > 0)
                    {
                        ///find the closest match of the "documents" with
                        ///a score above some threshold (like .8?) and get the movies
                        ///from the database and then check to see if the year is a match.
                        ///the result set should only be a handful of titles at most.
                        #region dumb
                        //var resultdocs = docs.Select(scoredoc =>
                        //{
                        //    Document doc = searcher.Doc(scoredoc.Doc);
                        //    String id = doc.Get("Id");
                        //    if (id != null && id != "")
                        //        return new { score = scoredoc.Score, id = id, type = MovieType.FilmTrove };
                        //    else
                        //    {
                        //        id = doc.Get("NetflixId");
                        //        if (id != null && id != "")
                        //            return new { score = scoredoc.Score, id = id, type = MovieType.Netflix };
                        //        else
                        //        {
                        //            id = doc.Get("RottenTomatoesId");
                        //            if (id != null && id != "")
                        //                return new { score = scoredoc.Score, id = id, type = MovieType.RottenTomatoes };
                        //        }
                        //    }

                        //    throw new Exception("all of those IDs were null - how what??");
                        //})
                        //.ToList();

                        //Movie m = null;
                        //Boolean foundmovie = false;

                        //var result = resultdocs
                        //    .OrderByDescending(d => d.score)
                        //    .Where(d =>
                        //    {
                        //        if (m == null)
                        //        {
                        //            switch (d.type)
                        //            {
                        //                case MovieType.FilmTrove:
                        //                    m = ftc.Movies.Find(Int32.Parse(d.id));
                        //                    break;
                        //                case MovieType.Netflix:
                        //                    m = ftc.Movies.FirstOrDefault(movie => movie.Netflix.Id == d.id);
                        //                    break;
                        //                case MovieType.RottenTomatoes:
                        //                    m = ftc.Movies.FirstOrDefault(movie => movie.RottenTomatoes.Id == d.id);
                        //                    break;
                        //            }
                        //        }

                        //        if (year > 0)
                        //        {
                        //            if (m != null)
                        //            {
                        //                if (m.Year == year ||
                        //                    m.Year + 1 == year ||
                        //                    m.Year - 1 == year)
                        //                {
                        //                    foundmovie = true;
                        //                    return false;
                        //                }
                        //                else
                        //                {
                        //                    m = null;
                        //                    return true;
                        //                }
                        //            }
                        //            else
                        //                return true;
                        //        }
                        //        else
                        //        {
                        //            if (m != null && !foundmovie)
                        //            {
                        //                foundmovie = true;
                        //                return false;
                        //            }
                        //            else
                        //                return true;
                        //        }
                        //        //if (!(m != null &&
                        //        //(year > 0 ?
                        //        //(m.Year == year ||
                        //        //m.Year + 1 == year ||
                        //        //m.Year - 1 == year)
                        //        //: true)))
                        //        //{
                        //        //    m = null;
                        //        //    return true;
                        //        //}
                        //        //else
                        //        //    return false;
                        //    })
                        //    .ToList();
                        //return m;
                        #endregion

                        foreach (var scoredoc in docs)
                        {
                            Document doc = searcher.Doc(scoredoc.Doc);
                            Movie m = null;

                            String id = doc.Get("Id");
                            if (id != null && id != "")
                                m = ftc.Movies.Find(Int32.Parse(id));
                            if (m == null)
                            {
                                String netflixid = doc.Get("NetflixId");
                                if (netflixid != null && netflixid != "")
                                    m = ftc.Movies.FirstOrDefault(movie => movie.Netflix.Id == netflixid);
                                if (m == null)
                                {
                                    String rottentomatoesid = doc.Get("RottenTomatoesId");
                                    if (rottentomatoesid != null && rottentomatoesid != "")
                                        m = ftc.Movies.FirstOrDefault(movie => movie.RottenTomatoes.Id == rottentomatoesid);
                                }
                            }

                            if (year > 0)
                            {
                                ///if i have a year then i care about it and 
                                ///so the year should match as well
                                if (m != null && (m.Year == year ||
                                    m.Year + 1 == year || m.Year - 1 == year))
                                {
                                    var movieid = m.MovieId.ToString();
                                    MergeCandidate record = ftc.MergeCandidates
                                        .FirstOrDefault(mc =>
                                        mc.PrimaryId == ititle.FullId &&
                                        mc.SecondaryId == movieid);
                                    if(record != null)
                                        ftc.MergeCandidates.Remove(record);
                                    ftc.SaveChanges();
                                    return m;
                                }
                                else
                                    continue;
                            }
                            else
                            {
                                MergeCandidate record = ftc.MergeCandidates.FirstOrDefault(mc =>
                                    mc.PrimaryId == ititle.FullId &&
                                    mc.SecondaryId == m.MovieId.ToString());
                                if (record != null)
                                    ftc.MergeCandidates.Remove(record);
                                ftc.SaveChanges();
                                return m;
                            }
                        }
                        return null;
                    }
                    else
                        return null;
                }

            }
        }

        public static ITitle FindTitleMatch(Movie m, Titles searchtitles, FilmTroveContext ftc, MiniProfiler profiler = null)
        {
            using (profiler.Step("FindTitleMatch"))
            {
                using (RAMDirectory ramindex = new RAMDirectory())
                {
                    using (IndexWriter iw = new IndexWriter(ramindex,
                        new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                        new IndexWriter.MaxFieldLength(250)))
                    {
                        foreach (var title in searchtitles)
                        {
                            Document d = new Document();
                            d.Add(new Field("Id", title.FullId.ToString(),
                                Field.Store.YES, Field.Index.NO));
                            d.Add(new Field("Title", title.FullTitle,
                                Field.Store.YES, Field.Index.ANALYZED));
                            //d.Add(new Field("Year", title.Year.ToString(),
                            //    Field.Store.YES, Field.Index.ANALYZED));
                            iw.AddDocument(d);
                        }
                        iw.Optimize();
                        //Int32 totaldocs = iw.NumDocs();
                    }
                    using (IndexReader reader = IndexReader.Open(ramindex, true))
                    {
                        //Int32 totaldocs = reader.NumDocs();
                        using (Searcher searcher = new IndexSearcher(reader))
                        {
                            QueryParser parser = new QueryParser(
                                Lucene.Net.Util.Version.LUCENE_30,
                                "Title",
                                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));

                            Query query = parser.Parse(QueryParser.Escape(m.Title));

                            TopDocs td = searcher.Search(query, 5);
                            var docs = td.ScoreDocs
                                .Where(d => d.Score > .6f)
                                .ToList();

                            var mergedocs = td.ScoreDocs
                                .Where(d => d.Score > .375f && d.Score <= .6f)
                                .ToList();

                            foreach (var mergedoc in mergedocs)
                            {
                                MergeCandidate mc = ftc.MergeCandidates.Create();

                                Document doc = searcher.Doc(mergedoc.Doc);
                                String id = doc.Get("Id");
                                ITitle it = searchtitles.First(t => t.FullId == id);
                                mc.PrimaryId = m.MovieId.ToString();
                                mc.PrimaryType = MovieType.FilmTrove;
                                mc.SecondaryId = it.FullId;
                                mc.SecondaryType = (MovieType)Enum.Parse(typeof(MovieType), it.Source.ToString(), true);
                                
                                ftc.MergeCandidates.Add(mc);
                            }

                            if (docs.Count > 0)
                            {
                                var resultdocs = docs
                                    .Select(d =>
                                    {
                                        Document doc = searcher.Doc(d.Doc);
                                    
                                        String id = doc.Get("Id");
                                        return new
                                        {
                                            title = searchtitles
                                                .First(t => t.FullId == id),
                                            docid = d.Doc
                                        };
                                    });
                                var result = resultdocs.FirstOrDefault(t =>
                                    m.Year == t.title.Year ||
                                    m.Year + 1 == t.title.Year ||
                                    m.Year - 1 == t.title.Year
                                    //||
                                    //(t.score > .95f && 
                                    /////hopefully this takes care of things like 
                                    /////Finding Nemo 3D without messing with 
                                    /////titles that have the same name
                                    //(t.movie.FullTitle.Contains(m.Title) ||
                                    //m.Title.Contains(t.movie.FullTitle)))
                                );
                                foreach (var doc in resultdocs.Where(sd => sd.docid != result.docid))
                                {
                                    MergeCandidate mc = ftc.MergeCandidates.Create();
                                    mc.PrimaryId = m.MovieId.ToString();
                                    mc.PrimaryType = MovieType.FilmTrove;
                                    mc.SecondaryId = doc.title.FullId;
                                    mc.SecondaryType = (MovieType)Enum.Parse(typeof(MovieType), doc.title.Source.ToString(), true);
                                    ftc.MergeCandidates.Add(mc);
                                }

                                if (result != null)
                                    return result.title;
                                else
                                    return null;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }
        
    }
}