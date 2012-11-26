using FlixSharp;
using FlixSharp.Queries;
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
            "7qf3845qydavuucmhj96b6hd;5jYe5FVhhF",// original one
            "hd5n8g4x8wz53ug34k65s8ag;QfhzFdfPt4",    
            "vghqkpd86meujk5y3fajvafq;BNR5pvUnzs",  
            "h6chq7dpqhw3pwsa9bvp5r9f;PCquKKNQJA",    
            "9abckmw54a5hskru6knt3zkr;FgedX4SSHB",    
            "nz2gyja59hq9ne22b7wfwpjb;wT2GmJ6xdZ",    
            "y7xw6vrkxqxnmguyff6825k5;XX4FWeBVPg",    
            "ym68z674zmkpszk7mf7z9fuz;eVZYNummyZ",    
            "dfh77wfcv3afvugjrjvcgm8y;GjKHCbQrHR",    
            "vntdbbpcja5huvqfd6ypcmvx;enn4mwrAPm"};
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            foreach (T v in values)
                if (!collection.Contains(v))
                    collection.Add(v);
        }

        public static String UrlFriendly(this String source)
        {
            if (source == null)
                return null;
            else
                return source.Replace(" ", "-").Replace(":", "-")
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
            Random r = new Random();
            String details = accounts[r.Next(0, 9)];
            String[] account = details.Split(new[] { ';' });
            Netflix.Login.SetCredentials(account[0], account[1], "FilmTrove");
            return Netflix.Fill;
        }
        public static NetflixSearch Randomized(this NetflixSearch source)
        {
            Random r = new Random();
            String details = accounts[r.Next(0, 9)];
            String[] account = details.Split(new[] { ';' });
            Netflix.Login.SetCredentials(account[0], account[1], "FilmTrove");
            return Netflix.Search;
        }
    }
}