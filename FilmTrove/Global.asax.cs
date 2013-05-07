﻿using FilmTrove.Code;
using FilmTrove.Models;
using FlixSharp;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebMatrix.WebData;

namespace FilmTrove
{
    public class MvcApplication : System.Web.HttpApplication
    {
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            RottenTomatoes.Login.SetCredentials("ej3d8ejgp3dn9cu2eguvke42");

            Netflix.Login.SetCredentials(
                "7qf3845qydavuucmhj96b6hd",
                "5jYe5FVhhF",
                "FilmTrove");
            
            Netflix.SetMethodForGettingCurrentUserAccount(FilmTrove.Models.NetflixAccount.GetCurrentUserNetflixUserInfo);

            
            ///Set up Lucene's index
            #region Lucene
            using (FilmTroveContext ftc = new FilmTroveContext())
            {
                var titles = ftc.Movies
                    .Select(m => new { MovieId = m.MovieId, Title = m.Title, AltTitle = m.AltTitle })
                    .ToList();
                using (var index = FSDirectory.Open(Server.MapPath("/App_Data/index")))
                {
                    using (IndexWriter iw = new IndexWriter(index,
                        new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                        IndexWriter.MaxFieldLength.LIMITED))
                    {
                        iw.DeleteAll();
                        foreach (var title in titles)
                        {
                            Document d = new Document();
                            d.Add(new Field("Id", title.MovieId.ToString(),
                                Field.Store.YES, Field.Index.ANALYZED));
                            d.Add(new Field("Title", title.Title,
                                Field.Store.YES, Field.Index.ANALYZED));
                            d.Add(new Field("AltTitle", title.AltTitle,
                                Field.Store.YES, Field.Index.ANALYZED));
                            iw.AddDocument(d);
                        }
                        iw.Optimize();
                    }
                }
                //Movie movie = GeneralHelpers.LuceneSearch("hot shots", ftc, index);
            }
            #endregion
        }

        //public class MovieTitleHolder
        //{
        //    public Int32 MovieId = -1;
        //    public String Title = "";
        //    public String AltTitle = "";
        //}

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.IsLocal)
            {
                MiniProfiler.Start();
                MiniProfilerEF.InitializeEF42();
            }

            HttpContext.Current.Items["ftcontext"] = new FilmTroveContext();
        }
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Current.Items["ftcontext"];
            ftc.Dispose();

            //AzureDirectory ad = (AzureDirectory)HttpContext.Current.Items["ftlucene"];
            //ad.Dispose();

            MiniProfiler.Stop();
        }
    }
}