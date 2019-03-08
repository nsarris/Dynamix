using Dynamix.Reflection;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix
{
    public sealed class DynamicTypePropertiesBuilder
    {
        readonly DynamicTypeProperty property;

        public DynamicTypePropertiesBuilder(string name, Type type)
        {
            property = new DynamicTypeProperty(name, type);
        }

        public DynamicTypePropertiesBuilder(PropertyInfo propertyInfo)
        {
            property = new DynamicTypeProperty(propertyInfo);
        }

        public DynamicTypePropertiesBuilder WithType(Type type)
        {
            property.Type = type ?? throw new ArgumentNullException(nameof(type));
            if (property.AsNullable) property.Type = property.Type.ToNullable();
            return this;
        }

        public DynamicTypePropertiesBuilder WithType<T>()
        {
            return WithType(typeof(T));
        }

        public DynamicTypePropertiesBuilder WithAttribute(Expression<Func<Attribute>> builderExpression)
        {
            property.AddAttributeBuilder(CustomAttributeBuilderFactory.FromExpression(builderExpression));
            return this;
        }

        public DynamicTypePropertiesBuilder AreInitializedInConstructor()
        {
            property.InitializeInConstructor = true;
            property.CtorParameterName = property.CtorParameterName ?? property.Name.ToCamelCase();
            property.HasConstructorDefaultValue = false;
            return this;
        }

        public DynamicTypePropertiesBuilder AreInitializedInConstructorOptional()
        {
            property.InitializeInConstructor = true;
            property.CtorParameterName = property.CtorParameterName ?? property.Name.ToCamelCase();
            property.ConstructorDefaultValue = property.Type.DefaultOf();
            property.HasConstructorDefaultValue = true;
            return this;
        }

        public DynamicTypePropertiesBuilder AreInitializedInConstructorOptional(object defaultValue)
        {
            if ((defaultValue == null && property.Type.IsValueType && !property.Type.IsNullable())
                || defaultValue != null && property.Type != defaultValue.GetType())
                throw new InvalidOperationException($"Incompatible default value for member {property.Name}");

            property.CtorParameterName = property.CtorParameterName ?? property.Name.ToCamelCase();
            property.InitializeInConstructor = true;
            property.ConstructorDefaultValue = defaultValue;
            property.HasConstructorDefaultValue = true;
            return this;
        }

        public DynamicTypePropertiesBuilder HaveGetter(GetSetAccessModifier accessModifier)
        {
            property.GetAccessModifier = accessModifier;
            return this;
        }

        public DynamicTypePropertiesBuilder HaveSetter(GetSetAccessModifier accessModifier)
        {
            property.SetAccessModifier = accessModifier;
            return this;
        }

        public DynamicTypePropertiesBuilder AsNullable(bool nullable = true)
        {
            property.AsNullable = nullable;
            if (property.AsNullable)
                property.Type = property.Type.ToNullable();
            return this;
        }

        public DynamicTypeProperty Build()
        {
            return property.Clone();
        }
    }
}
