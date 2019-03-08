using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix
{
    public sealed class DynamicTypeDescriptorBuilder
    {
        private readonly DynamicTypeDescriptor dynamicTypeDescriptor;

        public DynamicTypeDescriptorBuilder(string name = null, IEnumerable<DynamicTypeProperty> properties = null, IEnumerable<DynamicTypeField> fields = null, IEnumerable<Type> interfaces = null, Type baseType = null)
        {
            dynamicTypeDescriptor = new DynamicTypeDescriptor(name, properties, fields, interfaces, baseType);
        }


        public DynamicTypeDescriptorBuilder AddProperty(DynamicTypeProperty property, Action<DynamicTypePropertyBuilder> configuration = null)
        {
            var builder = new DynamicTypePropertyBuilder(property);
            configuration?.Invoke(builder);
            dynamicTypeDescriptor.AddProperty(builder.Build());
            return this;
        }

        public DynamicTypeDescriptorBuilder AddProperty(PropertyInfo propertyInfo, Action<DynamicTypePropertyBuilder> configuration = null)
        {
            return AddProperty(new DynamicTypeProperty(propertyInfo), configuration);
        }

        public DynamicTypeDescriptorBuilder AddProperty(string name, Type type, Action<DynamicTypePropertyBuilder> configuration = null)
        {
            return AddProperty(new DynamicTypeProperty(name, type), configuration);
        }

        public DynamicTypeDescriptorBuilder AddProperty<T>(string name, Action<DynamicTypePropertyBuilder> configuration = null)
        {
            return AddProperty(name, typeof(T), configuration);
        }

        public DynamicTypeDescriptorBuilder AddPropertyFromTemplate<T>(Expression<Func<T, object>> templateExpression, Action<DynamicTypePropertyBuilder> configuration = null)
        {
            var prop = ReflectionHelper.GetProperty(templateExpression);
            return AddProperty(prop.Name, prop.PropertyType, configuration);
        }

        public DynamicTypeDescriptorBuilder AddProperties(IEnumerable<PropertyInfo> properties, Action<DynamicTypePropertiesBuilder> configuration = null)
        {
            foreach (var prop in properties)
            {
                var builder = new DynamicTypePropertiesBuilder(prop);
                configuration?.Invoke(builder);
                AddProperty(builder.Build());
            }

            return this;
        }

        public DynamicTypeDescriptorBuilder AddPropertiesFromType(Type templateType, Action<DynamicTypePropertiesBuilder> configuration = null)
        {
            return AddProperties(templateType.GetProperties(), configuration);
        }

        public DynamicTypeDescriptorBuilder AddPropertiesFromType<T>(Action<DynamicTypePropertiesBuilder> configuration = null)
        {
            return AddPropertiesFromType(typeof(T), configuration);
        }

        public DynamicTypeDescriptorBuilder AddField(DynamicTypeField field, Action<DynamicTypeFieldBuilder> configuration = null)
        {
            var builder = new DynamicTypeFieldBuilder(field);
            configuration?.Invoke(builder);
            dynamicTypeDescriptor.AddField(builder.Build());
            return this;
        }

        public DynamicTypeDescriptorBuilder AddField(FieldInfo fieldInfo, Action<DynamicTypeFieldBuilder> configuration = null)
        {
            return AddField(new DynamicTypeField(fieldInfo), configuration);
        }

        public DynamicTypeDescriptorBuilder AddField(string name, Type type, Action<DynamicTypeFieldBuilder> configuration = null)
        {
            return AddField(new DynamicTypeField(name, type), configuration);
        }

        public DynamicTypeDescriptorBuilder AddField<T>(string name, Action<DynamicTypeFieldBuilder> configuration = null)
        {
            return AddField(name, typeof(T), configuration);
        }

        public DynamicTypeDescriptorBuilder AddFieldFromTemplate<T>(Expression<Func<T, object>> templateExpression, Action<DynamicTypeFieldBuilder> configuration = null)
        {
            var prop = ReflectionHelper.GetProperty(templateExpression);
            return AddField(prop.Name, prop.PropertyType, configuration);
        }

        public DynamicTypeDescriptorBuilder AddFieldsFromType(IEnumerable<FieldInfo> fields, Action<DynamicTypeFieldsBuilder> configuration = null)
        {
            foreach (var field in fields)
            {
                var builder = new DynamicTypeFieldsBuilder(field);
                configuration?.Invoke(builder);
                AddField(builder.Build());
            }

            return this;
        }

        public DynamicTypeDescriptorBuilder AddFieldsFromType(Type templateType, Action<DynamicTypeFieldsBuilder> configuration = null)
        {
            return AddFieldsFromType(templateType.GetFields(), configuration);
        }

        public DynamicTypeDescriptorBuilder AddFieldsFromType<T>(Action<DynamicTypeFieldsBuilder> configuration = null)
        {
            return AddFieldsFromType(typeof(T), configuration);
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
