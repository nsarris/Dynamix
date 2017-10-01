using Dynamix.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions.Builders
{
    public class StaticFieldAccessorBuilder
    {
        private Expression GetCastedValueExpression(Expression expression, Type returnType)
        {
            if (returnType != null && expression.Type != returnType)
                expression = Expression.Convert(expression, returnType);
            return expression;
        }

        public LambdaExpression BuildTypedGetterExpression(FieldInfo fieldInfo, Type valueType = null)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));


            var delegateType = GenericTypeExtensions.GetFuncGenericType(valueType ?? fieldInfo.FieldType);

            return Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Field(null, fieldInfo), valueType));
        }

        public LambdaExpression BuildTypedSetterExpression(FieldInfo fieldInfo, Type valueType = null)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            valueType = valueType ?? fieldInfo.FieldType;
            var value = Expression.Parameter(valueType, "value");
            var delegateType = GenericTypeExtensions.GetActionGenericType(valueType);

            if (!fieldInfo.IsInitOnly)
            {
                return Expression.Lambda(
                    delegateType,
                    Expression.Assign(
                        Expression.Field(null, fieldInfo),
                        GetCastedValueExpression(value, fieldInfo.FieldType)),
                    value);
            }
            else
            {
                var m = FieldAccessorMethodEmitter.GetFieldSetterMethod(fieldInfo, delegateType);
                return Expression.Lambda(
                    delegateType,
                    Expression.Call(m, GetCastedValueExpression(value, fieldInfo.FieldType)),
                    value);
            }
        }

        public Expression<Func<object>> BuildGetterExpression(FieldInfo fieldInfo)
        {
            return (Expression<Func<object>>)BuildTypedGetterExpression(fieldInfo, typeof(object));
        }

        public Expression<Action<object>> BuildSetterExpression(FieldInfo fieldInfo)
        {
            return (Expression<Action<object>>)BuildTypedSetterExpression(fieldInfo, typeof(object));
        }

        public Expression<Func<TField>> BuildTypedGetterExpression<TField>(FieldInfo fieldInfo)
        {
            return (Expression<Func<TField>>)BuildTypedGetterExpression(fieldInfo, typeof(TField));
        }

        public Expression<Action<TField>> BuildTypedSetterExpression<TField>(FieldInfo fieldInfo)
        {
            return (Expression<Action<TField>>)BuildTypedSetterExpression(fieldInfo, typeof(TField));
        }
    }
}
