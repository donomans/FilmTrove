using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FilmTrove.Code
{
    public static class GeneralExtensions
    {
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
                return source.Replace(" ", "_").Replace(":", "-")
                .Replace("'", "").Replace("?", "").Replace(",", "")
                .Replace("&", "and").Replace("/", "-"); ;
        }
        public static String UrlFriendly(this String source, Int32 year)
        {
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
    }
}