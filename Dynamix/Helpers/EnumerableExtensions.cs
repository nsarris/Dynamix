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
    public static class EnumerableExtensions
    {
        internal static class Methods
        {
            internal static readonly MethodInfo Any = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == nameof(Enumerable.Any) && x.GetParameters().Length == 1);
            internal static readonly MethodInfo Cast = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast));
            internal static readonly MethodInfo Contains = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == nameof(Enumerable.Contains) && x.GetParameters().Length == 2);
            internal static readonly MethodInfo Count = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == nameof(Enumerable.Count) && x.GetParameters().Length == 1);
            internal static readonly MethodInfo First = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == nameof(Enumerable.First) && x.GetParameters().Length == 1);
            internal static readonly MethodInfo ToList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList));
            internal static readonly MethodInfo ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));
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

        public static object DynamicFirst(this IEnumerable enumerable, Type elementType = null)
        {
            enumerable = enumerable.Prepare(ref elementType);

            return Methods.First.MakeGenericMethodCached(elementType).Invoke(null, new object[] { enumerable });
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

        public static IEnumerable DynamicToArray(this IEnumerable enumerable, Type elementType = null)
        {
            enumerable = enumerable.Prepare(ref elementType);

            return (IEnumerable)Methods.ToArray.MakeGenericMethodCached(elementType).Invoke(null, new object[] { enumerable });
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

        #region Cloning

        public static IEnumerable Clone(this IEnumerable enumerable)
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            return CloneTo(enumerable, enumerable.GetType());
        }

        public static IEnumerable CloneTo(this IEnumerable enumerable, Type typeToBuild)
        {
            if (typeToBuild is null)
                throw new ArgumentNullException(nameof(typeToBuild));

            var enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(typeToBuild);

            if (!enumerableTypeDescriptor.IsEnumerable)
                throw new ArgumentException(nameof(typeToBuild), $"Type {typeToBuild.Name} is not an IEnumerable type.");

            if (enumerable is null)
            {
                if (typeToBuild.IsArrayOrArrayClass())
                    return Array.CreateInstance(enumerableTypeDescriptor.ExplicitImplementation.ElementType, 0);

                if (enumerableTypeDescriptor.ParameterlessConstructor is null)
                    throw new InvalidOperationException($"Cannot create enumerable of type {typeToBuild.Name}. The type does not provide a parameterless constructor.");

                return (IEnumerable)enumerableTypeDescriptor.ParameterlessConstructor.Invoke();
            }

            if (typeToBuild.IsArrayOrArrayClass())
                return enumerable.DynamicToArray(enumerableTypeDescriptor.ExplicitImplementation.ElementType);

            var ctor = enumerableTypeDescriptor.ConstructorsWithEnumerableParameter
                .Where(x => x.Key.IsInstanceOfType(enumerable))
                .Select(x => x.Value)
                .FirstOrDefault();

            if (ctor != null)
                return (IEnumerable)ctor.Invoke(enumerable);

            if (enumerableTypeDescriptor.ParameterlessConstructor == null)
                throw new InvalidOperationException($"Cannot create enumerable of type {typeToBuild.Name} from data of type {enumerable.GetType().Name}. The type does not provide a proper constructor with a single parameter for data, nor a parameterless constructor.");

            var instance = (IEnumerable)enumerableTypeDescriptor.ParameterlessConstructor.Invoke();

            if (enumerableTypeDescriptor.ExplicitImplementation.Kind == EnumerableInterfaceKind.Dictionary)
            {
                foreach (var item in enumerable)
                {
                    var entry = GetKeyValuePair(item);

                    var addMethod = enumerableTypeDescriptor.GetApplicableDictionaryMethod(DictionaryMethod.Add, entry);
                    if (addMethod == null)
                        throw new InvalidOperationException($"Cannot add element of type {item?.GetType()} to dictionary of type {instance.GetType()}. No applicable Add method was found on the type");

                    addMethod.MethodInfoEx.Invoke(instance, entry.key, entry.value);
                }
            }
            else
            {
                var index = 0;
                foreach (var item in enumerable)
                {
                    var applicableMethod =
                        enumerableTypeDescriptor.GetApplicableListMethod(ListMethod.Add, item) ??
                        enumerableTypeDescriptor.GetApplicableListMethod(ListMethod.Insert, item);
                    if (applicableMethod is null)
                        throw new InvalidOperationException($"Cannot add element of type {item?.GetType()} to collection of type {instance.GetType()}. No applicable Add or Insert methods were found on the type");
                    else
                    {
                        try
                        {
                            if (applicableMethod.Method == ListMethod.Add)
                                applicableMethod.MethodInfoEx.Invoke(instance, item);
                            else
                                applicableMethod.MethodInfoEx.Invoke(instance, index, item);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException($"Cannot add element of type {item?.GetType()} to collection of type {instance.GetType()}. No applicable Add or Insert method was found on the type or the applicable method failed (see inner exception).", e);
                        }
                    }
                    index++;
                }
            }

            return instance;
        }

        private static (object key, object value) GetKeyValuePair(object entry)
        {
            if (entry is null)
                throw new ArgumentNullException(nameof(entry), "Null cannot be a valid dictionary entry");

            var keyProp = entry.GetType().GetPropertyEx("Key");
            var valueProp = entry.GetType().GetPropertyEx("Value");
            if (keyProp == null || valueProp == null)
                throw new ArgumentException("Object is not a valid dictionary entry");

            return (keyProp.Get(entry), keyProp.Get(entry));
        }

        #endregion
    }
}
