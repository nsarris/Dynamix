using System;

namespace Dynamix.Reflection
{
    public enum ListMethod
    {
        Add,
        Insert,
        Remove,
        RemoveAt,
        Contains
    }

    public class ListMethodDescriptor
    {
        public ListMethodDescriptor(ListMethod method, MethodInfoEx methodInfoEx, Type elementType)
        {
            Method = method;
            MethodInfoEx = methodInfoEx;
            ElementType = elementType;
        }

        public ListMethodDescriptor(ListMethod method, MethodInfoEx methodInfoEx, int parameterIndex)
        {
            Method = method;
            MethodInfoEx = methodInfoEx;
            ElementType = MethodInfoEx.MethodInfo.GetParameters()[parameterIndex].ParameterType;
        }

        public ListMethod Method { get; }
        public MethodInfoEx MethodInfoEx { get; }
        public Type ElementType { get; }
    }
}
