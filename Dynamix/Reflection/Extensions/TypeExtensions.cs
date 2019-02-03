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
                (assemblies ?? AssemblyReflector.GetLoadedAssemblies().Where(x => !x.IsDynamic))
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass && !x.IsGenericTypeDefinition
                    && x.IsSealed && x.IsAbstract && x.GetConstructor(Type.EmptyTypes) == null)
                .SelectMany(x => x.GetMethods(
                        BindingFlags.Static |
                        BindingFlags.Public | BindingFlags.NonPublic))
                .Where(x => x.GetParameters().Length > 0
                    && x.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
                    && x.GetParameters().Length > 0)
                .Select(x => new
                {
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
                return false;

            return givenType == targetType
              || givenType.HasGenericTypeDefinition(targetType)
              || givenType.ImplementsGeneric(targetType)
              || givenType.BaseType.IsAssignableToGenericType(targetType);
        }

        public static bool IsAssignableToGenericType(this Type givenType, Type targetType)
        {
            return targetType.IsAssignableFromGenericType(givenType);
        }

        public static bool Implements(this Type givenType, Type interfaceType)
        {
            AssertInterface(nameof(interfaceType), interfaceType);

            return givenType
              .GetInterfaces()
              .Contains(interfaceType);
        }

        public static bool ImplementsGeneric(this Type givenType, Type genericInterfaceType)
        {
            AssertInterface(nameof(genericInterfaceType), genericInterfaceType);

            return givenType
              .GetInterfaces()
              .Where(it => it.IsGenericType)
              .Any(it => it.GetGenericTypeDefinition() == genericInterfaceType);
        }

        public static bool ImplementsGeneric(this Type givenType, Type genericInterfaceType, out Type actualType)
        {
            actualType = null;

            AssertInterface(nameof(genericInterfaceType), genericInterfaceType);
            AssertGenericType(nameof(genericInterfaceType), genericInterfaceType);

            var interfaces = givenType
              .GetInterfaces()
              .Where(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericInterfaceType)
              .ToList();

            if (!interfaces.Any())
                return false;

            if (interfaces.Count > 1)
                throw new InvalidOperationException($"The generic interface {genericInterfaceType} is implemented {interfaces.Count} by type {givenType}.");

            actualType = interfaces.First();

            return true;
        }

        private static void AssertInterface(string paramName, Type type)
        {
            if (!type.IsInterface)
                throw new ArgumentException($"The type {type} is not an interface", paramName);
        }

        private static void AssertGenericType(string paramName, Type type)
        {
            if (!type.IsGenericType)
                throw new ArgumentException($"The type {type} is not a generic type", paramName);
        }

        private static bool HasGenericTypeDefinition(this Type givenType, Type genericType)
        {
            return genericType.IsGenericTypeDefinition
              && givenType.IsGenericType
              && givenType.GetGenericTypeDefinition() == genericType;
        }

        public static bool IsDelegate(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type);
        }

        public static bool Is<T>(this Type type)
        {
            return type == typeof(T);
        }

        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsNullable<T>(this Type type) where T : struct
        {
            return type == typeof(T?);
        }

        public static bool IsNullable(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }

        public static bool IsOrNullable<T>(this Type type) where T : struct
        {
            return type == typeof(T) || type == typeof(T?);
        }

        public static Type StripNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static Type ToNullable(this Type type)
        {
            if (type.IsNullable())
                return type;
            else if (type.IsValueType)
                return typeof(Nullable<>).MakeGenericTypeCached(type);
            else
                return type;
        }

        public static bool IsNumeric(this Type type)
        {
            return NumericTypeHelper.IsNumericType(type, false);
        }

        public static bool IsNumeric(this Type type, out NumericTypeDescriptor numericTypeDefinition)
        {
            return NumericTypeHelper.IsNumericType(type, out numericTypeDefinition, false);
        }
        public static bool IsNumericOrNullableNumeric(this Type type)
        {
            return NumericTypeHelper.IsNumericType(type);
        }

        public static bool IsNumericOrNullableNumeric(this Type type, out NumericTypeDescriptor numericTypeDefinition)
        {
            return NumericTypeHelper.IsNumericType(type, out numericTypeDefinition);
        }

        public static bool IsEnumOrNullableEnum(this Type type)
        {
            return (Nullable.GetUnderlyingType(type) ?? type).IsEnum;
        }

        public static bool IsEnum(this Type type, out Type underlyingType)
        {
            underlyingType = Enum.GetUnderlyingType(type);
            return underlyingType != null;
        }

        public static bool IsEnumOrNullableEnum(this Type type, out Type underlyingType)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            underlyingType = type.IsEnum ? Enum.GetUnderlyingType(type) : null;

            return underlyingType != null;
        }

        public static bool IsEnumerable(this Type type, out EnumerableTypeDescriptor enumerableTypeDescriptor)
        {
            enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(type);
            return (enumerableTypeDescriptor != null);
        }

        public static bool IsEnumerable(this Type type)
        {
            return EnumerableTypeDescriptor.Get(type) != null;
        }


        public static object DefaultOf(this Type type)
        {
            if (type.IsClass || type.IsInterface)
                return null;

            return defaultOfGenericMethod.Value
                .MakeGenericMethodCached(type)
                .Invoke(null, null);
        }

        static readonly Lazy<MethodInfo> defaultOfGenericMethod = new Lazy<MethodInfo>(() =>
            typeof(TypeExtensions).GetMethod(nameof(DefaultOfInternal), BindingFlags.Static | BindingFlags.NonPublic)
        );

        private static T DefaultOfInternal<T>()
        {
            return default;
        }

        public static bool IsOrSubclassOfGeneric(this Type type, Type openGenericType)
        {
            return IsOrSubclassOfGeneric(type, openGenericType, out var _);
        }

        public static bool IsSubclassOfGeneric(this Type type, Type openGenericType)
        {
            return IsSubclassOfGeneric(type, openGenericType, out var _);
        }

        public static bool IsSubclassOfGeneric(this Type type, Type openGenericType, out Type actualType)
        {
            return IsOrSubclassOfGeneric(type.BaseType, openGenericType, out actualType);
        }

        public static bool IsOrSubclassOfGeneric(this Type type, Type genericType, out Type actualType)
        {
            var openGenericType = GetTypeOrGenericTypeDefinition(genericType);
            
            while (type != typeof(object))
            {
                if (type == openGenericType ||
                    (type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType))
                {
                    actualType = type;
                    return true;
                }
                type = type.BaseType;
            }

            actualType = null;
            return false;
        }

        public static bool IsOrSubclassOf(this Type type, Type typeToCompare)
        {
            return type == typeToCompare || type.IsSubclassOf(typeToCompare);
        }


        public static bool IsSuperclassOf(this Type type, Type typeToCompare)
        {
            return typeToCompare.IsSubclassOf(type);
        }

        public static bool IsOrSuperclassOf(this Type type, Type typeToCompare)
        {
            return type == typeToCompare || typeToCompare.IsSubclassOf(type);
        }

        public static bool IsGenericSuperclassOf(this Type genericType, Type type)
        {
            return type.IsSubclassOfGeneric(GetTypeOrGenericTypeDefinition(genericType));
        }

        public static bool IsGenericOrGenericSuperclassOf(this Type genericType, Type type)
        {
            return type.IsOrSubclassOfGeneric(GetTypeOrGenericTypeDefinition(genericType));
        }

        public static Type GetTypeOrGenericTypeDefinition(this Type type)
        {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }
    }
}
