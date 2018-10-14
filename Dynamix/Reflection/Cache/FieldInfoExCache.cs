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

    internal static class FieldInfoExCache
    {
        const BindingFlags PUBLIC_ISTANCE_STATIC = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        static ConcurrentDictionary<FieldInfo, FieldInfoEx> cache
            = new ConcurrentDictionary<FieldInfo, FieldInfoEx>();



        public static FieldInfoEx GetFieldEx(FieldInfo fieldInfo)
        {
            if (!cache.TryGetValue(fieldInfo, out var prop))
            {
                prop = new FieldInfoEx(fieldInfo);
                cache.TryAdd(fieldInfo, prop);
            }

            return prop;
        }

        public static FieldInfoEx GetFieldEx(Type type, string name, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            var f = type.GetField(name, bindingFlags);
            if (f == null) return null;
            return GetFieldEx(f);
        }

        public static IReadOnlyDictionary<string, FieldInfoEx> GetFieldsExDic(Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return new ReadOnlyDictionary<string, FieldInfoEx>(
                GetFieldsEx(type, bindingFlags)
                .ToDictionary(x => x.Name));
        }

        public static IEnumerable<FieldInfoEx> GetFieldsEx(Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return type.GetFields(bindingFlags).Select(x => GetFieldEx(x));
        }

        public static FieldInfoEx GetFieldEx(Type type, string name, BindingFlagsEx bindingFlags)
        {
            return GetFieldEx(type, name, (BindingFlags)bindingFlags);
        }

        public static IReadOnlyDictionary<string, FieldInfoEx> GetFieldsExDic(Type type, BindingFlagsEx bindingFlags)
        {
            return GetFieldsExDic(type, (BindingFlags)bindingFlags);
        }

        public static IEnumerable<FieldInfoEx> GetFieldsEx(Type type, BindingFlagsEx bindingFlags)
        {
            return GetFieldsEx(type, (BindingFlags)bindingFlags);
        }

        //public static void ClearCache() { fieldCache.Clear(); }
        //public static void RemoveFromCache(Type t) { fieldCache.Where(x => x.Value.FieldInfo.ReflectedType == t).ToList().ForEach(x => { fieldCache.TryRemove(x.Key, out var vv); }); }

    }
}
