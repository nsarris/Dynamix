using Dynamix.Expressions.Visitors;
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
        public static Expression VisitAll(this Expression expression, Func<Expression, Expression> visitor)
        {
            return new ExpressionForEachVisitor(visitor)
                .Visit(expression);
        }

        public static Expression VisitOfType<TExpression>(this Expression expression, Func<TExpression, Expression> visitor)
            where TExpression : Expression
        {
            return new ExpressionForEachOfTypeVisitor<TExpression>(visitor)
                .Visit(expression);
        }

        public static void VisitAll(this Expression expression, Action<Expression> visitor)
        {
            new ExpressionForEachVisitor(visitor)
                .Visit(expression);
        }

        public static void VisitOfType<TExpression>(this Expression expression, Action<TExpression> visitor)
            where TExpression : Expression
        {
            new ExpressionForEachOfTypeVisitor<TExpression>(visitor)
                .Visit(expression);
        }

        public static List<ParameterExpression> GetParameters(this Expression expression, Func<ParameterExpression, bool> filter = null)
        {
            var r = new List<ParameterExpression>();

            expression.VisitOfType<ParameterExpression>(
                p =>
                {
                    if (!r.Contains(p)
                    && (filter == null || filter(p)))
                        r.Add(p);
                });
            
            return r;
        }


        public static List<ParameterExpression> GetParametersOfType<T>(this Expression expression)
        {
            return GetParameters(expression, x => x.Type == typeof(T));
        }

        public static List<ParameterExpression> GetParametersOfType(this Expression expression, Type type)
        {
            return GetParameters(expression, x => x.Type == type);
        }

        public static ParameterExpression GetFirstParameterOfType<T>(this Expression expression)
        {
            return GetFirstParameterOfType(expression, typeof(T));
        }

        public static ParameterExpression GetFirstParameterOfType(this Expression expression, Type type)
        {
            ParameterExpression r = null;

            expression.VisitOfType<ParameterExpression>(
                p =>
                {
                    if (p.Type == type)
                    {
                        r = p;
                        return null;
                    }

                    return p;
                });
            
            return r;
        }

        public static Expression ApplyConditional(this Expression expression, bool condition, Func<Expression, Expression> f)
        {
            return condition ? f(expression) : expression;
        }

        public static Expression ConditionalNot(this Expression expression, bool condition)
        {
            if (expression is ConstantExpression constantExpression)
                return condition ?
                    ExpressionEx.Constants.Bool(true.Equals(constantExpression.Value)) :
                    constantExpression;

            return condition ? Expression.Not(expression) : expression;
        }

        public static Expression IsNull(this Expression expression, bool not = false)
        {
            return
                not ?
                Expression.NotEqual(expression, ExpressionEx.Constants.Null) :
                Expression.Equal(expression, ExpressionEx.Constants.Null);
        }

        public static bool CheckEquals(this Expression expression,Expression other)
        {
            return ExpressionEqualityComparer.Instance.Equals(other);
        }

        public static int CalculateHashCode(this Expression expression)
        {
            return ExpressionEqualityComparer.Instance.GetHashCode(expression);
        }
    }
}
