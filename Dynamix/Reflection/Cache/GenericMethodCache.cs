using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    internal static class GenericMethodCache
    {
        private class MethodKey : Tuple<MethodInfo, Type[]>
        {
            public MethodKey(MethodInfo GenericMethodDefiniton, Type[] GenericMethodArguments)
                : base(GenericMethodDefiniton, GenericMethodArguments)
            {

            }
        }
        private static ConcurrentDictionary<MethodKey, MethodInfo> cache = new ConcurrentDictionary<MethodKey, MethodInfo>();

        public static MethodInfo GetCachedGenericMethod(MethodInfo GenericMethodDefiniton, params Type[] GenericMethodArguments)
        {
            if (GenericMethodDefiniton == null)
                throw new ArgumentNullException(nameof(GenericMethodDefiniton));

            if (GenericMethodArguments == null)
                throw new ArgumentNullException(nameof(GenericMethodArguments));

            var key = new MethodKey(GenericMethodDefiniton, GenericMethodArguments);
            if (!cache.TryGetValue(key, out var method))
            {
                method = GenericMethodDefiniton.MakeGenericMethod(GenericMethodArguments);
                cache.TryAdd(key, method);
            }
            return method;
        }
    }

    public static class GenericMethodExtensions
    {
        public static MethodInfo MakeGenericMethodCached(this MethodInfo GenericMethodDefiniton, params Type[] GenericMethodArguments)
        {
            return GenericMethodCache.GetCachedGenericMethod(GenericMethodDefiniton, GenericMethodArguments);
        }
    }
}
