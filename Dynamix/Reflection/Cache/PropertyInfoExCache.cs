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
    internal static class PropertyInfoExCache
    {
        const BindingFlags PUBLIC_ISTANCE_STATIC = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        static ConcurrentDictionary<PropertyInfo, PropertyInfoEx> cache
            = new ConcurrentDictionary<PropertyInfo, PropertyInfoEx>();



        public static PropertyInfoEx GetPropertyEx(PropertyInfo PropertyInfo)
        {
            if (!cache.TryGetValue(PropertyInfo, out var prop))
            {
                prop = new PropertyInfoEx(PropertyInfo);
                cache.TryAdd(PropertyInfo, prop);
            }

            return prop;
        }

        public static PropertyInfoEx GetPropertyEx(Type type, string name, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            var f = type.GetProperty(name, bindingFlags);
            if (f == null) return null;
            return GetPropertyEx(f);
        }

        public static IReadOnlyDictionary<string, PropertyInfoEx> GetPropertiesExDic(Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return new ReadOnlyDictionary<string, PropertyInfoEx>(
                GetPropertiesEx(type, bindingFlags)
                .ToDictionary(x => x.Name));
        }

        public static IEnumerable<PropertyInfoEx> GetPropertiesEx(Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return type.GetProperties(bindingFlags).Select(x => GetPropertyEx(x));
        }

        public static PropertyInfoEx GetPropertyEx(Type type, string name, BindingFlagsEx bindingFlags)
        {
            return GetPropertyEx(type, name, (BindingFlags)bindingFlags);
        }

        public static IReadOnlyDictionary<string, PropertyInfoEx> GetPropertiesExDic(Type type, BindingFlagsEx bindingFlags)
        {
            return GetPropertiesExDic(type, (BindingFlags)bindingFlags);
        }

        public static IEnumerable<PropertyInfoEx> GetPropertiesEx(Type type, BindingFlagsEx bindingFlags)
        {
            return GetPropertiesEx(type, (BindingFlags)bindingFlags);
        }
    }
}
