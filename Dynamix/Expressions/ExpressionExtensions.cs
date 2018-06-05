using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions.Extensions
{
    public static class ExpressionExtensions
    {
        public static void VisitAll(this Expression expression, Func<Expression, Expression> visitorFunc)
        {
            var visitor = new ExpressionForEachVisitor(visitorFunc);
            visitor.Visit(expression);
        }

        public static void VisitOfType<TExpression>(this Expression expression, Func<TExpression, Expression> visitorFunc)
            where TExpression : Expression
        {
            var visitor = new ExpressionForEachOfTypeVisitor<TExpression>(visitorFunc);
            visitor.Visit(expression);
        }

        public static List<ParameterExpression> GetParameters(this Expression expression)
        {
            var r = new List<ParameterExpression>();
            var visitor = new ExpressionForEachOfTypeVisitor<ParameterExpression>(
                (ParameterExpression p) =>
                {
                    r.Add(p);
                    return p;
                });
            visitor.Visit(expression);
            return r.Distinct().ToList();
        }


        public static List<ParameterExpression> GetParametersOfType<T>(this Expression expression)
        {
            var r = new List<ParameterExpression>();
            var visitor = new ExpressionForEachOfTypeVisitor<ParameterExpression>(
                (ParameterExpression p) =>
                {
                    if (p.Type == typeof(T))
                        r.Add(p);

                    return p;
                });
            visitor.Visit(expression);
            return r.Distinct().ToList();
        }

        public static ParameterExpression GetFirstParameterOfType<T>(this Expression expression)
        {
            ParameterExpression r = null;
            var visitor = new ExpressionForEachOfTypeVisitor<ParameterExpression>((ParameterExpression p) =>
            {
                if (p.Type == typeof(T))
                {
                    r = p;
                    return null;
                }

                return p;
            });
            visitor.Visit(expression);
            return r;
        }

        public static Expression ApplyConditional(this Expression expression, bool condition, Func<Expression, Expression> f)
        {
            return condition ? f(expression) : expression;
        }

        public static Expression ConditionalNot(this Expression expression, bool condition)
        {
            return condition ? Expression.Not(expression) : expression;
        }

        public static Expression IsNull(this Expression expression, bool not = false)
        {
            return
                not ?
                Expression.NotEqual(expression, ExpressionEx.Constants.Null) :
                Expression.Equal(expression, ExpressionEx.Constants.Null);
        }

    }
}
