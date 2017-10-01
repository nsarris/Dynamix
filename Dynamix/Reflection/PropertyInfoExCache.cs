using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Dynamix
{
    public static class PropertyInfoExCache
    {
        static Dictionary<Type, IReadOnlyDictionary<string, PropertyInfoEx>> typeCache
            = new Dictionary<Type, IReadOnlyDictionary<string, PropertyInfoEx>>();

        static Dictionary<PropertyInfo, PropertyInfoEx> propCache
            = new Dictionary<PropertyInfo, PropertyInfoEx>();

        static object oType = new object();
        static object oProp = new object();

        public static PropertyInfoEx GetPropertyEx(PropertyInfo PropertyInfo)
        {
            PropertyInfoEx prop;

            if (!propCache.TryGetValue(PropertyInfo, out prop))
            {
                lock (oProp)
                {
                    propCache.TryGetValue(PropertyInfo, out prop);
                    if (prop == null)
                    {
                        prop = new PropertyInfoEx(PropertyInfo);
                        propCache.Add(PropertyInfo, prop);
                    }
                }
            }

            return prop;
        }

        public static IReadOnlyDictionary<string, PropertyInfoEx> GetPropertiesExDic(Type Type)
        {
            IReadOnlyDictionary<string, PropertyInfoEx> props;

            if (!typeCache.TryGetValue(Type, out props))
            {
                lock (oType)
                {
                    if (!typeCache.TryGetValue(Type, out props))
                    {
                        props = new ReadOnlyDictionary<string, PropertyInfoEx>(
                            Type.GetProperties()
                            .Where(x => x.GetCustomAttribute<PropertyInfoExIgnoreAttribute>() == null)
                            //Ignore this property whose getters/setters have +1 parameters
                            .Where(x => (x.GetMethod == null || !x.GetMethod.GetParameters().Any())
                                && (x.SetMethod == null || x.SetMethod.GetParameters().Count() == 1))
                            .ToDictionary(x => x.Name, x => GetPropertyEx(x)));
                        typeCache.Add(Type, props);
                    }

                }
            }

            return props;
        }

        public static IEnumerable<PropertyInfoEx> GetPropertiesEx(Type Type)
        {
            return Type.GetPropertiesExDic().Values;
        }

        public static void ClearCache() { propCache.Clear(); typeCache.Clear(); }
        public static void RemoveFromCache(Type t) { typeCache.Remove(t); propCache.Where(x => x.Value.PropertyInfo.ReflectedType == t).ToList().ForEach(x => { propCache.Remove(x.Key); }); }

    }
}
