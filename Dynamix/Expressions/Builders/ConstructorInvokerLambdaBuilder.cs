using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Reflection;

namespace Dynamix.Expressions.LambdaBuilders
{
    public class ConstructorInvokerLambdaBuilder
    {
        #region Ctor

        public ConstructorInvokerLambdaBuilder()
        {

        }

        public ConstructorInvokerLambdaBuilder(bool enableCaching)
        {
            this.EnableCaching = enableCaching;
        }

        #endregion Ctor

        #region Cache
        private class CacheKey : HashKey
        {
            public CacheKey(ConstructorInfo ctor, Type instanceType, IEnumerable<Type> parameterTypes)
                : base(new object[] { ctor, instanceType }.Concat(parameterTypes == null ? Enumerable.Empty<object>() : parameterTypes.Cast<object>()))
            {
            }
        }
        static ConcurrentDictionary<HashKey, LambdaExpression> cache = new ConcurrentDictionary<HashKey, LambdaExpression>();
        static ConcurrentDictionary<Type, LambdaExpression> byDelegateTypeCache = new ConcurrentDictionary<Type, LambdaExpression>();
        static ConcurrentDictionary<ConstructorInfo, LambdaExpression> genericCache = new ConcurrentDictionary<ConstructorInfo, LambdaExpression>();

        private bool EnableCaching { get; set; } = true;

        #endregion

        #region GenericBuild

        public Expression<GenericStaticInvoker> BuildGeneric(ConstructorInfo ctorInfo)
        {
            if (ctorInfo == null)
                throw new ArgumentNullException(nameof(ctorInfo));

            if (EnableCaching && genericCache.TryGetValue(ctorInfo, out var cachedLambda))
                return (Expression<GenericStaticInvoker>)cachedLambda;
            
            var inputParameters = Expression.Parameter(typeof(object[]), "parameters");

            var methodParameters = ctorInfo.GetParameters();
            var methodCallParameters = methodParameters
                .Select((x, i) => ExpressionEx.ConvertIfNeeded(
                            Expression.ArrayAccess(inputParameters, Expression.Constant(i)),
                            x.ParameterType));

            var invokerExpression = Expression.New(
                ctorInfo,
                methodCallParameters);

            var body = ExpressionEx.CastTypeSafe(invokerExpression, typeof(object));

            var lambda = Expression.Lambda<GenericStaticInvoker>(body, new[] { inputParameters });

            if (EnableCaching)
                genericCache.TryAdd(ctorInfo, lambda);

            return lambda;
        }

        #endregion

        #region Typed Builders

        private LambdaExpression BuildFromTypes(ConstructorInfo ctorInfo, Type instanceType, IEnumerable<Type> parameterTypes, Type delegateType)
        {
            CacheKey cacheKey = null;
            if (EnableCaching && delegateType == null)
            {
                cacheKey = new CacheKey(ctorInfo, instanceType, parameterTypes);
                if (cache.TryGetValue(cacheKey, out var lambda))
                    return lambda;
            }

            var effectiveInstanceType = instanceType ?? ctorInfo.DeclaringType;

            var constructorParameters = ctorInfo.GetParameters()
                .ZipOutter(parameterTypes ?? Enumerable.Empty<Type>(),
                (cp, p) => new
                {
                    MethodType = cp.ParameterType,
                    Name = cp.Name,
                    InputParameter = Expression.Parameter(p ?? cp.ParameterType, cp.Name),
                })
                .Select(x => new
                {
                    x.InputParameter,
                    MethodCallParameter = x.MethodType == x.InputParameter.Type ? (Expression)x.InputParameter : ExpressionEx.ConvertIfNeeded(x.InputParameter, x.MethodType)
                })
                .ToList();

            var effectiveDelegateType = delegateType ?? GenericTypeExtensions.GetFuncGenericType(constructorParameters.Select(x => x.InputParameter.Type).Concat(new[] { effectiveInstanceType }));
            var invokerExpression = (Expression)Expression.New(ctorInfo, constructorParameters.Select(x => x.MethodCallParameter));
            if (effectiveInstanceType != ctorInfo.DeclaringType)
                invokerExpression = Expression.Convert(invokerExpression, effectiveInstanceType);

            var l = Expression.Lambda(effectiveDelegateType, invokerExpression, constructorParameters.Select(x => x.InputParameter).ToArray());

            if (EnableCaching && delegateType == null)
                cache.TryAdd(cacheKey, l);

            return l;
        }

        public LambdaExpression BuildFromTypes(ConstructorInfo ctorInfo, Type instanceType = null, IEnumerable<Type> parameterTypes = null)
        {
            return BuildFromTypes(ctorInfo, instanceType, parameterTypes, null);
        }

        public LambdaExpression BuildFromDelegate(ConstructorInfo ctorInfo, Type delegateType)
        {
            if (delegateType == null)
                return BuildFromTypes(ctorInfo);

            if (ctorInfo == null)
                throw new ArgumentNullException(nameof(ctorInfo));

            if (EnableCaching && byDelegateTypeCache.TryGetValue(delegateType, out var cachedLambda))
                return cachedLambda;
            
            var parameterTypes = ctorInfo.GetParameters().Select(x => x.ParameterType);
            Type instanceType = ctorInfo.DeclaringType;

            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException(delegateType.Name + " is not a delegate type");

            var invokeMethod = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            if (invokeMethod == null)
                throw new ArgumentException(delegateType.Name + " delegate type has no invoke method.");

            var invokeMethodParameters = invokeMethod.GetParameters();

            parameterTypes = invokeMethodParameters.Select(x => x.ParameterType);

            var lambda = BuildFromTypes(ctorInfo, instanceType, parameterTypes, delegateType);

            if (EnableCaching)
                byDelegateTypeCache.TryAdd(delegateType, lambda);

            return lambda;
        }

        public Expression<TDelegate> BuildFromDelegate<TDelegate>(ConstructorInfo ctorInfo)
        {
            return (Expression<TDelegate>)BuildFromDelegate(ctorInfo, typeof(TDelegate));
        }

        //No parameters

        public Expression<Func<TResult>> Build<TResult>(ConstructorInfo ctorInfo)
        {
            return (Expression<Func<TResult>>)BuildFromTypes(ctorInfo, typeof(TResult), null);
        }

        //One parameter
        public Expression<Func<TParam1, TResult>> Build<TParam1, TResult>(ConstructorInfo ctorInfo)
        {
            return (Expression<Func<TParam1, TResult>>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1) });
        }

        //Two parameters
        public Expression<Func<TParam1, TParam2, TResult>> Build<TParam1, TParam2, TResult>(ConstructorInfo ctorInfo)
        {
            return (Expression<Func<TParam1, TParam2, TResult>>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2) });
        }

        //Three parameters
        public Expression<Func<TParam1, TParam2, TParam3, TResult>> Build<TParam1, TParam2, TParam3, TResult>(ConstructorInfo ctorInfo)
        {
            return (Expression<Func<TParam1, TParam2, TParam3, TResult>>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) });
        }

        #endregion Typed Builders
    }
}
