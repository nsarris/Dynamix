using Dynamix.Expressions;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.DynamicProjection
{
    internal abstract class ProjectionSource
    {
        internal abstract Expression GetExpression(ParameterExpression itParameter);
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
    }
}
