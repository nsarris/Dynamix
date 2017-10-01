using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    internal static class GenericTypeCache
    {
        private class TypeKey : Tuple<Type, Type[]>
        {
            public TypeKey(Type GenericTypeDefiniton, Type[] GenericTypeArguments)
                : base(GenericTypeDefiniton, GenericTypeArguments)
            {

            }
        }
        private static ConcurrentDictionary<TypeKey, Type> cache = new ConcurrentDictionary<TypeKey, Type>();

        public static Type GetCachedGenericType(Type GenericTypeDefiniton, IEnumerable<Type> GenericTypeArguments)
        {
            return GetCachedGenericType(GenericTypeDefiniton, GenericTypeArguments.ToArray());
        }

        public static Type GetCachedGenericType(Type GenericTypeDefiniton, params Type[] GenericTypeArguments)
        {
            if (GenericTypeDefiniton == null)
                throw new ArgumentNullException(nameof(GenericTypeDefiniton));

            if (GenericTypeArguments == null)
                throw new ArgumentNullException(nameof(GenericTypeArguments));

            var key = new TypeKey(GenericTypeDefiniton, GenericTypeArguments);
            if (!cache.TryGetValue(key, out var type))
            {
                type = GenericTypeDefiniton.MakeGenericType(GenericTypeArguments);
                cache.TryAdd(key, type);
            }
            return type;
        }


    }

    public static class GenericTypeExtensions
    {
        public static Type MakeGenericTypeCached(this Type GenericTypeDefiniton, IEnumerable<Type> GenericTypeArguments)
        {
            return GenericTypeCache.GetCachedGenericType(GenericTypeDefiniton, GenericTypeArguments);
        }

        public static Type MakeGenericTypeCached(this Type GenericTypeDefiniton, params Type[] GenericTypeArguments)
        {
            return GenericTypeCache.GetCachedGenericType(GenericTypeDefiniton, GenericTypeArguments);
        }

        public static bool IsFunc(this Type Type)
        {
            return Type.IsGenericType && Type.GetGenericArguments().Any() && Type == GetFuncGenericType(Type.GetGenericArguments());
        }

        public static bool IsAction(this Type Type)
        {
            return Type.IsGenericType && Type == GetActionGenericType(Type.GetGenericArguments());
        }

        public static Type GetFuncGenericType(params Type[] TypeArguments)
        {
            return GetFuncGenericType(TypeArguments.AsEnumerable());
        }

        public static Type GetActionGenericType(params Type[] TypeArguments)
        {
            return GetActionGenericType(TypeArguments.AsEnumerable());
        }

        public static Type GetFuncGenericType(IEnumerable<Type> TypeArguments)
        {
            var argumentCount = TypeArguments.Count();

            if (argumentCount == 0)
                throw new ArgumentException("Func<> needs at least one type argument");
            if (argumentCount > 17)
                throw new ArgumentException("Func<> can have 17 type arguments at most");

            var type = Type.GetType("System.Func`" + argumentCount);
            return MakeGenericTypeCached(type, TypeArguments);
        }

        public static Type GetActionGenericType(IEnumerable<Type> TypeArguments)
        {
            var argumentCount = TypeArguments.Count();
            if (argumentCount > 16)
                throw new ArgumentException("Action<> can have 16 type arguments at most");

            var type = Type.GetType("System.Action`" + argumentCount);
            return MakeGenericTypeCached(type, TypeArguments);
        }
    }


}
