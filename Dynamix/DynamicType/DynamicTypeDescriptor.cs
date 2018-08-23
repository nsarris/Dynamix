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
        public IReadOnlyList<Type> Interfaces => interfaces;
        public IReadOnlyList<DynamicTypeField> Fields => fields;
        public IReadOnlyList<DynamicTypeProperty> Properties => properties;
        public IReadOnlyList<CustomAttributeBuilder> AttributeBuilders => attributeBuilders;

        private readonly List<CustomAttributeBuilder> attributeBuilders;
        private readonly List<Type> interfaces;
        private readonly List<DynamicTypeField> fields;
        private readonly List<DynamicTypeProperty> properties;

        internal DynamicTypeDescriptor(string name = null, IEnumerable<DynamicTypeProperty> properties = null, IEnumerable<DynamicTypeField> fields = null, IEnumerable<Type> interfaces = null, Type baseType = null, IEnumerable<CustomAttributeBuilder> customAttributeBuilders = null)
        {
            Name = name;
            BaseType = baseType;

            this.properties = properties == null ?
                new List<DynamicTypeProperty>() :
                properties.ToList();

            this.fields = fields == null ?
                new List<DynamicTypeField>() :
                fields.ToList();

            this.interfaces = interfaces == null ?
                new List<Type>() :
                interfaces.ToList();

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

        internal void AddInterface(Type @interface)
        {
            interfaces.Add(@interface);
        }

        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Type Name cannot be null or whitespace");

            if (!Properties.Any() && !Fields.Any())
                throw new InvalidOperationException("Type must have at least one property or field");

            if (Properties.GroupBy(x => x.Name).Count() < Properties.Count())
                throw new InvalidOperationException("Duplicate property name found in type descriptor");

            if (Fields.GroupBy(x => x.Name).Count() < Fields.Count())
                throw new InvalidOperationException("Duplicate field name found in type descriptor");

            if (Properties.Select(x => x.Name).Intersect(Fields.Select(x => x.Name)).Any())
                throw new InvalidOperationException("Properties and Fields with the same name found in type descriptor");
        }

        internal DynamicTypeDescriptor Clone()
        {
            return new DynamicTypeDescriptor(Name, properties, fields, interfaces, BaseType, attributeBuilders);
        }
    }
}
