using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Dynamix
{
    public static class FieldInfoExCache
    {
        static readonly BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        static Dictionary<Type, IReadOnlyDictionary<string, FieldInfoEx>> typeCache
            = new Dictionary<Type, IReadOnlyDictionary<string, FieldInfoEx>>();

        static Dictionary<FieldInfo, FieldInfoEx> fieldCache
            = new Dictionary<FieldInfo, FieldInfoEx>();

        static object oType = new object();
        static object oProp = new object();

        public static FieldInfoEx GetFieldEx(FieldInfo FieldInfo)
        {
            FieldInfoEx prop;

            if (!fieldCache.TryGetValue(FieldInfo, out prop))
            {
                lock (oProp)
                {
                    fieldCache.TryGetValue(FieldInfo, out prop);
                    if (prop == null)
                    {
                        prop = new FieldInfoEx(FieldInfo);
                        fieldCache.Add(FieldInfo, prop);
                    }
                }
            }

            return prop;
        }

        public static IReadOnlyDictionary<string, FieldInfoEx> GetFieldsExDic(Type Type)
        {
            IReadOnlyDictionary<string, FieldInfoEx> props;

            if (!typeCache.TryGetValue(Type, out props))
            {
                lock (oType)
                {
                    if (!typeCache.TryGetValue(Type, out props))
                    {
                        props = new ReadOnlyDictionary<string, FieldInfoEx>(
                            Type.GetFields(defaultBindingFlags)
                            .Where(x => x.GetCustomAttribute<FieldInfoExIgnoreAttribute>() == null)
                            .ToDictionary(x => x.Name, x => GetFieldEx(x)));
                        typeCache.Add(Type, props);
                    }
                }
            }

            return props;
        }

        public static IEnumerable<FieldInfoEx> GetFieldsEx(Type Type)
        {
            return Type.GetFieldsExDic().Values;
        }

        public static void ClearCache() { fieldCache.Clear(); typeCache.Clear(); }
        public static void RemoveFromCache(Type t) { typeCache.Remove(t); fieldCache.Where(x => x.Value.FieldInfo.ReflectedType == t).ToList().ForEach(x => { fieldCache.Remove(x.Key); }); }

    }
}
