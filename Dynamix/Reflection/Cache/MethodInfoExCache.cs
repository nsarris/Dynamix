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
    internal static class MethodInfoExCache
    {
        const BindingFlags PUBLIC_ISTANCE_STATIC = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        static ConcurrentDictionary<MethodInfo, MethodInfoEx> cache
            = new ConcurrentDictionary<MethodInfo, MethodInfoEx>();

        public static MethodInfoEx GetMethodEx(MethodInfo methodInfo)
        {
            if (!cache.TryGetValue(methodInfo, out var method))
            {
                method = new MethodInfoEx(methodInfo);
                cache.TryAdd(methodInfo, method);
            }

            return method;
        }

        
        public static MethodInfoEx GetMethodEx(Type type, string name, IEnumerable<Type> signature, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            var f = (signature == null) ? type.GetMethod(name, bindingFlags) : type.GetMethod(name, bindingFlags,null, signature.ToArray(), null);
            if (f == null) return null;
            return GetMethodEx(f);
        }

        public static MethodInfoEx GetMethodEx(Type type, string name, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return GetMethodEx(type, name, null, bindingFlags);
        }

        public static IReadOnlyDictionary<string, MethodInfoEx> GetMethodsExDic(Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return new ReadOnlyDictionary<string, MethodInfoEx>(
                GetMethodsEx(type, bindingFlags)
                .ToDictionary(x => x.Name));
        }

        public static IEnumerable<MethodInfoEx> GetMethodsEx(Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC)
        {
            return type.GetMethods(bindingFlags).Select(x => GetMethodEx(x));
        }

        
        public static MethodInfoEx GetMethodEx(Type type, string name, IEnumerable<Type> signature, BindingFlagsEx bindingFlags)
        {
            return GetMethodEx(type, name, signature, (BindingFlags)bindingFlags);
        }
        public static MethodInfoEx GetMethodEx(Type type, string name, BindingFlagsEx bindingFlags)
        {
            return GetMethodEx(type, name, (BindingFlags)bindingFlags);
        }

        public static IReadOnlyDictionary<string, MethodInfoEx> GetMethodsExDic(Type type, BindingFlagsEx bindingFlags)
        {
            return GetMethodsExDic(type, (BindingFlags)bindingFlags);
        }

        public static IEnumerable<MethodInfoEx> GetMethodsEx(Type type, BindingFlagsEx bindingFlags)
        {
            return GetMethodsEx(type, (BindingFlags)bindingFlags);
        }
    }
}
