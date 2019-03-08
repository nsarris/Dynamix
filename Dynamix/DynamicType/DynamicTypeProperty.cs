using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dynamix
{

    public sealed class DynamicTypeProperty
    {
        public string Name { get; internal set; }
        public Type Type { get; internal set; }
        public bool AsNullable { get; internal set; }
        public string CtorParameterName { get; internal set; }
        public object ConstructorDefaultValue { get; internal set; }
        public bool HasConstructorDefaultValue { get; internal set; }
        public bool InitializeInConstructor { get; internal set; }
        public GetSetAccessModifier GetAccessModifier { get; internal set; } = GetSetAccessModifier.Public;
        public GetSetAccessModifier SetAccessModifier { get; internal set; } = GetSetAccessModifier.Public;
        public IReadOnlyList<CustomAttributeBuilder> AttributeBuilders => attributeBuilders;

        private readonly List<CustomAttributeBuilder> attributeBuilders = new List<CustomAttributeBuilder>();

        internal DynamicTypeProperty(PropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            Type = propertyInfo.PropertyType;
            GetAccessModifier = GetMethodAccessModifier(propertyInfo.GetMethod);
            SetAccessModifier = GetMethodAccessModifier(propertyInfo.SetMethod);
            attributeBuilders = propertyInfo.GetCustomAttributesData().Select(x => x.ToBuilder()).ToList();
        }

        internal DynamicTypeProperty(string name, Type type, IEnumerable<CustomAttributeBuilder> attributeBuilders = null)
        {
            Name = name;
            Type = type;

            if (attributeBuilders != null)
                this.attributeBuilders = attributeBuilders.ToList();
        }

        internal void AddAttributeBuilder(CustomAttributeBuilder attributeBuilder)
        {
            attributeBuilders.Add(attributeBuilder);
        }

        internal DynamicTypeProperty Clone()
        {
            return new DynamicTypeProperty(Name, Type, attributeBuilders)
            {
                AsNullable = AsNullable,
                CtorParameterName = CtorParameterName,
                ConstructorDefaultValue = ConstructorDefaultValue,
                HasConstructorDefaultValue = HasConstructorDefaultValue,
                InitializeInConstructor = InitializeInConstructor,
                GetAccessModifier = GetAccessModifier,
                SetAccessModifier = SetAccessModifier
            };
        }

        private static GetSetAccessModifier GetMethodAccessModifier(MethodInfo methodInfo)
        {
            if (methodInfo is null)
                return GetSetAccessModifier.None;
            if (methodInfo.IsPrivate)
                return GetSetAccessModifier.Private;
            if (methodInfo.IsFamily)
                return GetSetAccessModifier.Protected;
            if (methodInfo.IsAssembly)
                return GetSetAccessModifier.Internal;
            if (methodInfo.IsPublic)
                return GetSetAccessModifier.Public;

            throw new ArgumentException("Did not find access modifier", "methodInfo");
        }
    }
}
