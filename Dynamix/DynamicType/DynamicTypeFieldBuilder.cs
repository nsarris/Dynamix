using Dynamix.Reflection;
using System;
using System.Linq.Expressions;

namespace Dynamix
{
    public sealed class DynamicTypeFieldBuilder
    {
        private readonly DynamicTypeField field;

        public DynamicTypeFieldBuilder(string name, Type type)
        {
            field = new DynamicTypeField(name, type);
        }

        public DynamicTypeFieldBuilder HasName(string name)
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));

            field.Name = name;
            return this;
        }

        public DynamicTypeFieldBuilder HasType(Type type)
        {
            field.Type = type ?? throw new ArgumentNullException(nameof(type));
            if (field.AsNullable) field.Type = field.Type.ToNullable();
            return this;
        }

        public DynamicTypeFieldBuilder HasType<T>()
        {
            return HasType(typeof(T));
        }

        public DynamicTypeFieldBuilder HasAttribute(Expression<Func<Attribute>> builderExpression)
        {
            field.AddAttributeBuilder(CustomAttributeBuilderFactory.FromExpression(builderExpression));
            return this;
        }

        public DynamicTypeFieldBuilder IsInitializedInConstructor(string parameterName = null)
        {
            field.InitializeInConstructor = true;
            field.CtorParameterName = field.CtorParameterName ?? field.Name.ToCamelCase();
            field.HasConstructorDefaultValue = false;
            return this;
        }

        public DynamicTypeFieldBuilder IsInitializedInConstructorOptional(string parameterName = null)
        {
            field.InitializeInConstructor = true;
            field.CtorParameterName = field.CtorParameterName ?? field.Name.ToCamelCase();
            field.ConstructorDefaultValue = field.Type.DefaultOf();
            field.HasConstructorDefaultValue = true;
            return this;
        }

        public DynamicTypeFieldBuilder IsInitializedInConstructorOptional(object defaultValue, string parameterName = null)
        {
            if ((defaultValue == null && field.Type.IsValueType && !field.Type.IsNullable())
                || defaultValue != null && field.Type != defaultValue.GetType())
                throw new InvalidOperationException($"Incompatible default value for member {field.Name}");

            field.CtorParameterName = field.CtorParameterName ?? field.Name.ToCamelCase();
            field.InitializeInConstructor = true;
            field.ConstructorDefaultValue = defaultValue;
            field.HasConstructorDefaultValue = true;
            return this;
        }

        public DynamicTypeFieldBuilder HasAccessModifier(MemberAccessModifier accessModifier)
        {
            field.AccessModifier = accessModifier;
            return this;
        }

        public DynamicTypeFieldBuilder AsNullable(bool nullable = true)
        {
            field.AsNullable = nullable;
            if (field.AsNullable)
                field.Type = field.Type.ToNullable();
            return this;
        }

        public DynamicTypeField Build()
        {
            return field.Clone();
        }
    }
}
