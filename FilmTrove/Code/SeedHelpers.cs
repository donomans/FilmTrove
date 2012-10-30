using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace FilmTrove.Code
{
    public static class SeedHelpers
    {
        public static void SeederCacheCallback(String k, Object v, CacheItemRemovedReason r)
        {
            Object t = v;
            if (t != null && t is List<String>)
            {
                foreach (String title in (List<String>)t)
                {
                    ///churn through
                }
            }
        }
    }
}