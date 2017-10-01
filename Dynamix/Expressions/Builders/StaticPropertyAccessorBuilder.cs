using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions.Builders
{
    public class StaticPropertyAccessorBuilder
    {
        private Expression GetCastedValueExpression(Expression expression, Type returnType)
        {
            if (returnType != null && expression.Type != returnType)
                expression = Expression.Convert(expression, returnType);
            return expression;
        }


        public LambdaExpression BuildTypedGetterExpression(PropertyInfo propertyInfo, Type ValueType = null)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(PropertyInfo));

            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
                throw new ArgumentException("Property has no get method", nameof(PropertyInfo));

            var delegateType = GenericTypeExtensions.GetFuncGenericType(ValueType ?? propertyInfo.PropertyType);

            return Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Call(null, getMethod), ValueType));
        }


        public LambdaExpression BuildTypedSetterExpression(PropertyInfo propertyInfo, Type ValueType = null)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(PropertyInfo));

            var setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod == null)
                throw new ArgumentException("Property has no set method", nameof(PropertyInfo));

            var value = Expression.Parameter(ValueType ?? propertyInfo.PropertyType, "value");
            var delegateType = GenericTypeExtensions.GetActionGenericType(ValueType ?? propertyInfo.PropertyType);

            return Expression.Lambda(
                delegateType,
                Expression.Call(null, setMethod, GetCastedValueExpression(value, propertyInfo.PropertyType)),
                value);
        }

        public Expression<Func<object>> BuildGetterExpression(PropertyInfo propertyInfo)
        {
            return (Expression<Func<object>>)BuildTypedGetterExpression(propertyInfo, typeof(object));
        }

        public Expression<Action<object>> BuildSetterExpression(PropertyInfo propertyInfo)
        {
            return (Expression<Action<object>>)BuildTypedSetterExpression(propertyInfo, typeof(object));
        }

        public Expression<Func<TProp>> BuildTypedGetterExpression<TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<TProp>>)BuildTypedGetterExpression(propertyInfo, typeof(TProp));
        }

        public Expression<Action<TProp>> BuildTypedSetterExpression<TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<TProp>>)BuildTypedSetterExpression(propertyInfo, typeof(TProp));
        }
    }
}
