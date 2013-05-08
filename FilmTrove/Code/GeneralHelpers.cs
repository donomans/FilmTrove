using FilmTrove.Code.Netflix;
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
            using (profiler.Step("GetDatabaseMoviesNetflix inside"))
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
                                NetflixHelpers.AddNetflixGenres(movie, ftc, netflixmovie);

                                Document d = new Document();
                                d.Add(new Field("NetflixId", netflixmovie.FullId,
                                    Field.Store.YES, Field.Index.NO));
                                d.Add(new Field("Title", netflixmovie.FullTitle,
                                    Field.Store.YES, Field.Index.ANALYZED));
                                d.Add(new Field("AltTitle", netflixmovie.ShortTitle,
                                    Field.Store.YES, Field.Index.ANALYZED));
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
            using (profiler.Step("Awaiting results"))
            {
                var realresults = await results;
                return await GetDatabaseMoviesRottenTomatoes(realresults, ftc, profiler);
            }
        }
        public static async Task<List<Models.Movie>> GetDatabaseMoviesRottenTomatoes(Titles results, 
            FilmTroveContext ftc, MiniProfiler profiler = null)
        {
            //var profiler = MiniProfiler.Current;
            using (profiler.Step("GetDatabaseMoviesRottenTomatoes inside"))
            {
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

                                        RottenTomatoesHelpers.AddRottenTomatoesGenres(movie, ftc, unmatched);

                                        Document d = new Document();
                                        d.Add(new Field("RottenTomatoesId", unmatched.FullId,
                                            Field.Store.YES, Field.Index.NO));
                                        d.Add(new Field("Title", unmatched.FullTitle,
                                            Field.Store.YES, Field.Index.ANALYZED));
                                        //d.Add(new Field("AltTitle", unmatched.FullTitle,
                                        //    Field.Store.YES, Field.Index.ANALYZED));
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
        public static async Task<Models.Movie> GetExistingMovie(ITitle title, 
            FilmTroveContext ftc)
        {
            var profiler = MiniProfiler.Current;
            using (profiler.Step("GetExistingMovie"))
            {
                return TitleSearch(title.FullTitle, ftc, title.Year);
            }
        }

        public static Movie TitleSearch(String title,
            FilmTroveContext ftc, Int32 year = -1)
        {
            //var Cache = new System.Web.Caching.Cache();
            //var ramindex = (RAMDirectory)Cache.Get("ftramindex");
            //RAMDirectory ramindex = (RAMDirectory)HttpContext.Current.Items["ftramindex"];
            using (var fsindex = FSDirectory.Open(HostingEnvironment.MapPath("/App_Data/index")))
            {
                using (IndexReader reader = IndexReader.Open(fsindex, true))
                {
                    using (Searcher searcher = new IndexSearcher(reader))
                    {

                        MultiFieldQueryParser parser =
                            new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                            new[] { "Title", "AltTitle" },
                            new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));

                        Query query = parser.Parse(QueryParser.Escape(title));

                        TopDocs td = searcher.Search(query, 10);
                        var docs = td.ScoreDocs
                            .Where(d => d.Score > 7.5f)
                            .ToList();
                        if (docs.Count > 0)
                        {
                            ///find the closest match of the "documents" with
                            ///a score above some threshold (like 80?) and get the movies
                            ///from the database and then check to see if the year is a match.
                            ///the result set should only be a handful of titles at most.
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
                                        return m;
                                    else
                                        continue;
                                }
                                else
                                    return m;
                            }
                            return null;
                        }
                        else
                            return null;
                    }
                }
            }
        }

        public static ITitle FindTitleMatch(Movie m, Titles searchtitles)
        {
            var profiler = MiniProfiler.Current;
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
                            d.Add(new Field("Year", title.Year.ToString(),
                                Field.Store.YES, Field.Index.NO));
                            iw.AddDocument(d);
                        }
                        iw.Optimize();
                        Int32 totaldocs = iw.NumDocs();
                    }
                    using (IndexReader reader = IndexReader.Open(ramindex, true))
                    {
                        Int32 totaldocs = reader.NumDocs();
                        using (Searcher searcher = new IndexSearcher(reader))
                        {
                            QueryParser parser =
                                new QueryParser(Lucene.Net.Util.Version.LUCENE_30,
                                "Title",
                                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));

                            Query query = parser.Parse(QueryParser.Escape(m.Title));

                            TopDocs td = searcher.Search(query, 10);
                            var docs = td.ScoreDocs
                                //.Where(d => d.Score > 7.5f)
                                .ToList();

                            if (docs.Count > 0)
                            {
                                return (Title)docs.Select(d =>
                                {
                                    Document doc = searcher.Doc(d.Doc);
                                    String id = doc.Get("Id");
                                    return searchtitles.Single(t => t.FullId == id);
                                }).FirstOrDefault(t =>
                                    m.Year == t.Year ||
                                    m.Year + 1 == t.Year ||
                                    m.Year - 1 == t.Year);
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