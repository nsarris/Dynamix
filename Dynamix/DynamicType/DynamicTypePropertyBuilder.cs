using Dynamix.Reflection;
using System;
using System.Linq.Expressions;

namespace Dynamix
{
    public sealed class DynamicTypePropertyBuilder
    {
        readonly DynamicTypeProperty property;

        public DynamicTypePropertyBuilder(string name, Type type)
        {
            property = new DynamicTypeProperty(name, type);
        }

        public DynamicTypePropertyBuilder HasName(string name)
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));

            property.Name = name;
            return this;
        }

        public DynamicTypePropertyBuilder HasType(Type type)
        {
            property.Type = type ?? throw new ArgumentNullException(nameof(type));
            if (property.AsNullable) property.Type = property.Type.ToNullable();
            return this;
        }

        public DynamicTypePropertyBuilder HasType<T>()
        {
            return HasType(typeof(T));
        }

        public DynamicTypePropertyBuilder HasAttribute(Expression<Func<Attribute>> builderExpression)
        {
            property.AddAttributeBuilder(CustomAttributeBuilderFactory.FromExpression(builderExpression));
            return this;
        }

        public DynamicTypePropertyBuilder IsInitializedInConstructor(string parameterName = null)
        {
            property.InitializeInConstructor = true;
            property.CtorParameterName = property.CtorParameterName ?? property.Name.ToCamelCase();
            property.HasConstructorDefaultValue = false;
            return this;
        }

        public DynamicTypePropertyBuilder IsInitializedInConstructorOptional(string parameterName = null)
        {
            property.InitializeInConstructor = true;
            property.CtorParameterName = property.CtorParameterName ?? property.Name.ToCamelCase();
            property.ConstructorDefaultValue = property.Type.DefaultOf();
            property.HasConstructorDefaultValue = true;
            return this;
        }

        public DynamicTypePropertyBuilder IsInitializedInConstructorOptional(object defaultValue, string parameterName = null)
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

        public DynamicTypePropertyBuilder HasGetter(GetSetAccessModifier accessModifier)
        {
            property.GetAccessModifier = accessModifier;
            return this;
        }

        public DynamicTypePropertyBuilder HasSetter(GetSetAccessModifier accessModifier)
        {
            property.SetAccessModifier = accessModifier;
            return this;
        }

        public DynamicTypePropertyBuilder AsNullable(bool nullable = true)
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
