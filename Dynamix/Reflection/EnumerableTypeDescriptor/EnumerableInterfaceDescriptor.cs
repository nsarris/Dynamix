using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dynamix.Reflection
{
    public class EnumerableInterfaceDescriptor
    {
        internal int GenericArgumentHashCode { get; }

        public Type Type { get; }
        public EnumerableInterfaceKind Kind { get; }
        public bool IsReadOnly { get; }
        public bool HasGenericParameters { get; }

        public Type ElementType { get; }
        public Type DictionaryKeyType { get; }
        public Type DictionaryElementType { get; }

        internal EnumerableInterfaceDescriptor(Type interfaceType, SupportedEnumerableTypes enumerableType)
        {
            Type = interfaceType;
            Kind = enumerableType.EnumerableInterfaceKind;
            IsReadOnly = enumerableType.IsReadOnly;

            if (enumerableType.EnumerableInterfaceKind != EnumerableInterfaceKind.Dictionary)
            {
                ElementType = interfaceType.IsGenericType ? 
                    interfaceType.GenericTypeArguments.Single() : typeof(object);

                HasGenericParameters = ElementType.IsGenericParameter;
            }
            else
            {
                if (interfaceType.IsGenericType)
                {
                    ElementType = typeof(KeyValuePair<,>).MakeGenericTypeCached(interfaceType.GenericTypeArguments);
                    DictionaryKeyType = interfaceType.GenericTypeArguments[0];
                    DictionaryElementType = interfaceType.GenericTypeArguments[1];
                    HasGenericParameters = DictionaryKeyType.IsGenericParameter || DictionaryElementType.IsGenericParameter;
                }
                else
                {
                    ElementType = typeof(DictionaryEntry);
                    DictionaryKeyType = typeof(object);
                    DictionaryElementType = typeof(object);
                }
            }

            GenericArgumentHashCode = interfaceType.GenericTypeArguments
                .Flatten(a => a.GenericTypeArguments)
                .Where(a => !a.IsGenericType)
                .Sum(a => a.GetHashCode());
        }

        private EnumerableInterfaceDescriptor(Type type, EnumerableInterfaceKind kind, bool isReadOnly, Type elementType)
        {
            Type = type;
            Kind = kind;
            IsReadOnly = isReadOnly;
            ElementType = elementType;
            HasGenericParameters = ElementType.IsGenericParameter;
        }

        internal static EnumerableInterfaceDescriptor GetArray(Type arrayType)
        {
            return new EnumerableInterfaceDescriptor(
                typeof(IList<>).MakeGenericType(arrayType.GetElementType()),
                EnumerableInterfaceKind.List,
                false,
                arrayType.GetElementType()
                );
        }
    }
}
