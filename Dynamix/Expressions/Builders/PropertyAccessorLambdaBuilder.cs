using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Expressions;
using System.Collections.Concurrent;
using Dynamix.Reflection;

namespace Dynamix.Expressions.LambdaBuilders
{
    public delegate object GenericPropertyGetter(object instance, params object[] indexParameters);
    public delegate void GenericPropertySetter(object instance, object value, params object[] indexParameters);

    public class PropertyAccessorLambdaBuilder
    {
        #region Ctor

        public PropertyAccessorLambdaBuilder()
        {

        }

        public PropertyAccessorLambdaBuilder(bool enableCaching)
        {
            this.EnableCaching = enableCaching;
        }

        #endregion Ctor

        #region Cache
        private class CacheKey : HashKey
        {
            public CacheKey(PropertyInfo propertyInfo, Type instanceType, Type valueType, IEnumerable<Type> indexerTypes)
                : base(new object[] { propertyInfo, instanceType, valueType }.Concat(indexerTypes == null ? Enumerable.Empty<object>() : indexerTypes.Cast<object>()))
            {
            }
        }

        static ConcurrentDictionary<PropertyInfo, LambdaExpression> genericGetterCache = new ConcurrentDictionary<PropertyInfo, LambdaExpression>();
        static ConcurrentDictionary<PropertyInfo, LambdaExpression> genericSetterCache = new ConcurrentDictionary<PropertyInfo, LambdaExpression>();
        static ConcurrentDictionary<HashKey, LambdaExpression> getterCache = new ConcurrentDictionary<HashKey, LambdaExpression>();
        static ConcurrentDictionary<HashKey, LambdaExpression> setterCache = new ConcurrentDictionary<HashKey, LambdaExpression>();

        private bool EnableCaching { get; set; } = true;

        #endregion

        #region Helpers
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

        private MethodInfo ValidatePropertyAndGetAccessorMethod(PropertyInfo propertyInfo, bool? isStatic, bool getOrSet) //true = get, false = set
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            var method = getOrSet ? propertyInfo.GetGetMethod(true) : propertyInfo.GetSetMethod(true);

            var getterOrSetter = getOrSet ? "getter" : "setter";

            if (method == null)
                throw new ArgumentException("Property has no " + getterOrSetter + " method", nameof(propertyInfo));

            if (isStatic.HasValue && method.IsStatic != isStatic)
                throw new ArgumentException("Property " + getterOrSetter + " is not " + ((isStatic == true) ? "static" : "instance"), nameof(propertyInfo));

            return method;
        }

        #endregion Helpers

        #region Untyped Builders

        public Expression<GenericPropertyGetter> BuildGenericGetter(PropertyInfo propertyInfo)
        {
            var getMethod = ValidatePropertyAndGetAccessorMethod(propertyInfo, null, true);

            if (EnableCaching
                && genericGetterCache.TryGetValue(propertyInfo, out var cachedLambda))
                    return (Expression < GenericPropertyGetter > )cachedLambda;
            
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var inputParameters = Expression.Parameter(typeof(object[]), "indexers");

            var methodCallParameters = propertyInfo.GetIndexParameters()
                .Select((x, i) => ExpressionEx.ConvertIfNeeded(
                            Expression.ArrayAccess(inputParameters, Expression.Constant(i)),
                            x.ParameterType));

            var invokerExpression =
                Expression.Call(
                getMethod.IsStatic ? null : ExpressionEx.ConvertIfNeeded(instanceParameter, getMethod.DeclaringType),
                getMethod,
                methodCallParameters);

            var body = ExpressionEx.CastTypeSafe(invokerExpression, typeof(object));
            var lambda = Expression.Lambda<GenericPropertyGetter>(body, new[] { instanceParameter, inputParameters });

            if (EnableCaching)
                genericGetterCache.TryAdd(propertyInfo, lambda);

            return lambda;
        }

        public Expression<GenericPropertySetter> BuildGenericSetter(PropertyInfo propertyInfo)
        {
            var setMethod = ValidatePropertyAndGetAccessorMethod(propertyInfo, null, false);

            if (EnableCaching
                && genericSetterCache.TryGetValue(propertyInfo, out var cachedLambda))
                    return (Expression < GenericPropertySetter > )cachedLambda;
            
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var inputParameters = Expression.Parameter(typeof(object[]), "indexers");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var methodCallParameters = propertyInfo.GetIndexParameters()
                .Select((x, i) => ExpressionEx.ConvertIfNeeded(
                            Expression.ArrayAccess(inputParameters, Expression.Constant(i)),
                            x.ParameterType))
                .Concat(new[] { ExpressionEx.ConvertIfNeeded(valueParameter, propertyInfo.PropertyType) });

            var invokerExpression =
                Expression.Call(
                setMethod.IsStatic ? null : ExpressionEx.ConvertIfNeeded(instanceParameter, setMethod.DeclaringType),
                setMethod,
                methodCallParameters);

            var lambda = Expression.Lambda<GenericPropertySetter>(invokerExpression, new[] { instanceParameter, valueParameter, inputParameters });

            if (EnableCaching)
                genericSetterCache.TryAdd(propertyInfo, lambda);

            return lambda;
        }

        #endregion Untyped Builders

        #region Instance Builders

        public LambdaExpression BuildInstanceGetter(PropertyInfo propertyInfo, Type instanceType = null, Type valueType = null, IEnumerable<Type> indexerTypes = null)
        {
            var getMethod = ValidatePropertyAndGetAccessorMethod(propertyInfo, false, true);

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(propertyInfo, instanceType, valueType, indexerTypes);
                if (getterCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            instanceType = instanceType ?? propertyInfo.DeclaringType;
            valueType = valueType ?? propertyInfo.PropertyType;

            var instance = Expression.Parameter(instanceType, "instance");
            var castedInstance = GetCastedInstanceExpression(instance, propertyInfo);
            var indexedParameters = propertyInfo.GetIndexParameters();
            var indexedParametersExpressions = indexerTypes == null ?
                indexedParameters.Select((x, i) => Expression.Parameter(x.ParameterType, "itemIndexer" + i)).ToList() :
                indexedParameters.Zip(indexerTypes, (p, i) => new { p, i }).Select((x, i) => Expression.Parameter(x.i ?? x.p.ParameterType, "itemIndexer" + i)).ToList();

            var delegateType = GenericTypeExtensions.GetFuncGenericType(new Type[] { instanceType }.Concat(indexedParametersExpressions.Select(x => x.Type)).Concat(new Type[] { valueType }));
            var methodCallparameters = indexerTypes == null ?
                indexedParametersExpressions :
                indexedParametersExpressions.Zip(indexedParameters, (p, i) => GetCastedValueExpression(p, i.ParameterType));

            var expressionParameters = new[] { instance }.Concat(indexedParametersExpressions);

            var lambda = Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Call(castedInstance, getMethod, methodCallparameters), valueType),
                    expressionParameters);

            if (EnableCaching)
                getterCache.TryAdd(cacheKey, lambda);

            return lambda;
        }


        public LambdaExpression BuildInstanceSetter(PropertyInfo propertyInfo, Type instanceType = null, Type valueType = null, IEnumerable<Type> indexerTypes = null)
        {
            var setMethod = ValidatePropertyAndGetAccessorMethod(propertyInfo, false, false);

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(propertyInfo, instanceType, valueType, indexerTypes);
                if (setterCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

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
                indexedParametersExpressions
                    .Zip(indexedParameters, (p, i) => GetCastedValueExpression(p, i.ParameterType)))
                    .Concat(new[] { GetCastedValueExpression(value, propertyInfo.PropertyType) });

            var delegateType = GenericTypeExtensions.GetActionGenericType(new Type[] { instanceType }.Concat(expressionParameters.Skip(1).Select(x => x.Type)));

            var lambda = Expression.Lambda(
                delegateType,
                Expression.Call(castedInstance, setMethod, methodCallparameters),
                expressionParameters);

            if (EnableCaching)
                setterCache.TryAdd(cacheKey, lambda);

            return lambda;
        }

        #endregion Instance Builders

        #region Static Builders

        public LambdaExpression BuildStaticGetter(PropertyInfo propertyInfo, Type valueType = null)
        {
            var getMethod = ValidatePropertyAndGetAccessorMethod(propertyInfo, true, true);

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(propertyInfo, null, valueType, null);
                if (getterCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            var delegateType = GenericTypeExtensions.GetFuncGenericType(valueType ?? propertyInfo.PropertyType);

            var lambda = Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Call(null, getMethod), valueType));

            if (EnableCaching)
                getterCache.TryAdd(cacheKey, lambda);

            return lambda;
        }


        public LambdaExpression BuildStaticSetter(PropertyInfo propertyInfo, Type valueType = null)
        {
            var setMethod = ValidatePropertyAndGetAccessorMethod(propertyInfo, true, false);

            valueType = valueType ?? propertyInfo.PropertyType;

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(propertyInfo, null, valueType, null);
                if (setterCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            var value = Expression.Parameter(valueType, "value");
            var delegateType = GenericTypeExtensions.GetActionGenericType(valueType);

            var lambda = Expression.Lambda(
                delegateType,
                Expression.Call(null, setMethod, GetCastedValueExpression(value, propertyInfo.PropertyType)),
                value);

            if (EnableCaching)
                getterCache.TryAdd(cacheKey, lambda);

            return lambda;
        }

        #endregion Static Builders

        #region Instance Getters and Setters

        public Expression<Func<T, object>> BuildInstanceGetter<T>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, object>>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(object));
        }

        public Expression<Action<T, object>> BuildInstanceSetter<T>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, object>>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(object));
        }

        public Expression<Func<T, TProp>> BuildInstanceGetter<T, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TProp>>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp));
        }

        public Expression<Action<T, TProp>> BuildInstanceSetter<T, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TProp>>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp));
        }

        #endregion Instance Getters and Setters

        #region Static Getters and Setters

        public Expression<Func<object>> BuildStaticGetter(PropertyInfo propertyInfo)
        {
            return (Expression<Func<object>>)BuildStaticGetter(propertyInfo, typeof(object));
        }

        public Expression<Action<object>> BuildStaticSetter(PropertyInfo propertyInfo)
        {
            return (Expression<Action<object>>)BuildStaticSetter(propertyInfo, typeof(object));
        }

        public Expression<Func<TProp>> BuildStaticGetter<TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<TProp>>)BuildStaticGetter(propertyInfo, typeof(TProp));
        }

        public Expression<Action<TProp>> BuildStaticSetter<TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<TProp>>)BuildStaticSetter(propertyInfo, typeof(TProp));
        }

        #endregion Static Getters and Setters

        #region Indexed Getters

        public Expression<Func<T, TIndex, TProp>> BuildInstanceGetter<T, TIndex, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TIndex, TProp>>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex) });
        }
        public Expression<Func<T, TIndex1, TIndex2, TProp>> BuildInstanceGetter<T, TIndex1, TIndex2, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TIndex1, TIndex2, TProp>>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2) });
        }
        public Expression<Func<T, TIndex1, TIndex2, TIndex3, TProp>> BuildInstanceGetter<T, TIndex1, TIndex2, TIndex3, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TIndex1, TIndex2, TIndex3, TProp>>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3) });
        }
        public Expression<Func<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>> BuildInstanceGetter<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Func<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3), typeof(TIndex4) });
        }

        #endregion Indexed Getters

        #region Indexed Setters

        public Expression<Action<T, TIndex, TProp>> BuildInstanceSetter<T, TIndex, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TIndex, TProp>>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex) });
        }
        public Expression<Action<T, TIndex1, TIndex2, TProp>> BuildInstanceSetter<T, TIndex1, TIndex2, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TIndex1, TIndex2, TProp>>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2) });
        }
        public Expression<Action<T, TIndex1, TIndex2, TIndex3, TProp>> BuildInstanceSetter<T, TIndex1, TIndex2, TIndex3, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TIndex1, TIndex2, TIndex3, TProp>>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3) });
        }
        public Expression<Action<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>> BuildInstanceSetter<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>(PropertyInfo propertyInfo)
        {
            return (Expression<Action<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3), typeof(TIndex4) });
        }

        #endregion Indexed Setters
    }
}
