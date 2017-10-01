using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public static class MethodInfoExTypeExtensions
    {
        const BindingFlags PUBLIC_ISTANCE_STATIC = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        public static MethodInfoEx GetMethodEx(this MethodInfo methodInfo, bool enableCaching = true)
        {
            if (enableCaching)
                return MethodInfoExCache.GetMethodEx(methodInfo);
            else
                return new MethodInfoEx(methodInfo, false);
        }


        public static MethodInfoEx GetMethodEx(this Type type, string name, IEnumerable<Type> signature, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return MethodInfoExCache.GetMethodEx(type, name, signature, bindingFlags);
            else
            {
                var f = (signature == null) ? type.GetMethod(name, bindingFlags) : type.GetMethod(name, bindingFlags, null, signature.ToArray(), null);
                if (f == null) return null;
                return new MethodInfoEx(f, false);
            }
        }

        public static MethodInfoEx GetMethodEx(this Type type, string name, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            return GetMethodEx(type, name, null, bindingFlags, enableCaching);
        }

        public static IReadOnlyDictionary<string, MethodInfoEx> GetMethodsExDic(this Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {

            if (enableCaching)
                return MethodInfoExCache.GetMethodsExDic(type, bindingFlags);
            else
                return new ReadOnlyDictionary<string, MethodInfoEx>(
                    type.GetMethods(bindingFlags).ToDictionary(x => x.Name, x => new MethodInfoEx(x, false)));
        }

        public static IEnumerable<MethodInfoEx> GetMethodsEx(this Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return MethodInfoExCache.GetMethodsEx(type, bindingFlags);
            else
                return type.GetMethods(bindingFlags).Select(x => GetMethodEx(x, false));
        }


        public static MethodInfoEx GetMethodEx(this Type type, string name, IEnumerable<Type> signature, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetMethodEx(type, name, signature, (BindingFlags)bindingFlags, enableCaching);
        }
        public static MethodInfoEx GetMethodEx(this Type type, string name, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetMethodEx(type, name, (BindingFlags)bindingFlags, enableCaching);
        }

        public static IReadOnlyDictionary<string, MethodInfoEx> GetMethodsExDic(this Type type, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetMethodsExDic(type, (BindingFlags)bindingFlags, enableCaching);
        }

        public static IEnumerable<MethodInfoEx> GetMethodsEx(this Type type, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetMethodsEx(type, (BindingFlags)bindingFlags, enableCaching);
        }
    }
}
