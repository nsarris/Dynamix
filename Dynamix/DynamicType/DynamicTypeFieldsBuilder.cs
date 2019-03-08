using Dynamix.Reflection;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix
{
    public sealed class DynamicTypeFieldsBuilder
    {
        private readonly DynamicTypeField field;

        public DynamicTypeFieldsBuilder(string name, Type type)
        {
            field = new DynamicTypeField(name, type);
        }

        public DynamicTypeFieldsBuilder(FieldInfo fieldInfo)
        {
            field = new DynamicTypeField(fieldInfo);
        }

        public DynamicTypeFieldsBuilder WithType(Type type)
        {
            field.Type = type ?? throw new ArgumentNullException(nameof(type));
            if (field.AsNullable) field.Type = field.Type.ToNullable();
            return this;
        }

        public DynamicTypeFieldsBuilder WithType<T>()
        {
            return WithType(typeof(T));
        }

        public DynamicTypeFieldsBuilder WithAttribute(Expression<Func<Attribute>> builderExpression)
        {
            field.AddAttributeBuilder(CustomAttributeBuilderFactory.FromExpression(builderExpression));
            return this;
        }

        public DynamicTypeFieldsBuilder AreInitializedInConstructor(string parameterName = null)
        {
            field.InitializeInConstructor = true;
            field.CtorParameterName = parameterName ?? field.CtorParameterName ?? field.Name.ToCamelCase();
            field.HasConstructorDefaultValue = false;
            return this;
        }

        public DynamicTypeFieldsBuilder AreInitializedInConstructorOptional(string parameterName = null)
        {
            field.InitializeInConstructor = true;
            field.CtorParameterName = parameterName ?? field.CtorParameterName ?? field.Name.ToCamelCase();
            field.ConstructorDefaultValue = field.Type.DefaultOf();
            field.HasConstructorDefaultValue = true;
            return this;
        }

        public DynamicTypeFieldsBuilder AreInitializedInConstructorOptional(object defaultValue, string parameterName = null)
        {
            if ((defaultValue == null && field.Type.IsValueType && !field.Type.IsNullable())
                || defaultValue != null && field.Type != defaultValue.GetType())
                throw new InvalidOperationException($"Incompatible default value for member {field.Name}");

            field.CtorParameterName = parameterName ?? field.CtorParameterName ?? field.Name.ToCamelCase();
            field.InitializeInConstructor = true;
            field.ConstructorDefaultValue = defaultValue;
            field.HasConstructorDefaultValue = true;
            return this;
        }

        public DynamicTypeFieldsBuilder WithAccessModifier(MemberAccessModifier accessModifier)
        {
            field.AccessModifier = accessModifier;
            return this;
        }

        public DynamicTypeFieldsBuilder AsNullable(bool nullable = true)
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
