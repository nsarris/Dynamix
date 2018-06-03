using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Dynamix.Reflection
{
    internal static class CustomAttributeBuilderFactory
    {
        public static CustomAttributeBuilder FromExpression(Expression<Func<Attribute>> builderExpression)
        {
            if (builderExpression.Body.NodeType == ExpressionType.New)
            {
                var constructionExpression = builderExpression.Body as NewExpression;
                var ci = constructionExpression.Constructor;
                var parameters = constructionExpression.Arguments
                    .Select(x => x.NodeType == ExpressionType.Constant
                    ? ((ConstantExpression)x).Value
                    : Expression.Lambda(x).Compile().DynamicInvoke())
                    .ToArray();
                var builder = new CustomAttributeBuilder(ci, parameters);
                return builder;
            }
            else if (builderExpression.Body.NodeType == ExpressionType.MemberInit)
            {
                var memberInitExpression = builderExpression.Body as MemberInitExpression;
                var ci = memberInitExpression.NewExpression.Constructor;
                var parameters = memberInitExpression.NewExpression.Arguments
                    .Select(x => x.NodeType == ExpressionType.Constant
                    ? ((ConstantExpression)x).Value
                    : Expression.Lambda(x).Compile().DynamicInvoke())
                    .ToArray();

                var namedProperties
                    = memberInitExpression.Bindings
                    .Where(x => x.BindingType == MemberBindingType.Assignment
                        && x.Member.MemberType == System.Reflection.MemberTypes.Property)
                    .Select(x => new
                    {
                        Property = (System.Reflection.PropertyInfo)x.Member,
                        Value = Expression.Lambda(((MemberAssignment)x).Expression, new ParameterExpression[0]).Compile().DynamicInvoke()
                    })
                    .ToList();

                var namedFields
                    = memberInitExpression.Bindings
                    .Where(x => x.BindingType == MemberBindingType.Assignment
                        && x.Member.MemberType == System.Reflection.MemberTypes.Field)
                    .Select(x => new
                    {
                        Field = (System.Reflection.FieldInfo)x.Member,
                        Value = Expression.Lambda(((MemberAssignment)x).Expression, new ParameterExpression[0]).Compile().DynamicInvoke()
                    })
                    .ToList();

                var builder = new CustomAttributeBuilder(ci, parameters,
                    namedProperties.Select(x => x.Property).ToArray(),
                    namedProperties.Select(x => x.Value).ToArray(),
                    namedFields.Select(x => x.Field).ToArray(),
                    namedFields.Select(x => x.Field).ToArray()
                    );
                return builder;
            }
            else
                throw new ArgumentException("Builder expression must be an attribute construction statement");
        }
    }
}
