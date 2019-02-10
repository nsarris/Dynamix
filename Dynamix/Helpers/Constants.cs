using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    internal static class Constants
    {
        private static ConcurrentDictionary<Type, Array> emptyArrayCache = new ConcurrentDictionary<Type, Array>();

        public static Array GetEmptyArray(Type elementType)
        {
            if (!emptyArrayCache.TryGetValue(elementType, out var array))
            {
                array = Array.CreateInstance(elementType, 0);
                emptyArrayCache.TryAdd(elementType, array);
            }
            return array;
        }

        public static T[] GetEmptyArray<T>()
        {
            return (T[])GetEmptyArray(typeof(T));
        }

        public static object[] EmptyObjectArray { get; } = GetEmptyArray<object>();
        public static string[] EmptyStringArray { get; } = GetEmptyArray<string>();
    }
}
