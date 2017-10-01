using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public static class PropertyInfoExTypeExtensions
    {
        public static PropertyInfoEx GetPropertyEx(this PropertyInfo PropertyInfo, bool EnableCaching = true)
        {
            if (EnableCaching)
                return PropertyInfoExCache.GetPropertyEx(PropertyInfo);
            else
                return new PropertyInfoEx(PropertyInfo);
        }

        public static IReadOnlyDictionary<string, PropertyInfoEx> GetPropertiesExDic(this Type Type, bool EnableCaching = true)
        {
            if (EnableCaching)
                return PropertyInfoExCache.GetPropertiesExDic(Type);
            else
                return new ReadOnlyDictionary<string, PropertyInfoEx>(Type.GetProperties().ToDictionary(x => x.Name, x => new PropertyInfoEx(x)));
        }

        public static IEnumerable<PropertyInfoEx> GetPropertiesEx(this Type Type, bool EnableCaching = true)
        {
            if (EnableCaching)
                foreach (var prop in PropertyInfoExCache.GetPropertiesExDic(Type).Values)
                    yield return prop;
            else
                foreach (var prop in Type.GetProperties())
                    yield return new PropertyInfoEx(prop);
        }

        public static PropertyInfoEx GetPropertyEx(this Type Type, string PropertyName, bool EnableCaching = true)
        {
            if (EnableCaching)
            {
                PropertyInfoEx prop = null;
                PropertyInfoExCache.GetPropertiesExDic(Type).TryGetValue(PropertyName, out prop);
                return prop;
            }
            else
            {
                var prop = Type.GetProperty(PropertyName);
                if (prop != null)
                    return new PropertyInfoEx(prop);
                else
                    return null;
            }
        }
    }
}
