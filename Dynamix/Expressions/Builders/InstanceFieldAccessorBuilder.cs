using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Expressions;
using System.Reflection.Emit;
using Dynamix.Reflection.Emit;

namespace Dynamix.Expressions.Builders
{
    public class InstanceFieldAccessorBuilder
    {
        private Expression GetCastedInstanceExpression(ParameterExpression instance, FieldInfo fieldInfo)
        {

            var castedInstance = (Expression)instance;
            if (instance.Type != fieldInfo.DeclaringType)
            {
                if (instance.Type.IsSubclassOf(fieldInfo.DeclaringType))
                    castedInstance = Expression.TypeAs(instance, fieldInfo.DeclaringType);
                else if (instance.Type == typeof(object))
                    castedInstance = Expression.Convert(instance, fieldInfo.DeclaringType);
                else
                    throw new ArgumentException("Property " + fieldInfo.Name + " is not a member of " + instance.Type.Name);
            }
            return castedInstance;
        }

        private Expression GetCastedValueExpression(Expression expression, Type returnType)
        {
            if (returnType != null && expression.Type != returnType)
                expression = Expression.Convert(expression, returnType);
            return expression;
        }


        public LambdaExpression BuildTypedGetterExpression(FieldInfo fieldInfo, Type instanceType = null, Type valueType = null)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            //TODO exception if static

            instanceType = instanceType ?? fieldInfo.DeclaringType;
            valueType = valueType ?? fieldInfo.FieldType;

            var instance = Expression.Parameter(instanceType, "instance");
            var castedInstance = GetCastedInstanceExpression(instance, fieldInfo);

            var delegateType = GenericTypeExtensions.GetFuncGenericType(instanceType, valueType);

            return Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Field(castedInstance, fieldInfo), valueType),
                    instance);
        }


        public LambdaExpression BuildTypedSetterExpression(FieldInfo fieldInfo, Type instanceType = null, Type valueType = null)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            instanceType = instanceType ?? fieldInfo.DeclaringType;
            valueType = valueType ?? fieldInfo.FieldType;

            var instance = Expression.Parameter(instanceType, "instance");
            var value = Expression.Parameter(valueType, "value");
            
            var expressionParameters = new[] { instance, value };

            var castedInstance = GetCastedInstanceExpression(instance, fieldInfo);

            var delegateType = GenericTypeExtensions.GetActionGenericType(instanceType, valueType);

            if (!fieldInfo.IsInitOnly)
            {
                return Expression.Lambda(
                    delegateType,
                    Expression.Assign(
                        Expression.Field(castedInstance, fieldInfo),
                        GetCastedValueExpression(value, fieldInfo.FieldType)),
                    expressionParameters);
            }
            else
            {
                var m = FieldAccessorMethodEmitter.GetFieldSetterMethod(fieldInfo, delegateType);
                return Expression.Lambda(
                    delegateType,
                    Expression.Call(m, castedInstance, GetCastedValueExpression(value, fieldInfo.FieldType)),
                    expressionParameters);
            }
        }

        public Expression<Func<object, object>> BuildGetterExpression(FieldInfo fieldInfo)
        {
            return (Expression<Func<object, object>>)BuildTypedGetterExpression(fieldInfo, typeof(object), typeof(object));
        }

        public Expression<Action<object, object>> BuildSetterExpression(FieldInfo fieldInfo)
        {
            return (Expression<Action<object, object>>)BuildTypedSetterExpression(fieldInfo, typeof(object), typeof(object));
        }

        public Expression<Func<T, object>> BuildGetterExpression<T>(FieldInfo fieldInfo)
        {
            return (Expression<Func<T, object>>)BuildTypedGetterExpression(fieldInfo,typeof(T), typeof(object));
        }

        public Expression<Action<T, object>> BuildSetterExpression<T>(FieldInfo fieldInfo)
        {
            return (Expression<Action<T, object>>)BuildTypedSetterExpression(fieldInfo, typeof(T), typeof(object));
        }

        public Expression<Func<T, TField>> BuildTypedGetterExpression<T,TField>(FieldInfo fieldInfo)
        {
            return (Expression<Func<T, TField>>)BuildTypedGetterExpression(fieldInfo, typeof(T), typeof(TField));
        }

        public Expression<Action<T, TField>> BuildTypedSetterExpression<T,TField>(FieldInfo fieldInfo)
        {
            return (Expression<Action<T, TField>>)BuildTypedSetterExpression(fieldInfo, typeof(T), typeof(TField));
        }
    }
    
   

}
