using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.Expressions
{
    public class ParameterReplacerOfNameVisitor : ExpressionVisitor
    {
        private readonly string parameterName;
        private readonly ParameterExpression target;

        public ParameterReplacerOfNameVisitor
              (string parameterName, ParameterExpression target)
        {
            this.parameterName = parameterName;
            this.target = target;
        }

        public Expression<T> Visit<T>(Expression<T> root)
        {
            return (Expression<T>)Visit((Expression)root);
        }

        public LambdaExpression Visit(LambdaExpression root)
        {
            return (LambdaExpression)Visit((Expression)root);
        }

        public override Expression Visit(Expression node)
        {
            if (node is LambdaExpression lambdaExpression)
                return Expression.Lambda(
                    Visit(lambdaExpression.Body),
                    lambdaExpression.Parameters.Select
                                 (p => p.Name == parameterName ? target : p));
            else
                return base.Visit(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Name == parameterName ? target : base.VisitParameter(node);
        }
    }
}

