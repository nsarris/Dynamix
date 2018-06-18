using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public sealed class DynamicTypeDescriptor
    {
        public string Name { get; internal set; }
        public Type BaseType { get; internal set; }
        public IReadOnlyList<DynamicTypeField> Fields => fields;
        public IReadOnlyList<DynamicTypeProperty> Properties => properties;
        public IReadOnlyList<CustomAttributeBuilder> AttributeBuilders => attributeBuilders;

        private readonly List<CustomAttributeBuilder> attributeBuilders;
        private readonly List<DynamicTypeField> fields;
        private readonly List<DynamicTypeProperty> properties;

        internal DynamicTypeDescriptor(string name = null, IEnumerable<DynamicTypeProperty> properties = null, IEnumerable<DynamicTypeField> fields = null, Type baseType = null, IEnumerable<CustomAttributeBuilder> customAttributeBuilders = null)
        {
            Name = name;
            BaseType = baseType;

            this.properties = properties == null ?
                new List<DynamicTypeProperty>() :
                properties.ToList();

            this.fields = fields == null ?
                new List<DynamicTypeField>() :
                fields.ToList();

            this.attributeBuilders = customAttributeBuilders == null ?
                new List<CustomAttributeBuilder>() :
                customAttributeBuilders.ToList();
        }

        internal void AddCustomAttributeBuilder(CustomAttributeBuilder customAttributeBuilder)
        {
            attributeBuilders.Add(customAttributeBuilder);
        }

        internal void AddProperty(CustomAttributeBuilder customAttributeBuilder)
        {
            attributeBuilders.Add(customAttributeBuilder);
        }

        internal void AddProperty(DynamicTypeProperty property)
        {
            properties.Add(property);
        }

        internal void AddField(DynamicTypeField field)
        {
            fields.Add(field);
        }

        internal DynamicTypeDescriptor Clone()
        {
            return new DynamicTypeDescriptor(Name, properties, fields, BaseType, attributeBuilders);
        }
    }
}
