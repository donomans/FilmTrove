using FilmTrove.Code;
using FilmTrove.Models;
using FlixSharp;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
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

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.IsLocal)
            {
                MiniProfiler.Start();
                MiniProfilerEF.InitializeEF42();
            }
            HttpContext.Current.Items["ftcontext"] = new FilmTroveContext();

            StorageCredentialsAccountAndKey scaak = new StorageCredentialsAccountAndKey("fttable",
                "ZUyPecw760rXbPfpGuUbOgc6LL2EgxrkKLWIxGAC2XL53gQWsVGmRz1y1tT7JDYFWoO0R/uEled6MYsLAdZWVg==");
            CloudStorageAccount csa = new CloudStorageAccount(scaak, true);

            AzureDirectory ad = new AzureDirectory(csa, "TESTING");///change this to get the live or localhost version
            HttpContext.Current.Items["ftlucene"] = ad;
        }
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Current.Items["ftcontext"];
            ftc.Dispose();

            AzureDirectory ad = (AzureDirectory)HttpContext.Current.Items["ftlucene"];
            ad.Dispose();

            MiniProfiler.Stop();
        }
    }
}