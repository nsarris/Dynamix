using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public static class PropertyInfoExTypeExtensions
    {
        const BindingFlags PUBLIC_ISTANCE_STATIC = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        public static PropertyInfoEx GetPropertyEx(this PropertyInfo PropertyInfo, bool enableCaching = true)
        {
            if (enableCaching)
                return PropertyInfoExCache.GetPropertyEx(PropertyInfo);
            else
                return new PropertyInfoEx(PropertyInfo, false);
        }

        public static PropertyInfoEx GetPropertyEx(this Type type, string name, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return PropertyInfoExCache.GetPropertyEx(type, name, bindingFlags);
            else
            {
                var f = type.GetProperty(name, bindingFlags);
                if (f == null) return null;
                return new PropertyInfoEx(f, false);
            }
        }

        public static IReadOnlyDictionary<string, PropertyInfoEx> GetPropertiesExDic(this Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return PropertyInfoExCache.GetPropertiesExDic(type, bindingFlags);
            else
                return new ReadOnlyDictionary<string, PropertyInfoEx>(
                    type.GetProperties(bindingFlags).ToDictionary(x => x.Name, x => new PropertyInfoEx(x, false)));
        }

        public static IEnumerable<PropertyInfoEx> GetPropertiesEx(this Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return PropertyInfoExCache.GetPropertiesEx(type, bindingFlags);
            else
                return type.GetProperties(bindingFlags).Select(x => new PropertyInfoEx(x,false));
        }

        public static PropertyInfoEx GetPropertyEx(this Type type, string name, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetPropertyEx(type, name, (BindingFlags)bindingFlags, enableCaching);
        }

        public static IReadOnlyDictionary<string, PropertyInfoEx> GetPropertiesExDic(this Type type, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetPropertiesExDic(type, (BindingFlags)bindingFlags, enableCaching);
        }

        public static IEnumerable<PropertyInfoEx> GetPropertiesEx(this Type type, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetPropertiesEx(type, (BindingFlags)bindingFlags,enableCaching);
        }
    }
}
