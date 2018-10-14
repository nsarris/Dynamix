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
        public static Expression Parse(ParameterExpression[] parameters, Type resultType, string expression, params object[] values)
        {
            return System.Linq.Dynamic.DynamicExpression.Parse(parameters, resultType, expression, values);
        }

        public static Expression Parse(Type resultType, string expression, params object[] values)
        {
            return System.Linq.Dynamic.DynamicExpression.Parse(resultType, expression, values);
        }

        public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, params object[] values)
        {
            return System.Linq.Dynamic.DynamicExpression.ParseLambda(itType, resultType, expression, values);
        }

        public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, string expression, params object[] values)
        {
            return System.Linq.Dynamic.DynamicExpression.ParseLambda(parameters, resultType, expression, values);
        }

        public static LambdaExpression ParseLambda(Type delegateType, ParameterExpression[] parameters, Type resultType, string expression, params object[] values)
        {
            return System.Linq.Dynamic.DynamicExpression.ParseLambda(delegateType, parameters, resultType, expression, values);
        }

        public static Expression<Func<T, S>> ParseLambda<T, S>(string expression, params object[] values)
        {
            return System.Linq.Dynamic.DynamicExpression.ParseLambda<T, S>(expression, values);
        }
    }
}
