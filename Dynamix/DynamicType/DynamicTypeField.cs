using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dynamix
{
    public sealed class DynamicTypeField
    {
        public string Name { get; internal set; }
        public Type Type { get; internal set; }
        public bool AsNullable { get; internal set; }
        public string CtorParameterName { get; internal set; }
        public object ConstructorDefaultValue { get; internal set; }
        public bool HasConstructorDefaultValue { get; internal set; }
        public bool InitializeInConstructor { get; internal set; }
        public MemberAccessModifier AccessModifier { get; internal set; } = MemberAccessModifier.Public;

        public IReadOnlyList<CustomAttributeBuilder> AttributeBuilders => attributeBuilders;

        private readonly List<CustomAttributeBuilder> attributeBuilders = new List<CustomAttributeBuilder>();

        internal DynamicTypeField(string name, Type type, IEnumerable<CustomAttributeBuilder> attributeBuilders = null)
        {
            Name = name;
            Type = type;

            if (attributeBuilders != null)
                this.attributeBuilders = attributeBuilders.ToList();
        }

        internal DynamicTypeField(FieldInfo fieldInfo)
        {
            Name = fieldInfo.Name;
            Type = fieldInfo.FieldType;
            attributeBuilders = fieldInfo.GetCustomAttributesData().Select(x => x.ToBuilder()).ToList();
        }

        internal void AddAttributeBuilder(CustomAttributeBuilder attributeBuilder)
        {
            attributeBuilders.Add(attributeBuilder);
        }

        internal DynamicTypeField Clone()
        {
            return new DynamicTypeField(Name, Type, attributeBuilders)
            {
                AsNullable = AsNullable,
                CtorParameterName = CtorParameterName,
                ConstructorDefaultValue = ConstructorDefaultValue,
                HasConstructorDefaultValue = HasConstructorDefaultValue,
                InitializeInConstructor = InitializeInConstructor,
                AccessModifier = AccessModifier,
            };
        }
    }
}
