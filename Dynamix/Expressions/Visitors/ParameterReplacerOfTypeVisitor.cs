using System;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.Expressions.Visitors
{
    public class ParameterReplacerOfTypeVisitor : ExpressionVisitor
    {
        private readonly Type sourceType;
        private readonly ParameterExpression target;

        public ParameterReplacerOfTypeVisitor
              (Type source, ParameterExpression target)
        {
            sourceType = source;
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
            {
                if (lambdaExpression.Parameters.Count(x => x.Type == sourceType) > 1)
                    throw new InvalidOperationException("Cannot replace a lambda parameter by type when there are more than one input parameters of the same type");

                return Expression.Lambda(
                    Visit(lambdaExpression.Body),
                    lambdaExpression.Parameters.Select
                                 (p => p.Type == sourceType ? target : p));
            }
            else
                return base.Visit(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Type == sourceType ? target : base.VisitParameter(node);
        }
    }
}

