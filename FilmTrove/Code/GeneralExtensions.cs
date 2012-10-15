using System;
using System.Collections.Generic;
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
    }
}