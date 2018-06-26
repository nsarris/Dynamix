using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix
{
    public sealed class DynamicTypeDescriptorBuilder
    {
        private readonly DynamicTypeDescriptor dynamicTypeDescriptor;

        public DynamicTypeDescriptorBuilder(string name = null, IEnumerable<DynamicTypeProperty> properties = null, IEnumerable<DynamicTypeField> fields = null, IEnumerable<Type> interfaces = null, Type baseType = null)
        {
            dynamicTypeDescriptor = new DynamicTypeDescriptor(name, properties, fields, interfaces, baseType);
        }

        
        public DynamicTypeDescriptorBuilder AddProperty(DynamicTypeProperty property)
        {
            dynamicTypeDescriptor.AddProperty(property);
            return this;
        }

        public DynamicTypeDescriptorBuilder AddProperty(string name, Type type, Func<DynamicTypePropertyBuilder, DynamicTypePropertyBuilder> configuration = null)
        {
            var builder = new DynamicTypePropertyBuilder(name, type);
            if (configuration != null) builder = configuration(builder);
            AddProperty(builder.Build());
            return this;
        }

        public DynamicTypeDescriptorBuilder AddProperty<T>(string name, Func<DynamicTypePropertyBuilder, DynamicTypePropertyBuilder> configuration = null)
        {
            return AddProperty(name, typeof(T), configuration);
        }

        public DynamicTypeDescriptorBuilder AddPropertyFromTemplate<T>(Expression<Func<T, object>> templateExpression, Func<DynamicTypePropertyBuilder, DynamicTypePropertyBuilder> configuration = null)
        {
            var prop = ReflectionHelper.GetProperty(templateExpression);
            return AddProperty(prop.Name, prop.PropertyType, configuration);
        }


        public DynamicTypeDescriptorBuilder AddField(DynamicTypeField field)
        {
            dynamicTypeDescriptor.AddField(field);
            return this;
        }

        public DynamicTypeDescriptorBuilder AddField(string name, Type type, Func<DynamicTypeFieldBuilder, DynamicTypeFieldBuilder> configuration = null)
        {
            var builder = new DynamicTypeFieldBuilder(name, type);
            if (configuration != null) builder = configuration(builder);
            AddField(builder.Build());
            return this;
        }

        public DynamicTypeDescriptorBuilder AddField<T>(string name, Func<DynamicTypeFieldBuilder, DynamicTypeFieldBuilder> configuration = null)
        {
            return AddField(name, typeof(T), configuration);
        }

        public DynamicTypeDescriptorBuilder AddFieldFromTemplate<T>(Expression<Func<T, object>> templateExpression, Func<DynamicTypeFieldBuilder, DynamicTypeFieldBuilder> configuration = null)
        {
            var prop = ReflectionHelper.GetProperty(templateExpression);
            return AddField(prop.Name, prop.PropertyType, configuration);
        }


        public DynamicTypeDescriptorBuilder HasBaseType(Type baseType)
        {
            dynamicTypeDescriptor.BaseType = baseType;
            return this;
        }

        public DynamicTypeDescriptorBuilder HasBaseType<T>()
        {
            dynamicTypeDescriptor.BaseType = typeof(T);
            return this;
        }

        public DynamicTypeDescriptorBuilder HasName(string name)
        {
            dynamicTypeDescriptor.Name = name;
            return this;
        }


        public DynamicTypeDescriptorBuilder HasAttribute(Expression<Func<Attribute>> builderExpression)
        {
            dynamicTypeDescriptor.AddCustomAttributeBuilder(CustomAttributeBuilderFactory.FromExpression(builderExpression));
            return this;
        }


        public DynamicTypeDescriptorBuilder ImplementsInterfaces(params Type[] interfaceTypes)
        {
            return ImplementsInterfaces(interfaceTypes.AsEnumerable());
        }
        public DynamicTypeDescriptorBuilder ImplementsInterfaces(IEnumerable<Type> interfaceTypes)
        {
            foreach (var t in interfaceTypes)
                ImplementsInterface(t);
            return this;
        }
        public DynamicTypeDescriptorBuilder ImplementsInterface(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException($"Type {interfaceType.Name} is not an interface");

            dynamicTypeDescriptor.AddInterface(interfaceType);

            return this;
        }

        public DynamicTypeDescriptor Build()
        {
            dynamicTypeDescriptor.Validate();
            return dynamicTypeDescriptor.Clone();
        }
    }
}
