using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.DynamicLinq
{
    public static class DynamicExpressionParser
    {
        static readonly System.Linq.Dynamic.Core.ParsingConfig config = new System.Linq.Dynamic.Core.ParsingConfig
        {
            
        };

        public static Expression Parse(ParameterExpression[] parameters, Type resultType, string expression, params object[] values)
        {
            return new System.Linq.Dynamic.Core.Parser.ExpressionParser(parameters, expression, values, config).Parse(resultType);
        }

        public static Expression Parse(Type resultType, string expression, params object[] values)
        {
            return new System.Linq.Dynamic.Core.Parser.ExpressionParser(null, expression, values, config).Parse(resultType);
        }

        public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, params object[] values)
        {
            var parameter = Expression.Parameter(itType);
            var expr = new System.Linq.Dynamic.Core.Parser.ExpressionParser(new[] { parameter }, expression, values, config).Parse(resultType);
            resultType = resultType ?? expr.Type;
            var delegateType = Expression.GetDelegateType(new[] { itType, resultType });

            return Expression.Lambda(delegateType, expr, parameter);
        }

        public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, string expression, params object[] values)
        {
            var expr = new System.Linq.Dynamic.Core.Parser.ExpressionParser(parameters, expression, values, config).Parse(resultType);
            resultType = resultType ?? expr.Type;
            var delegateType = Expression.GetDelegateType(parameters.Select(x => x.Type).Concat( new[] { resultType }).ToArray());

            return Expression.Lambda(delegateType, expr, parameters);
        }

        public static LambdaExpression ParseLambda(Type delegateType, ParameterExpression[] parameters, Type resultType, string expression, params object[] values)
        {
            var expr = new System.Linq.Dynamic.Core.Parser.ExpressionParser(parameters, expression, values, config).Parse(resultType);
            
            return Expression.Lambda(delegateType, expr, parameters);
        }

        public static Expression<Func<T, S>> ParseLambda<T, S>(string expression, params object[] values)
        {
            var parameter = Expression.Parameter(typeof(T));
            var expr = new System.Linq.Dynamic.Core.Parser.ExpressionParser(new[] { parameter }, expression, values, config).Parse(typeof(S));
            
            return Expression.Lambda<Func<T, S>>(expr, parameter);
        }
    }
}
