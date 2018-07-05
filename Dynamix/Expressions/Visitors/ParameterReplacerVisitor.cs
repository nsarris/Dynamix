using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.Expressions.Visitors
{
    public class ParameterReplacerVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression source;
        private readonly ParameterExpression target;

        public ParameterReplacerVisitor
              (ParameterExpression source, ParameterExpression target)
        {
            this.source = source;
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
                                 (p => p == source ? target : p));
            else
                return base.Visit(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
        }
    }
}
