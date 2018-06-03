using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public static class TypeExtensions 
    {
        public static bool HasGenericDefinition(this Type type, Type genericTypeDefinition)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;
        }

        public static bool HasGenericArguments(this Type type, params Type[] typeArguments)
        {
            return type.IsGenericType && type.GetGenericArguments().SequenceEqual(typeArguments);
        }

        public static bool HasGenericSignature(this Type type, Type genericTypeDefinition, params Type[] typeArguments)
        {
            return HasGenericDefinition(type, genericTypeDefinition)
                && HasGenericArguments(type, typeArguments);
        }

        public static bool IsExtension(this MethodInfo method)
        {
            return method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
        }
        public static bool IsExtensionOf(this MethodInfo method, Type type)
        {
            if (method.IsExtension())
            {
                var extensionType = method.GetParameters().FirstOrDefault();
                if (extensionType != null)
                {
                    return type.IsAssignableTo(extensionType.ParameterType)
                        || (extensionType.ParameterType.IsGenericType && type.IsGenericType
                        && extensionType.ParameterType.GetGenericTypeDefinition().IsAssignableFromGenericType(
                            type.GetGenericTypeDefinition()));
                }
            }
            return false;
        }

        public static IEnumerable<MethodInfo> GetExtensionsMethods(this Type type, IEnumerable<Assembly> assemblies = null)
        {
            return
                (assemblies ?? AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic))
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass && !x.IsGenericTypeDefinition
                    && x.IsSealed && x.IsAbstract && x.GetConstructor(Type.EmptyTypes) == null)
                .SelectMany(x => x.GetMethods(
                        BindingFlags.Static |
                        BindingFlags.Public | BindingFlags.NonPublic))
                .Where(x => x.GetParameters().Length > 0
                    && x.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
                    && x.GetParameters().Length > 0)
                .Select(x => new {
                    Method = x,
                    ExtendedType = x.GetParameters().First().ParameterType
                })
                .Where(x =>
                    x.ExtendedType.IsAssignableFrom(type)
                   || (x.ExtendedType.IsGenericType && type.IsGenericType
                       && x.ExtendedType.GetGenericTypeDefinition().IsAssignableFromGenericType(type.GetGenericTypeDefinition())
                    ))
                .Select(x => x.Method);
        }



        public static bool IsAssignableTo(this Type givenType, Type targetType)
        {
            return targetType.IsAssignableFrom(givenType);
        }

        public static bool IsAssignableFromGenericType(this Type targetType, Type givenType)
        {
            if (givenType == null || targetType == null)
            {
                return false;
            }

            return givenType == targetType
              || givenType.MapsToGenericTypeDefinition(targetType)
              || givenType.HasInterfaceThatMapsToGenericTypeDefinition(targetType)
              || givenType.BaseType.IsAssignableToGenericType(targetType);
        }

        public static bool IsAssignableToGenericType(this Type givenType, Type targetType)
        {
            if (givenType == null || targetType == null)
            {
                return false;
            }

            return givenType == targetType
              || givenType.MapsToGenericTypeDefinition(targetType)
              || givenType.HasInterfaceThatMapsToGenericTypeDefinition(targetType)
              || givenType.BaseType.IsAssignableToGenericType(targetType);
        }

        private static bool HasInterfaceThatMapsToGenericTypeDefinition(this Type givenType, Type genericType)
        {
            return givenType
              .GetInterfaces()
              .Where(it => it.IsGenericType)
              .Any(it => it.GetGenericTypeDefinition() == genericType);
        }

        private static bool MapsToGenericTypeDefinition(this Type givenType, Type genericType)
        {
            return genericType.IsGenericTypeDefinition
              && givenType.IsGenericType
              && givenType.GetGenericTypeDefinition() == genericType;
        }

        private static bool IsDelegate(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type);
        }

        
        public static object DefaultOf(this Type type)
        {
            return defaultOfGenericMethod.Value
                .MakeGenericMethodCached(type)
                .Invoke(null, null);
        }

        static readonly Lazy<MethodInfo> defaultOfGenericMethod = new Lazy<MethodInfo>(() =>
            typeof(TypeExtensions).GetMethod(nameof(DefaultOfInternal), BindingFlags.Static | BindingFlags.NonPublic)
        );

        private static T DefaultOfInternal<T>()
        {
            return default(T);
        }
    }
}
