using System;
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
    public delegate object GenericInstanceInvoker(object instance, params object[] parameters);
    public delegate object GenericStaticInvoker(params object[] parameters);
    public class MethodInvokerLambdaBuilder
    {
        #region Ctor

        public MethodInvokerLambdaBuilder()
        {

        }

        public MethodInvokerLambdaBuilder(bool enableCaching)
        {
            this.EnableCaching = enableCaching;
        }

        #endregion Ctor

        #region Cache
        private class CacheKey : HashKey
        {
            public CacheKey(MethodInfo methodInfo, Type instanceType, Type returnType, IEnumerable<Type> parameterTypes)
                : base(new object[] { methodInfo, instanceType }.Concat(parameterTypes == null ? Enumerable.Empty<object>() : parameterTypes.Cast<object>()))
            { }

            public CacheKey(Type delegateType) : base(new[] { delegateType })
            { }

            public CacheKey(MethodInfo methodInfo, bool asExtension) : base(new object[] { methodInfo, asExtension })
            { }
        }
        static ConcurrentDictionary<HashKey, LambdaExpression> byTypesCache = new ConcurrentDictionary<HashKey, LambdaExpression>();
        static ConcurrentDictionary<Type, LambdaExpression> byDelegateTypeCache = new ConcurrentDictionary<Type, LambdaExpression>();
        static ConcurrentDictionary<HashKey, LambdaExpression> genericCache = new ConcurrentDictionary<HashKey, LambdaExpression>();

        private bool EnableCaching { get; set; } = true;

        #endregion

        #region Generic Builders
        public Expression<GenericStaticInvoker> BuildGenericStatic(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (!methodInfo.IsStatic)
                throw new ArgumentException("Method is not static", nameof(methodInfo));

            return (Expression<GenericStaticInvoker>)BuildGenericInvoker(methodInfo);
        }

        public Expression<GenericInstanceInvoker> BuildGenericInstance(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            //if (methodInfo.IsStatic)
            //    throw new ArgumentException("Method is not instance", nameof(methodInfo));

            return (Expression<GenericInstanceInvoker>)BuildGenericInvoker(methodInfo, true);
        }


        internal LambdaExpression BuildGenericInvoker(MethodInfo methodInfo, bool asInstance = false)
        {
            //if (asExtension && !methodInfo.IsExtension())
            //    throw new ArgumentException("Method is not an extension method", nameof(methodInfo));

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(methodInfo, asInstance);
                if (genericCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var inputParameters = Expression.Parameter(typeof(object[]), "parameters");

            var methodParameters = methodInfo.GetParameters();

            var methodInstanceParameters = methodParameters
                .Skip(asInstance && methodInfo.IsExtension() ? 1 : 0)
                .ToList();

            var byRefVariables = methodInstanceParameters
                .Select((x, i) => new { Index = i, Parameter = x })
                .Where(x => x.Parameter.ParameterType.IsByRef)
                .Select(x => new
                {
                    x.Index,
                    Variable = Expression.Variable(x.Parameter.ParameterType.GetElementType(), x.Parameter.Name)
                })
                .ToArray();

            var methodCallParameters =
                methodInstanceParameters
                .Select((x, i) => ExpressionEx.ConvertIfNeeded(
                            x.ParameterType.IsByRef ?
                            (Expression)byRefVariables.Single(v => v.Index == i).Variable :
                            (Expression)Expression.ArrayAccess(inputParameters, Expression.Constant(i)),
                            x.ParameterType))
                .ToList();

            Expression invokerExpression;
            ParameterExpression[] expressionParameters;
            Type delegateType;

            //Instance
            if (!methodInfo.IsStatic)
            {
                invokerExpression = Expression.Call(
                ExpressionEx.ConvertIfNeeded(instanceParameter, methodInfo.DeclaringType),
                methodInfo,
                methodCallParameters);

                expressionParameters = new[] { instanceParameter, inputParameters };
                delegateType = typeof(GenericInstanceInvoker);
            }
            //Extension as Instance
            else if (asInstance && methodInfo.IsExtension())
            {
                invokerExpression = Expression.Call(
                    methodInfo,
                    new Expression[] { ExpressionEx.ConvertIfNeeded(instanceParameter, methodParameters[0].ParameterType) }.Concat(methodCallParameters));

                expressionParameters = new[] { instanceParameter, inputParameters };
                delegateType = typeof(GenericInstanceInvoker);
            }
            //Static as Instance
            else if (asInstance && methodInfo.IsStatic)
            {
                invokerExpression = Expression.Call(
                    methodInfo,
                    methodCallParameters);

                expressionParameters = new[] { instanceParameter, inputParameters };
                delegateType = typeof(GenericInstanceInvoker);
            }
            //Static
            else
            {
                invokerExpression = Expression.Call(
                methodInfo,
                methodCallParameters);

                expressionParameters = new[] { inputParameters };
                delegateType = typeof(GenericStaticInvoker);
            }

            
            var isFunc = methodInfo.ReturnType != typeof(void);

            
                
            var byRefAssignmentsFromArray = byRefVariables
                .Select(x => (Expression)Expression.Assign(x.Variable, 
                    ExpressionEx.ConvertIfNeeded(
                        Expression.ArrayAccess(inputParameters, Expression.Constant(x.Index)), x.Variable.Type)));

            var byRefAssignmentsToArray = byRefVariables
                .Select(x => (Expression)Expression.Assign(Expression.ArrayAccess(inputParameters, Expression.Constant(x.Index)), ExpressionEx.ConvertIfNeeded(x.Variable, typeof(object))));

            var returnVariable = isFunc && byRefVariables.Any() ? Expression.Variable(methodInfo.ReturnType) : null;

            var declaredVariables =
                (returnVariable != null ? new[] { returnVariable } : Enumerable.Empty<ParameterExpression>())
                .Concat(byRefVariables.Select(x => x.Variable))
                .ToArray();

            var invokerBody = Enumerable.Empty<Expression>()
                .Concat(byRefAssignmentsFromArray)
                .Concat(returnVariable != null ? new[] { Expression.Assign(returnVariable, invokerExpression) } : new[] { invokerExpression })
                .Concat(byRefAssignmentsToArray)
                .Concat(returnVariable != null ? new[] { returnVariable } : Enumerable.Empty<Expression>());
               
            if (!isFunc)
                invokerBody.Concat(new Expression[] { Expression.Constant(null) });

            //If is Action, return null as object
            //Else if is Func return actual type and cast to object
            var body = (!isFunc) ?
                    Expression.Block(typeof(object), declaredVariables, invokerBody) :
                    ExpressionEx.ConvertIfNeeded(
                        Expression.Block(methodInfo.ReturnType, declaredVariables, invokerBody), 
                        typeof(object));

            
            var lambda = Expression.Lambda(delegateType, body, expressionParameters);

            if (EnableCaching)
                genericCache.TryAdd(cacheKey, lambda);

            return lambda;
        }

        #endregion Generic Builders

        #region Typed Builders
        private class MethodParameter
        {
            public Type MethodType { get; set; }
            public ParameterExpression InputParameter { get; set; }
        }

        private LambdaExpression BuildInvokerExpressionFromTypes(MethodInfo methodInfo, Type instanceType, IEnumerable<Type> parameterTypes, Type returnType, Type delegateType)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            CacheKey cacheKey = null;
            if (EnableCaching && delegateType != null)
            {
                cacheKey = new CacheKey(methodInfo, instanceType, returnType, parameterTypes);
                if (byTypesCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            returnType = returnType ?? methodInfo.ReturnType;
            instanceType = methodInfo.IsStatic ? null : instanceType ?? methodInfo.DeclaringType;

            var methodParameters =
                    (methodInfo.IsStatic ?
                        Enumerable.Empty<MethodParameter>() :
                        new[] { new MethodParameter { InputParameter = Expression.Parameter(instanceType, "instance"), MethodType = methodInfo.DeclaringType } })
                    .Concat(methodInfo.GetParameters().ZipOutter(parameterTypes ?? Enumerable.Empty<Type>(),
                        (mp, pt) => new MethodParameter
                        {
                            InputParameter = Expression.Parameter(pt ?? mp.ParameterType, mp.Name),
                            MethodType = mp.ParameterType
                        }))
                        .Select(x => new
                        {
                            x.InputParameter,
                            MethodCallParameter = x.MethodType == x.InputParameter.Type ? (Expression)x.InputParameter : ExpressionEx.ConvertIfNeeded(x.InputParameter, x.MethodType)
                        })
                        .ToList();

            var isAction = methodInfo.ReturnType == typeof(void) && (returnType == null || returnType == typeof(void));

            var effectiveDelegateType = delegateType ?? (isAction ?
                    GenericTypeExtensions.GetActionGenericType(methodParameters.Select(x => x.MethodCallParameter.Type)) :
                    GenericTypeExtensions.GetFuncGenericType(methodParameters.Select(x => x.MethodCallParameter.Type).Concat(new[] { returnType }).ToArray()));

            var invokerExpression = Expression.Call(
                methodInfo.IsStatic ? null : methodParameters.Select(x => x.MethodCallParameter).First(),
                methodInfo,
                methodParameters.Select(x => x.MethodCallParameter).Skip(methodInfo.IsStatic ? 0 : 1));

            var body = methodInfo.ReturnType == typeof(void) ?
                        (!isAction ?
                        (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                        invokerExpression) :
                    Expression.Convert(invokerExpression, returnType);

            var lambda = Expression.Lambda(effectiveDelegateType, body, methodParameters.Select(x => x.InputParameter));

            if (EnableCaching && delegateType != null)
                byTypesCache.TryAdd(cacheKey, lambda);

            return lambda;
        }

        public LambdaExpression BuildFromTypes(MethodInfo methodInfo, Type instanceType = null, IEnumerable<Type> parameterTypes = null, Type returnType = null)
        {
            return BuildInvokerExpressionFromTypes(methodInfo, instanceType, parameterTypes, returnType, null);
        }

        public LambdaExpression BuildFromDelegate(MethodInfo methodInfo, Type delegateType)
        {
            if (delegateType == null)
                return BuildFromTypes(methodInfo);

            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (EnableCaching)
            {
                if (byDelegateTypeCache.TryGetValue(delegateType, out var cachedLambda))
                    return cachedLambda;
            }

            var instanceType = methodInfo.IsStatic ? null : methodInfo.DeclaringType;
            var parameterTypes = methodInfo.GetParameters().Select(x => x.ParameterType);
            Type returnType = methodInfo.ReturnType;

            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException(delegateType.Name + " is not a delegate type");

            var invokeMethod = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            if (invokeMethod == null)
                throw new ArgumentException(delegateType.Name + " delegate type has no invoke method.");

            var invokeMethodParameters = invokeMethod.GetParameters();

            instanceType = methodInfo.IsStatic ? null : invokeMethodParameters.Select(x => x.ParameterType).FirstOrDefault();
            parameterTypes = invokeMethodParameters.Skip(methodInfo.IsStatic ? 0 : 1).Select(x => x.ParameterType);

            var lambda = BuildInvokerExpressionFromTypes(methodInfo, instanceType, parameterTypes, returnType, delegateType);

            if (EnableCaching)
                byDelegateTypeCache.TryAdd(delegateType, lambda);

            return lambda;
        }

        public Expression<TDelegate> BuildFromDelegate<TDelegate>(MethodInfo methodInfo)
        {
            return (Expression<TDelegate>)BuildFromDelegate(methodInfo, typeof(TDelegate));
        }

        /// INSTANCE ///
        //No parameters

        public Expression<Action<T>> BuildActionInstance<T>(MethodInfo methodInfo)
        {
            return (Expression<Action<T>>)BuildFromTypes(methodInfo, typeof(T), null, typeof(void));
        }
        public Expression<Func<T, TResult>> BuildFuncInstance<T, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<T, TResult>>)BuildFromTypes(methodInfo, typeof(T), null, typeof(TResult));
        }

        //One parameter
        public Expression<Action<T, TParam1>> BuildActionInstance<T, TParam1>(MethodInfo methodInfo)
        {
            return (Expression<Action<T, TParam1>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1) }, typeof(void));
        }
        public Expression<Func<T, TParam1, TResult>> BuildFuncInstance<T, TParam1, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<T, TParam1, TResult>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1) }, typeof(TResult));
        }

        //Two parameters
        public Expression<Action<T, TParam1, TParam2>> BuildActionInstance<T, TParam1, TParam2>(MethodInfo methodInfo)
        {
            return (Expression<Action<T, TParam1, TParam2>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2) }, typeof(void));
        }
        public Expression<Func<T, TParam1, TParam2, TResult>> BuildFuncInstance<T, TParam1, TParam2, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<T, TParam1, TParam2, TResult>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2) }, typeof(TResult));
        }

        //Three parameters
        public Expression<Action<T, TParam1, TParam2, TParam3>> BuildActionInstance<T, TParam1, TParam2, TParam3>(MethodInfo methodInfo)
        {
            return (Expression<Action<T, TParam1, TParam2, TParam3>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(void));
        }
        public Expression<Func<T, TParam1, TParam2, TParam3, TResult>> BuildFuncInstance<T, TParam1, TParam2, TParam3, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<T, TParam1, TParam2, TParam3, TResult>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(TResult));
        }


        // STATIC ///
        //No parameters

        public Expression<Action> BuildActionStatic(MethodInfo methodInfo)
        {
            return (Expression<Action>)BuildFromTypes(methodInfo, null, null, typeof(void));
        }
        public Expression<Func<TResult>> BuildFuncStatic<TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TResult>>)BuildFromTypes(methodInfo, null, null, typeof(TResult));
        }

        //One parameter
        public Expression<Action<TParam1>> BuildActionStatic<TParam1>(MethodInfo methodInfo)
        {
            return (Expression<Action<TParam1>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1) }, typeof(void));
        }
        public Expression<Func<TParam1, TResult>> BuildFuncStatic<TParam1, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TParam1, TResult>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1) }, typeof(TResult));
        }

        //Two parameters
        public Expression<Action<TParam1, TParam2>> BuildActionStatic<TParam1, TParam2>(MethodInfo methodInfo)
        {
            return (Expression<Action<TParam1, TParam2>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2) }, typeof(void));
        }
        public Expression<Func<TParam1, TParam2, TResult>> BuildFuncStatic<TParam1, TParam2, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TParam1, TParam2, TResult>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2) }, typeof(TResult));
        }

        //Three parameters
        public Expression<Action<TParam1, TParam2, TParam3>> BuildActionStatic<TParam1, TParam2, TParam3>(MethodInfo methodInfo)
        {
            return (Expression<Action<TParam1, TParam2, TParam3>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(void));
        }
        public Expression<Func<TParam1, TParam2, TParam3, TResult>> BuildFuncStatic<TParam1, TParam2, TParam3, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TParam1, TParam2, TParam3, TResult>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(TResult));
        }

        #endregion Typed Builders
    }
}