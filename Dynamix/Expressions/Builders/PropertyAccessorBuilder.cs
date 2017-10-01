using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Expressions;

namespace Dynamix.Expressions.Builders
{
    public class InstancePropertyAccessorBuilder
    {
        //public InstancePropertyAccessorBuilder()
        //{
        //    if (typeof(T).IsPrimitive || typeof(T).IsEnum)
        //        throw new InvalidOperationException(this.GetType().Name + " cannot be used with primitive types or enums");
        //}


        private Expression GetCastedInstanceExpression(ParameterExpression instance, PropertyInfo propertyInfo)
        {
            var castedInstance = (Expression)instance;
            if (instance.Type != propertyInfo.DeclaringType)
            {
                if (instance.Type.IsSubclassOf(propertyInfo.DeclaringType))
                    castedInstance = Expression.TypeAs(instance, propertyInfo.DeclaringType);
                else if (instance.Type == typeof(object))
                    castedInstance = Expression.Convert(instance, propertyInfo.DeclaringType);
                else
                    throw new ArgumentException("Property " + propertyInfo.Name + " is not a member of " + instance.Type.Name);
            }
            return castedInstance;
        }

        private Expression GetCastedValueExpression(Expression expression, Type returnType)
        {
            if (returnType != null && expression.Type != returnType)
                expression = Expression.Convert(expression, returnType);
            return expression;
        }


        public LambdaExpression BuildTypedGetterExpression(PropertyInfo propertyInfo, Type instanceType = null, Type valueType = null, IEnumerable<Type> indexerTypes = null)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
                throw new ArgumentException("Property has no get method", nameof(propertyInfo));

            instanceType = instanceType ?? propertyInfo.DeclaringType;
            valueType = valueType ?? propertyInfo.PropertyType;

            var instance = Expression.Parameter(instanceType, "instance");
            var castedInstance = GetCastedInstanceExpression(instance, propertyInfo);
            var indexedParameters = propertyInfo.GetIndexParameters();
            var indexedParametersExpressions = indexerTypes == null ?
                indexedParameters.Select((x, i) => Expression.Parameter(x.ParameterType, "itemIndexer" + i)).ToList() :
                indexedParameters.Zip(indexerTypes, (p, i) => new { p, i }).Select((x, i) => Expression.Parameter(x.i ?? x.p.ParameterType, "itemIndexer" + i)).ToList();

            var delegateType = GenericTypeExtensions.GetFuncGenericType(new Type[] { instanceType }.Concat(indexedParametersExpressions.Select(x => x.Type)).Concat(new Type[] { valueType}));
            var methodCallparameters = indexerTypes == null ?
                indexedParametersExpressions :
                indexedParametersExpressions.Zip(indexedParameters, (p, i) => GetCastedValueExpression(p, i.ParameterType));

            var expressionParameters = new[] { instance }.Concat(indexedParametersExpressions);

            return Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Call(castedInstance, getMethod, methodCallparameters), valueType),
                    expressionParameters);
        }


        public LambdaExpression BuildTypedSetterExpression(PropertyInfo propertyInfo, Type instanceType = null, Type valueType = null, IEnumerable<Type> indexerTypes = null)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            var setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod == null)
                throw new ArgumentException("Property has no set method", nameof(propertyInfo));

            instanceType = instanceType ?? propertyInfo.DeclaringType;
            valueType = valueType ?? propertyInfo.PropertyType;

            var instance = Expression.Parameter(instanceType, "instance");
            var value = Expression.Parameter(valueType, "value");

            var indexedParameters = propertyInfo.GetIndexParameters();
            var indexedParametersExpressions = indexerTypes == null ?
                indexedParameters.Select((x, i) => Expression.Parameter(x.ParameterType, "itemIndexer" + i)).ToList() :
                indexedParameters.Zip(indexerTypes, (p, i) => new { p, i }).Select((x, i) => Expression.Parameter(x.i ?? x.p.ParameterType, "itemIndexer" + i)).ToList();

            var expressionParameters = new[] { instance }.Concat(indexedParametersExpressions).Concat(new[] { value });

            var castedInstance = GetCastedInstanceExpression(instance, propertyInfo);
            var methodCallparameters = (indexerTypes == null ?
                indexedParametersExpressions :
                indexedParametersExpressions.Zip(indexedParameters, (p, i) => GetCastedValueExpression(p, i.ParameterType)))
                .Cast<Expression>().Concat(new[] { GetCastedValueExpression(value, propertyInfo.PropertyType) });

            var delegateType = GenericTypeExtensions.GetActionGenericType(new Type[] { instanceType }.Concat(expressionParameters.Skip(1).Select(x => x.Type)));

            return Expression.Lambda(
                delegateType,
                Expression.Call(castedInstance, setMethod, methodCallparameters),
                expressionParameters);
        }


        public Expression<Func<T, object>> BuildGetterExpression<T>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, object>>)BuildTypedGetterExpression(propertyInfo, typeof(T), typeof(object));
        }

        public Expression<Action<T, object>> BuildSetterExpression<T>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, object>>)BuildTypedSetterExpression(propertyInfo, typeof(T), typeof(object));
        }

        public Expression<Func<T, TProp>> BuildTypedGetterExpression<T, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TProp>>)BuildTypedGetterExpression(propertyInfo, typeof(T), typeof(TProp));
        }

        public Expression<Action<T, TProp>> BuildTypedSetterExpression<T,TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TProp>>)BuildTypedSetterExpression(propertyInfo, typeof(T), typeof(TProp));
        }


        public Expression<Func<T, TIndex, TProp>> BuildTypedIndexedGetterExpression<T,TIndex, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TIndex, TProp>>)BuildTypedGetterExpression(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex) });
        }
        public Expression<Func<T, TIndex1, TIndex2, TProp>> BuildTypedIndexedGetterExpression<T,TIndex1, TIndex2, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TIndex1, TIndex2, TProp>>)BuildTypedGetterExpression(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2) });
        }
        public Expression<Func<T, TIndex1, TIndex2, TIndex3, TProp>> BuildTypedIndexedGetterExpression<T,TIndex1, TIndex2, TIndex3, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TIndex1, TIndex2, TIndex3, TProp>>)BuildTypedGetterExpression(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3) });
        }
        public Expression<Func<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>> BuildTypedIndexedGetterExpression<T,TIndex1, TIndex2, TIndex3, TIndex4, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>>)BuildTypedGetterExpression(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3), typeof(TIndex4) });
        }

        public Expression<Action<T, TIndex, TProp>> BuildTypedIndexedSetterExpression<T,TIndex, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TIndex, TProp>>)BuildTypedSetterExpression(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex) });
        }
        public Expression<Action<T, TIndex1, TIndex2, TProp>> BuildTypedIndexedSetterExpression<T,TIndex1, TIndex2, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TIndex1, TIndex2, TProp>>)BuildTypedSetterExpression(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2) });
        }
        public Expression<Action<T, TIndex1, TIndex2, TIndex3, TProp>> BuildTypedIndexedSetterExpression<T,TIndex1, TIndex2, TIndex3, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TIndex1, TIndex2, TIndex3, TProp>>)BuildTypedSetterExpression(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3) });
        }
        public Expression<Action<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>> BuildTypedIndexedSetterExpression<T,TIndex1, TIndex2, TIndex3, TIndex4, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>>)BuildTypedSetterExpression(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3), typeof(TIndex4) });
        }
    }
}
