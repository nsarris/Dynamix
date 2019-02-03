using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Expressions;
using System.Reflection.Emit;
using System.Collections.Concurrent;
using Dynamix.Reflection;

namespace Dynamix.Expressions.LambdaBuilders
{
    public class FieldAccessorLambdaBuilder
    {
        #region Ctor

        public FieldAccessorLambdaBuilder()
        {

        }

        public FieldAccessorLambdaBuilder(bool enableCaching)
        {
            this.EnableCaching = enableCaching;
        }

        #endregion Ctor

        #region Cache
        private class CacheKey : HashKey
        {
            public CacheKey(FieldInfo fieldInfo, Type instanceType, Type valueType)
                : base(new object[] { fieldInfo, instanceType, valueType })
            {
            }
        }
        static ConcurrentDictionary<HashKey, LambdaExpression> getterCache = new ConcurrentDictionary<HashKey, LambdaExpression>();
        static ConcurrentDictionary<HashKey, LambdaExpression> setterCache = new ConcurrentDictionary<HashKey, LambdaExpression>();
        static ConcurrentDictionary<FieldInfo, LambdaExpression> genericGetterCache = new ConcurrentDictionary<FieldInfo, LambdaExpression>();
        static ConcurrentDictionary<FieldInfo, LambdaExpression> genericSetterCache = new ConcurrentDictionary<FieldInfo, LambdaExpression>();

        private bool EnableCaching { get; set; } = true;

        #endregion

        #region Helpers

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

        private void ValidateField(FieldInfo fieldInfo, bool? isStatic)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            if (isStatic.HasValue && fieldInfo.IsStatic != isStatic)
                throw new FieldAccessException("Field is not " + ((isStatic == true) ? "static" : "instance"));
        }

        #endregion Helpers

        #region Untyped Builders

        public Expression<Func<object, object>> BuildGenericGetter(FieldInfo fieldInfo)
        {
            ValidateField(fieldInfo, null);

            if (EnableCaching
                && genericGetterCache.TryGetValue(fieldInfo, out var cachedLambda))
                return (Expression<Func<object, object>>)cachedLambda;

            var instanceParameter = Expression.Parameter(typeof(object), "instance");

            var invokerExpression =
                Expression.Field(
                fieldInfo.IsStatic ? null : ExpressionEx.ConvertIfNeeded(instanceParameter, fieldInfo.DeclaringType),
                fieldInfo);

            var body = ExpressionEx.CastTypeSafe(invokerExpression, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(body, instanceParameter);

            if (EnableCaching)
                genericGetterCache.TryAdd(fieldInfo, lambda);

            return lambda;
        }

        public Expression<Action<object, object>> BuildGenericSetter(FieldInfo fieldInfo)
        {
            ValidateField(fieldInfo, null);

            if (EnableCaching
                && genericSetterCache.TryGetValue(fieldInfo, out var cachedLambda))
                return (Expression<Action<object, object>>)cachedLambda;

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");
            var castedInstance = ExpressionEx.ConvertIfNeeded(instanceParameter, fieldInfo.DeclaringType);

            Expression<Action<object, object>> lambda;

            if (!fieldInfo.IsInitOnly)
            {
                lambda = Expression.Lambda<Action<object, object>>(
                    Expression.Assign(
                        Expression.Field(fieldInfo.IsStatic ? null : castedInstance, fieldInfo),
                        GetCastedValueExpression(value, fieldInfo.FieldType)),
                    instanceParameter, value);
            }
            else
            {
#if !NETSTANDARD2_0
                var m = Reflection.Emit.FieldAccessorMethodEmitter.GetFieldSetterMethod(fieldInfo, GenericTypeExtensions.GetActionGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType));
                lambda = Expression.Lambda<Action<object, object>>(
                    Expression.Call(m, castedInstance, GetCastedValueExpression(value, fieldInfo.FieldType)),
                    instanceParameter, value);
#else
                throw new InvalidOperationException($"Field {fieldInfo.Name} is InitOnly and cannot be set.");
#endif
            }

            if (EnableCaching)
                genericGetterCache.TryAdd(fieldInfo, lambda);

            return lambda;
        }

#endregion Untyped Builders

#region Instance Builders

        public LambdaExpression BuildInstanceGetter(FieldInfo fieldInfo, Type instanceType = null, Type valueType = null)
        {
            ValidateField(fieldInfo, false);

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(fieldInfo, instanceType, valueType);
                if (getterCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            instanceType = instanceType ?? fieldInfo.DeclaringType;
            valueType = valueType ?? fieldInfo.FieldType;

            var instance = Expression.Parameter(instanceType, "instance");
            var castedInstance = GetCastedInstanceExpression(instance, fieldInfo);

            var delegateType = GenericTypeExtensions.GetFuncGenericType(instanceType, valueType);

            var lambda = Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Field(castedInstance, fieldInfo), valueType),
                    instance);

            if (EnableCaching)
                getterCache.TryAdd(cacheKey, lambda);

            return lambda;
        }


        public LambdaExpression BuildInstanceSetter(FieldInfo fieldInfo, Type instanceType = null, Type valueType = null)
        {
            ValidateField(fieldInfo, false);

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(fieldInfo, instanceType, valueType);
                if (setterCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            instanceType = instanceType ?? fieldInfo.DeclaringType;
            valueType = valueType ?? fieldInfo.FieldType;

            var instance = Expression.Parameter(instanceType, "instance");
            var value = Expression.Parameter(valueType, "value");

            var expressionParameters = new[] { instance, value };

            var castedInstance = GetCastedInstanceExpression(instance, fieldInfo);

            var delegateType = GenericTypeExtensions.GetActionGenericType(instanceType, valueType);

            LambdaExpression lambda;

            if (!fieldInfo.IsInitOnly)
            {
                lambda = Expression.Lambda(
                    delegateType,
                    Expression.Assign(
                        Expression.Field(castedInstance, fieldInfo),
                        GetCastedValueExpression(value, fieldInfo.FieldType)),
                    expressionParameters);
            }
            else
            {
#if !NETSTANDARD2_0
                var m = Reflection.Emit.FieldAccessorMethodEmitter.GetFieldSetterMethod(fieldInfo, delegateType);
                lambda = Expression.Lambda(
                    delegateType,
                    Expression.Call(m, castedInstance, GetCastedValueExpression(value, fieldInfo.FieldType)),
                    expressionParameters);
#else
                throw new InvalidOperationException($"Field {fieldInfo.Name} is InitOnly and cannot be set.");
#endif
            }

            if (EnableCaching)
                setterCache.TryAdd(cacheKey, lambda);

            return lambda;
        }

#endregion Instance Builders

#region Static Builders

        public LambdaExpression BuildStaticGetter(FieldInfo fieldInfo, Type valueType = null)
        {
            ValidateField(fieldInfo, true);

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(fieldInfo, null, valueType);
                if (getterCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            var delegateType = GenericTypeExtensions.GetFuncGenericType(valueType ?? fieldInfo.FieldType);

            var lambda = Expression.Lambda(
                    delegateType,
                    GetCastedValueExpression(Expression.Field(null, fieldInfo), valueType));

            if (EnableCaching)
                getterCache.TryAdd(cacheKey, lambda);

            return lambda;
        }

        public LambdaExpression BuildStaticSetter(FieldInfo fieldInfo, Type valueType = null)
        {
            ValidateField(fieldInfo, true);

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(fieldInfo, null, valueType);
                if (setterCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            valueType = valueType ?? fieldInfo.FieldType;
            var value = Expression.Parameter(valueType, "value");
            var delegateType = GenericTypeExtensions.GetActionGenericType(valueType);

            LambdaExpression lambda;
            if (!fieldInfo.IsInitOnly)
            {
                lambda = Expression.Lambda(
                    delegateType,
                    Expression.Assign(
                        Expression.Field(null, fieldInfo),
                        GetCastedValueExpression(value, fieldInfo.FieldType)),
                    value);
            }
            else
            {
#if !NETSTANDARD2_0
                var m = Reflection.Emit.FieldAccessorMethodEmitter.GetFieldSetterMethod(fieldInfo, delegateType);
                lambda = Expression.Lambda(
                    delegateType,
                    Expression.Call(m, GetCastedValueExpression(value, fieldInfo.FieldType)),
                    value);
#else
                throw new InvalidOperationException($"Field {fieldInfo.Name} is InitOnly and cannot be set.");
#endif
            }

            if (EnableCaching)
                setterCache.TryAdd(cacheKey, lambda);

            return lambda;
        }

#endregion Static Builders

#region Instance Getters and Setters

        public Expression<Func<object, object>> BuildInstanceGetter(FieldInfo fieldInfo)
        {
            return (Expression<Func<object, object>>)BuildInstanceGetter(fieldInfo, typeof(object), typeof(object));
        }

        public Expression<Action<object, object>> BuildInstanceSetter(FieldInfo fieldInfo)
        {
            return (Expression<Action<object, object>>)BuildInstanceSetter(fieldInfo, typeof(object), typeof(object));
        }

        public Expression<Func<T, object>> BuildInstanceGetter<T>(FieldInfo fieldInfo)
        {
            return (Expression<Func<T, object>>)BuildInstanceGetter(fieldInfo, typeof(T), typeof(object));
        }

        public Expression<Action<T, object>> BuildInstanceSetter<T>(FieldInfo fieldInfo)
        {
            return (Expression<Action<T, object>>)BuildInstanceSetter(fieldInfo, typeof(T), typeof(object));
        }

        public Expression<Func<T, TField>> BuildInstanceGetter<T, TField>(FieldInfo fieldInfo)
        {
            return (Expression<Func<T, TField>>)BuildInstanceGetter(fieldInfo, typeof(T), typeof(TField));
        }

        public Expression<Action<T, TField>> BuildInstanceSetter<T, TField>(FieldInfo fieldInfo)
        {
            return (Expression<Action<T, TField>>)BuildInstanceSetter(fieldInfo, typeof(T), typeof(TField));
        }

#endregion Instance Getters and Setters

#region Static Getters and Setters

        public Expression<Func<object>> BuildStaticGetter(FieldInfo fieldInfo)
        {
            return (Expression<Func<object>>)BuildStaticGetter(fieldInfo, typeof(object));
        }

        public Expression<Action<object>> BuildStaticSetter(FieldInfo fieldInfo)
        {
            return (Expression<Action<object>>)BuildStaticSetter(fieldInfo, typeof(object));
        }

        public Expression<Func<TField>> BuildStaticGetter<TField>(FieldInfo fieldInfo)
        {
            return (Expression<Func<TField>>)BuildStaticGetter(fieldInfo, typeof(TField));
        }

        public Expression<Action<TField>> BuildStaticSetter<TField>(FieldInfo fieldInfo)
        {
            return (Expression<Action<TField>>)BuildStaticSetter(fieldInfo, typeof(TField));
        }

#endregion Static Getters and Setters
    }
}
