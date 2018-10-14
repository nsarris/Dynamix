using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public static class FieldInfoExTypeExtensions
    {
        const BindingFlags PUBLIC_ISTANCE_STATIC = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        public static FieldInfoEx GetFieldEx(this FieldInfo fieldInfo, bool enableCaching = true)
        {
            if (enableCaching)
                return FieldInfoExCache.GetFieldEx(fieldInfo);
            else
                return new FieldInfoEx(fieldInfo, false);
        }

        public static FieldInfoEx GetFieldEx(this Type type, string name, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return FieldInfoExCache.GetFieldEx(type, name, bindingFlags);
            else
            {
                var f = type.GetField(name, bindingFlags);
                if (f == null) return null;
                return new FieldInfoEx(f, false);
            }
        }

        public static IReadOnlyDictionary<string, FieldInfoEx> GetFieldsExDic(this Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return FieldInfoExCache.GetFieldsExDic(type, bindingFlags);
            else
                return new ReadOnlyDictionary<string, FieldInfoEx>(
                    type.GetFields(bindingFlags).ToDictionary(x => x.Name, x => new FieldInfoEx(x, false)));
        }

        public static IEnumerable<FieldInfoEx> GetFieldsEx(this Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return FieldInfoExCache.GetFieldsEx(type, bindingFlags);
            else
                return type.GetFields(bindingFlags).Select(x => new FieldInfoEx(x, false));
        }

        public static FieldInfoEx GetFieldEx(this Type type, string name, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetFieldEx(type, name, (BindingFlags)bindingFlags, enableCaching);
        }

        public static IReadOnlyDictionary<string, FieldInfoEx> GetFieldsExDic(this Type type, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetFieldsExDic(type, (BindingFlags)bindingFlags, enableCaching);
        }

        public static IEnumerable<FieldInfoEx> GetFieldsEx(this Type type, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetFieldsEx(type, (BindingFlags)bindingFlags, enableCaching);
        }
    }
}
