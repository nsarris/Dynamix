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

namespace Dynamix.Expressions
{
    public class InstanceFieldAccessorBuilder<T>
    {
        public InstanceFieldAccessorBuilder()
        {
            if (typeof(T).IsPrimitive || typeof(T).IsEnum)
                throw new InvalidOperationException(this.GetType().Name + " cannot be used with primitive types or enums");
        }


        private Expression GetCastedInstanceExpression(ParameterExpression instance, FieldInfo FieldInfo)
        {

            var castedInstance = (Expression)instance;
            if (typeof(T) != FieldInfo.DeclaringType)
            {
                if (typeof(T).IsSubclassOf(FieldInfo.DeclaringType))
                    castedInstance = Expression.TypeAs(instance, FieldInfo.DeclaringType);
                else if (typeof(T) == typeof(object))
                    castedInstance = Expression.Convert(instance, FieldInfo.DeclaringType);
                else
                    throw new ArgumentException("Property " + FieldInfo.Name + " is not a member of " + typeof(T).Name);
            }
            return castedInstance;
        }

        private Expression GetCastedValueExpression(Expression expression, Type returnType)
        {
            if (returnType != null && expression.Type != returnType)
                expression = Expression.Convert(expression, returnType);
            return expression;
        }


        public LambdaExpression BuildTypedGetterExpression(FieldInfo FieldInfo, Type ValueType = null)
        {
            if (FieldInfo == null)
                throw new ArgumentNullException(nameof(FieldInfo));

            //TODO exception if static

            var instance = Expression.Parameter(typeof(T), "instance");
            var castedInstance = GetCastedInstanceExpression(instance, FieldInfo);

            var delegateType = GenericTypeExtensions.GetFuncGenericType(typeof(T), ValueType ?? FieldInfo.FieldType);

            return Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Field(castedInstance, FieldInfo), ValueType),
                    instance);
        }


        public LambdaExpression BuildTypedSetterExpression(FieldInfo FieldInfo, Type ValueType = null)
        {
            if (FieldInfo == null)
                throw new ArgumentNullException(nameof(FieldInfo));

            var instance = Expression.Parameter(typeof(T), "instance");
            var value = Expression.Parameter(ValueType ?? FieldInfo.FieldType, "value");


            var expressionParameters = new[] { instance, value };

            var castedInstance = GetCastedInstanceExpression(instance, FieldInfo);

            var delegateType = GenericTypeExtensions.GetActionGenericType(typeof(T), ValueType ?? FieldInfo.FieldType);

            if (!FieldInfo.IsInitOnly)
            {
                return Expression.Lambda(
                    delegateType,
                    Expression.Assign(
                        Expression.Field(castedInstance, FieldInfo),
                        GetCastedValueExpression(value, FieldInfo.FieldType)),
                    expressionParameters);
            }
            else
            {
                var m = FieldAccessorMethodEmitter.GetFieldSetterMethod(FieldInfo, delegateType);
                return Expression.Lambda(
                    delegateType,
                    Expression.Call(m, castedInstance, GetCastedValueExpression(value, FieldInfo.FieldType)),
                    expressionParameters);
            }
        }

        //public Func<T, TField> BuildTypedGetter<TField>(FieldInfo FieldInfo)
        //{
        //    return BuildTypedGetterExpression<TField>(FieldInfo).Compile();
        //}

        //public Action<T, TField> BuildTypedSetter<TField>(FieldInfo FieldInfo)
        //{
        //    return BuildTypedSetterExpression<TField>(FieldInfo).Compile();
        //}

        public Expression<Func<T, object>> BuildGetterExpression(FieldInfo FieldInfo)
        {
            return (Expression<Func<T, object>>)BuildTypedGetterExpression(FieldInfo, typeof(object));
        }

        public Expression<Action<T, object>> BuildSetterExpression(FieldInfo FieldInfo)
        {
            return (Expression<Action<T, object>>)BuildTypedSetterExpression(FieldInfo, typeof(object));
        }

        public Expression<Func<T, TField>> BuildTypedGetterExpression<TField>(FieldInfo FieldInfo)
        {
            return (Expression<Func<T, TField>>)BuildTypedGetterExpression(FieldInfo, typeof(TField));
        }

        public Expression<Action<T, TField>> BuildTypedSetterExpression<TField>(FieldInfo FieldInfo)
        {
            return (Expression<Action<T, TField>>)BuildTypedSetterExpression(FieldInfo, typeof(TField));
        }

        private static void BuildFieldSetterMethod()
        {

        }
    }

    public class FieldAccessorBuilder : InstanceFieldAccessorBuilder<object>
    {

    }

    public class StaticFieldAccessorBuilder
    {
        private Expression GetCastedValueExpression(Expression expression, Type returnType)
        {
            if (returnType != null && expression.Type != returnType)
                expression = Expression.Convert(expression, returnType);
            return expression;
        }

        public LambdaExpression BuildTypedGetterExpression(FieldInfo FieldInfo, Type ValueType = null)
        {
            if (FieldInfo == null)
                throw new ArgumentNullException(nameof(FieldInfo));

            
            var delegateType = GenericTypeExtensions.GetFuncGenericType(ValueType ?? FieldInfo.FieldType);

            return Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Field(null, FieldInfo), ValueType));
        }

        public LambdaExpression BuildTypedSetterExpression(FieldInfo FieldInfo, Type ValueType = null)
        {
            if (FieldInfo == null)
                throw new ArgumentNullException(nameof(FieldInfo));

            var value = Expression.Parameter(ValueType ?? FieldInfo.FieldType, "value");
            var delegateType = GenericTypeExtensions.GetActionGenericType(ValueType ?? FieldInfo.FieldType);

            if (!FieldInfo.IsInitOnly)
            {
                return Expression.Lambda(
                    delegateType,
                    Expression.Assign(
                        Expression.Field(null, FieldInfo),
                        GetCastedValueExpression(value, FieldInfo.FieldType)),
                    value);
            }
            else
            {
                var m = FieldAccessorMethodEmitter.GetFieldSetterMethod(FieldInfo, delegateType);
                return Expression.Lambda(
                    delegateType,
                    Expression.Call(m, GetCastedValueExpression(value, FieldInfo.FieldType)),
                    value);
            }
        }

        public Expression<Func<object>> BuildGetterExpression(FieldInfo FieldInfo)
        {
            return (Expression<Func<object>>)BuildTypedGetterExpression(FieldInfo, typeof(object));
        }

        public Expression<Action<object>> BuildSetterExpression(FieldInfo FieldInfo)
        {
            return (Expression<Action<object>>)BuildTypedSetterExpression(FieldInfo, typeof(object));
        }

        public Expression<Func<TField>> BuildTypedGetterExpression<TField>(FieldInfo FieldInfo)
        {
            return (Expression<Func<TField>>)BuildTypedGetterExpression(FieldInfo, typeof(TField));
        }

        public  Expression<Action<TField>> BuildTypedSetterExpression<TField>(FieldInfo FieldInfo)
        {
            return (Expression<Action<TField>>)BuildTypedSetterExpression(FieldInfo, typeof(TField));
        }
    }

}
