using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public static class FieldInfoExTypeExtensions
    {
        private static readonly BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static FieldInfoEx GetFieldEx(this FieldInfo FieldInfo, bool EnableCaching = true)
        {
            if (EnableCaching)
                return FieldInfoExCache.GetFieldEx(FieldInfo);
            else
                return new FieldInfoEx(FieldInfo);
        }

        public static IReadOnlyDictionary<string, FieldInfoEx> GetFieldsExDic(this Type Type, bool EnableCaching = true)
        {
            if (EnableCaching)
                return FieldInfoExCache.GetFieldsExDic(Type);
            else
                return new ReadOnlyDictionary<string, FieldInfoEx>(
                    Type.GetFields(defaultBindingFlags).ToDictionary(x => x.Name, x => new FieldInfoEx(x)));
        }

        public static IEnumerable<FieldInfoEx> GetFieldsEx(this Type Type, bool EnableCaching = true)
        {
            if (EnableCaching)
                foreach (var field in FieldInfoExCache.GetFieldsExDic(Type).Values)
                    yield return field;
            else
                foreach (var field in Type.GetFields(defaultBindingFlags))
                    yield return new FieldInfoEx(field);
        }

        public static FieldInfoEx GetFieldEx(this Type Type, string FieldName, bool EnableCaching = true)
        {
            if (EnableCaching)
            {
                FieldInfoEx field = null;
                FieldInfoExCache.GetFieldsExDic(Type).TryGetValue(FieldName, out field);
                return field;
            }
            else
            {
                var field = Type.GetField(FieldName);
                if (field != null)
                    return new FieldInfoEx(field);
                else
                    return null;
            }
        }
    }
}
