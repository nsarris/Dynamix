using Dynamix.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    internal static class EnumerableExtensions
    {
        internal static class Methods
        {
            internal static readonly MethodInfo Any = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Any) && x.GetParameters().Length == 1).FirstOrDefault();
            internal static readonly MethodInfo Cast = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast));
            internal static readonly MethodInfo Contains = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Contains) && x.GetParameters().Length == 2).FirstOrDefault();
            internal static readonly MethodInfo Count = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Count) && x.GetParameters().Length == 1).FirstOrDefault();
            internal static readonly MethodInfo ToList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList));
        }

        private static IEnumerable Prepare(this IEnumerable enumerable, ref Type elementType)
        {
            if (elementType == null) elementType = typeof(object);
            return enumerable.DynamicCast(elementType);
        }

        public static bool DynamicContains(this IEnumerable enumerable, object item)
        {
            if (item == null)
                return false;

            var elementType = item.GetType();

            return (bool)Methods.Contains.MakeGenericMethodCached(elementType).Invoke(null, new object[] { enumerable, item });
        }

        public static bool DynamicAny(this IEnumerable enumerable, Type elementType = null)
        {
            enumerable = enumerable.Prepare(ref elementType);

            return (bool)Methods.Any.MakeGenericMethodCached(elementType).Invoke(null, new object[] { enumerable });
        }

        public static int DynamicCount(this IEnumerable enumerable, Type elementType = null)
        {
            enumerable = enumerable.Prepare(ref elementType);

            return (int)Methods.Count.MakeGenericMethodCached(elementType).Invoke(null, new object[] { enumerable });
        }

        public static IEnumerable DynamicToList(this IEnumerable enumerable, Type elementType = null)
        {
            enumerable = enumerable.Prepare(ref elementType);

            return (IEnumerable)Methods.ToList.MakeGenericMethodCached(elementType).Invoke(null, new object[] { enumerable });
        }

        public static IEnumerable DynamicCast(this IEnumerable enumerable, Type typeToCastTo = null)
        {
            if (typeToCastTo == null)
                typeToCastTo = typeof(object);

            return (IEnumerable)Methods.Cast.MakeGenericMethodCached(typeToCastTo).Invoke(null, new object[] { enumerable });
        }

        public static IEnumerable ToCastedList(this IEnumerable enumerable, Type typeToCastTo = null)
        {
            return enumerable.DynamicCast(typeToCastTo).DynamicToList(typeToCastTo);
        }
    }
}
