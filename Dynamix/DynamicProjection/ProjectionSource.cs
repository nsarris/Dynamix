using Dynamix.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.DynamicProjection
{
    internal abstract class ProjectionSource
    {
        internal abstract Expression GetExpression(ParameterExpression itParameter);
        internal abstract Type GetExpressionType();
    }

    internal class StringProjectionSource : ProjectionSource
    {
        internal string SourceExpression { get; }
        internal StringProjectionSource(string sourceExpression)
        {
            SourceExpression = sourceExpression;
        }

        internal override Expression GetExpression(ParameterExpression itParameter)
        {
            return MemberExpressionBuilder.GetExpression(itParameter, SourceExpression);
        }

        internal override Type GetExpressionType()
        {
            return null;
        }
    }

    internal class ExpressionProjectionSource : ProjectionSource
    {
        internal Expression SourceExpression { get; }
        internal ExpressionProjectionSource(Expression sourceExpression)
        {
            SourceExpression = sourceExpression;
        }

        internal override Expression GetExpression(ParameterExpression itParameter)
        {
            return LambdaParameterReplacer.ReplaceOfType(SourceExpression, itParameter.Type, itParameter);
        }

        internal override Type GetExpressionType()
        {
            return SourceExpression.Type;
        }
    }

    internal class LambdaExpressionProjectionSource : ProjectionSource
    {
        internal LambdaExpression SourceExpression { get; }
        internal LambdaExpressionProjectionSource(LambdaExpression sourceExpression)
        {
            SourceExpression = sourceExpression;
        }

        internal override Expression GetExpression(ParameterExpression itParameter)
        {
            return LambdaParameterReplacer.Replace(SourceExpression, SourceExpression.Parameters.First(), itParameter);
        }

        internal override Type GetExpressionType()
        {
            return SourceExpression.Type;
        }
    }

    internal class ConstantProjectionSource : ProjectionSource
    {
        public object Value { get; }
        internal ConstantProjectionSource(object value)
        {
            Value = value;
        }

        internal override Expression GetExpression(ParameterExpression itParameter)
        {
            return Expression.Constant(Value);
        }

        internal override Type GetExpressionType()
        {
            return Value?.GetType();
        }
    }
}
