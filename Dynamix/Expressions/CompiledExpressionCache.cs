using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{
    internal static class CompiledExpressionCache
    {
        private static ConcurrentDictionary<LambdaExpression, object> cache 
            = new ConcurrentDictionary<LambdaExpression, object>();

        public static object GetCached(LambdaExpression l)
        {
            if (!cache.TryGetValue(l, out var d))
            {
                d = l.Compile();
                cache.TryAdd(l, d);
            }

            return d;
        }
    }

    public static class CompiledExpressionCacheExtensions
    {
        public static Delegate CompileCached(this LambdaExpression lambda)
        {
            return (Delegate)(CompiledExpressionCache.GetCached(lambda));
        }
        public static TDelegate CompileCached<TDelegate>(this Expression<TDelegate> lambda)
        {
            return (TDelegate)(CompiledExpressionCache.GetCached(lambda));
        }
    }
}
