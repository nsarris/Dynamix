using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public class EpxressionParameterReplacer
    {
        // Produces an expression identical to 'expression'
        // except with 'source' parameter replaced with 'target' parameter.     
        public static Expression<T> Replace<T>
                     (Expression<T> expression,
                      string source,
                      ParameterExpression target)
        {
            return new ParameterReplacerVisitor(source, target)
                      .VisitAndConvert<T>(expression);
        }

        public static LambdaExpression Replace
             (LambdaExpression expression,
              string source,
              ParameterExpression target)
        {
            return (LambdaExpression)(new ParameterReplacerVisitor(source, target)
                      .VisitAndConvert(expression));
        }

        public static LambdaExpression ReplaceType
             (LambdaExpression expression,
              string sourceName,
              Type targetType)
        {
            foreach (var p in expression.Parameters)
            {
                if (p.Name == sourceName)
                    expression = (LambdaExpression)
                        (new ParameterReplacerVisitor(p.Name, Expression.Parameter(targetType))
                            .VisitAndConvert(expression));
            }
            return expression;
        }


        private class ParameterReplacerVisitor : ExpressionVisitor
        {
            private readonly string _source;
            private readonly ParameterExpression _target;

            public ParameterReplacerVisitor
                  (string source, ParameterExpression target)
            {
                _source = source;
                _target = target;
            }

            internal Expression<T> VisitAndConvert<T>(Expression<T> root)
            {
                return (Expression<T>)VisitLambda(root);
            }

            internal Expression VisitAndConvert(LambdaExpression root)
            {
                return Visit(root);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                // Leave all parameters alone except the one we want to replace.
                var parameters = node.Parameters.Select
                                 (p => p.Name == _source ? _target : p);
                return Expression.Lambda(Visit(node.Body), parameters);
            }

            public override Expression Visit(Expression node)
            {
                var l = node as LambdaExpression;
                var par = node as ParameterExpression;
                if (l != null)
                {
                    var parameters = l.Parameters.Select
                                     (p => p.Name == _source ? _target : p);
                    return Expression.Lambda(Visit(l.Body), parameters);
                }
                else if (par != null)
                {
                    if (par.Name == _source)
                        return _target;
                    else
                        return base.Visit(node);
                }
                else
                    return base.Visit(node);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                // Replace the source with the target, visit other params as usual.
                return node.Name == _source ? _target : base.VisitParameter(node);
            }
        }


    }
}
