using Dynamix.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.DynamicProjection
{
    internal interface IProjectionSource
    {

    }

    internal class StringProjectionSource : IProjectionSource
    {
        internal string SourceExpression { get; }
        internal StringProjectionSource(string sourceExpression)
        {
            SourceExpression = sourceExpression;
        }
    }

    internal class ExpressionProjectionSource : IProjectionSource
    {
        internal Expression SourceExpression { get; }
        internal ExpressionProjectionSource(Expression sourceExpression)
        {
            SourceExpression = sourceExpression;
        }
    }

    internal class LambdaExpressionProjectionSource : IProjectionSource
    {
        internal LambdaExpression SourceExpression { get; }
        internal LambdaExpressionProjectionSource(LambdaExpression sourceExpression)
        {
            SourceExpression = sourceExpression;
        }
    }

    internal class ConstantProjectionSource : IProjectionSource
    {
        public object Value { get; }
        internal ConstantProjectionSource(object value)
        {
            Value = value;
        }
    }
}
