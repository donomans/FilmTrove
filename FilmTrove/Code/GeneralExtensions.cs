using FlixSharp;
using FlixSharp.Queries;
using FlixSharp.Queries.Netflix;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FilmTrove.Code
{
    public static class GeneralExtensions
    {
        private static String[] accounts = new[]{
            "7qf3845qydavuucmhj96b6hd;5jYe5FVhhF;FilmTrove",// original one
            "hd5n8g4x8wz53ug34k65s8ag;QfhzFdfPt4;half+past+meow",    
            "vghqkpd86meujk5y3fajvafq;BNR5pvUnzs;robutpanda",  
            "h6chq7dpqhw3pwsa9bvp5r9f;PCquKKNQJA;Humdinger",    
            "9abckmw54a5hskru6knt3zkr;FgedX4SSHB;ft",    
            "nz2gyja59hq9ne22b7wfwpjb;wT2GmJ6xdZ;Signedmeup",    
            "y7xw6vrkxqxnmguyff6825k5;XX4FWeBVPg;RobotPanda",    
            "ym68z674zmkpszk7mf7z9fuz;eVZYNummyZ;ftrove",    
            "dfh77wfcv3afvugjrjvcgm8y;GjKHCbQrHR;ft",    
            "vntdbbpcja5huvqfd6ypcmvx;enn4mwrAPm;filmt"};
        private static Random random = new Random();

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            if (values == null)
                return;
            foreach (T v in values)
                if (!collection.Contains(v))
                    collection.Add(v);
        }

        public static String UrlFriendly(this String source)
        {
            if (source == null)
                return null;
            else
                return source.ToLower().Replace(" ", "-").Replace(":", "-")
                .Replace("'", "").Replace("?", "").Replace(",", "")
                .Replace("&", "and").Replace("/", "-").Replace(".", "");
        }
        public static String UrlFriendly(this String source, Int32 year)
        {
            if (year == 0)
                return source.UrlFriendly();
            else
                return (source + " (" + year + ")").UrlFriendly();
        }

        public static TSource WhereFirstOrCreate<TSource>(this IQueryable<TSource> source, Func<TSource,Boolean> predicate) where TSource : new()
        {
            TSource ts = source.Where(predicate).FirstOrDefault();
            if (ts == null)
                return new TSource();
            else
                return ts;
        }
        public static TSource WhereFirstOrCreate<TSource>(this ICollection<TSource> source, Func<TSource, Boolean> predicate) where TSource : new()
        {
            TSource ts = source.Where(predicate).FirstOrDefault();
            if (ts == null)
                return new TSource();
            else
                return ts;
        }

        public static NetflixFill Randomized(this NetflixFill source)
        {
            //String details = accounts[random.Next(0, accounts.Length - 1)];
            //String[] account = details.Split(new[] { ';' });
            //Netflix.Login.SetCredentials(account[0], account[1], account[2]);
            //Netflix.OnUserBehalf = false;
            return Netflix.Fill;
        }
        public static NetflixSearch Randomized(this NetflixSearch source)
        {
            //String details = accounts[random.Next(0, accounts.Length - 1)];
            //String[] account = details.Split(new[] { ';' });
            //Netflix.Login.SetCredentials(account[0], account[1], account[2]);
            //Netflix.OnUserBehalf = false;
            return Netflix.Search;
        }
    }
}