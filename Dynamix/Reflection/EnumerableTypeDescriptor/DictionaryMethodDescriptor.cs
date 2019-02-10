using System;

namespace Dynamix.Reflection
{
    public enum DictionaryMethod
    {
        Add,
        ContainsKey
    }

    public class DictionaryMethodDescriptor
    {
        public DictionaryMethodDescriptor(DictionaryMethod method, MethodInfoEx methodInfoEx, Type keyType, Type elementType)
        {
            Method = method;
            MethodInfoEx = methodInfoEx;
            KeyType = keyType;
            ElementType = elementType;
        }

        public DictionaryMethodDescriptor(DictionaryMethod method, MethodInfoEx methodInfoEx, int keyParameterIndex, int elementParameterIndex)
        {
            Method = method;
            MethodInfoEx = methodInfoEx;
            if (keyParameterIndex >= 0)
                KeyType = MethodInfoEx.MethodInfo.GetParameters()[keyParameterIndex].ParameterType;
            if (elementParameterIndex >= 0)
                ElementType = MethodInfoEx.MethodInfo.GetParameters()[elementParameterIndex].ParameterType;
        }

        public DictionaryMethod Method { get; }
        public MethodInfoEx MethodInfoEx { get; }
        public Type KeyType { get; }
        public Type ElementType { get; }
    }
}
