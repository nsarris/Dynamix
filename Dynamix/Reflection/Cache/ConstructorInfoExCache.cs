using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;

namespace Dynamix.Reflection
{
    internal static class ConstructorInfoExCache
    {
        const BindingFlags PUBLIC_ISTANCE_STATIC = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        static ConcurrentDictionary<ConstructorInfo, ConstructorInfoEx> cache
            = new ConcurrentDictionary<ConstructorInfo, ConstructorInfoEx>();
        
        public static ConstructorInfoEx GetConstructorEx(ConstructorInfo constructorInfo)
        {
            if (!cache.TryGetValue(constructorInfo, out var prop))
            {
                prop = new ConstructorInfoEx(constructorInfo);
                cache.TryAdd(constructorInfo, prop);
            }

            return prop;
        }

        public static ConstructorInfoEx GetConstructorEx(Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return GetConstructorEx(type, Type.EmptyTypes, bindingFlags);
        }

        public static ConstructorInfoEx GetConstructorEx(Type type, IEnumerable<Type> signature = null, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            var f = type.GetConstructor(bindingFlags, null,signature?.ToArray() ?? Type.EmptyTypes,null);
            if (f == null) return null;
            return GetConstructorEx(f);
        }
        
        public static IReadOnlyDictionary<string, ConstructorInfoEx> GetConstructorsExDic(Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return new ReadOnlyDictionary<string, ConstructorInfoEx>(
                GetConstructorsEx(type, bindingFlags)
                .ToDictionary(x => x.Name));
        }

        public static IEnumerable<ConstructorInfoEx> GetConstructorsEx(Type type,BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return type.GetConstructors(bindingFlags).Select(x => GetConstructorEx(x));
        }
    }
}
