using Dynamix.Expressions.LambdaBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection.DelegateBuilders
{


    public class MethodInvokerDelegateBuilder
    {
        #region Ctor

        public MethodInvokerDelegateBuilder()
        {
            builder = new MethodInvokerLambdaBuilder(EnableCaching);
        }

        public MethodInvokerDelegateBuilder(bool enableCaching)
        {
            this.EnableCaching = enableCaching;
            builder = new MethodInvokerLambdaBuilder(enableCaching);
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
        static ConcurrentDictionary<HashKey, Delegate> byTypesCache = new ConcurrentDictionary<HashKey, Delegate>();
        static ConcurrentDictionary<Type, Delegate> byDelegateTypeCache = new ConcurrentDictionary<Type, Delegate>();
        static ConcurrentDictionary<HashKey, Delegate> genericCache = new ConcurrentDictionary<HashKey, Delegate>();

        private bool EnableCaching { get; set; } = true;

        private MethodInvokerLambdaBuilder builder;

        #endregion

        #region Generic Builders
        public GenericStaticInvoker BuildGenericStatic(MethodInfo MethodInfo)
        {
            if (!MethodInfo.IsStatic)
                throw new ArgumentException("Method is not static", nameof(MethodInfo));

            return (GenericStaticInvoker)BuildGenericInvoker(MethodInfo);
        }

        public GenericInstanceInvoker BuildGenericInstance(MethodInfo MethodInfo)
        {
            //if (MethodInfo.IsStatic)
            //throw new ArgumentException("Method is not instance", nameof(MethodInfo));

            return (GenericInstanceInvoker)BuildGenericInvoker(MethodInfo, true);
        }


        private Delegate BuildGenericInvoker(MethodInfo methodInfo, bool asInstance = false)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(methodInfo, asInstance);
                if (genericCache.TryGetValue(cacheKey, out var cachedLambda))
                    return cachedLambda;
            }

            var delegate_ = builder.BuildGenericInvoker(methodInfo, asInstance).Compile();

            if (EnableCaching)
                genericCache.TryAdd(cacheKey, delegate_);

            return delegate_;
        }

        #endregion Generic Builders

        #region Typed Builders

        public Delegate BuildFromTypes(MethodInfo methodInfo, Type instanceType = null, IEnumerable<Type> parameterTypes = null, Type returnType = null)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(methodInfo, instanceType, returnType, parameterTypes);
                if (byTypesCache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            var delegate_ = builder.BuildFromTypes(methodInfo, instanceType, parameterTypes, returnType).Compile();

            if (EnableCaching)
                byTypesCache.TryAdd(cacheKey, delegate_);

            return delegate_;
        }

        public Delegate BuildFromDelegate(MethodInfo methodInfo, Type delegateType)
        {
            if (delegateType == null)
                return BuildFromTypes(methodInfo);

            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (EnableCaching)
            {
                if (byDelegateTypeCache.TryGetValue(delegateType, out var cachedDelegate))
                    return cachedDelegate;
            }

            var delegate_ = builder.BuildFromDelegate(methodInfo, delegateType).Compile();

            if (EnableCaching)
                byDelegateTypeCache.TryAdd(delegateType, delegate_);

            return delegate_;
        }

        public TDelegate BuildFromDelegate<TDelegate>(MethodInfo methodInfo)
            where TDelegate : class
        {
            return (TDelegate)(object)BuildFromDelegate(methodInfo, typeof(TDelegate));
        }

        /// INSTANCE ///
        //No parameters

        public Action<T> BuildActionInstance<T>(MethodInfo methodInfo)
        {
            return (Action<T>)BuildFromTypes(methodInfo, typeof(T), null, typeof(void));
        }
        public Func<T, TResult> BuildFuncInstance<T, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TResult>)BuildFromTypes(methodInfo, typeof(T), null, typeof(TResult));
        }

        //One parameter
        public Action<T, TParam1> BuildActionInstance<T, TParam1>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1) }, typeof(void));
        }
        public Func<T, TParam1, TResult> BuildFuncInstance<T, TParam1, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1) }, typeof(TResult));
        }

        //Two parameters
        public Action<T, TParam1, TParam2> BuildActionInstance<T, TParam1, TParam2>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2) }, typeof(void));
        }
        public Func<T, TParam1, TParam2, TResult> BuildFuncInstance<T, TParam1, TParam2, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2) }, typeof(TResult));
        }

        //Three parameters
        public Action<T, TParam1, TParam2, TParam3> BuildActionInstance<T, TParam1, TParam2, TParam3>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(void));
        }
        public Func<T, TParam1, TParam2, TParam3, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(TResult));
        }


        // STATIC ///
        //No parameters

        public Action BuildActionStatic(MethodInfo methodInfo)
        {
            return (Action)BuildFromTypes(methodInfo, null, null, typeof(void));
        }
        public Func<TResult> BuildFuncStatic<TResult>(MethodInfo methodInfo)
        {
            return (Func<TResult>)BuildFromTypes(methodInfo, null, null, typeof(TResult));
        }

        //One parameter
        public Action<TParam1> BuildActionStatic<TParam1>(MethodInfo methodInfo)
        {
            return (Action<TParam1>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1) }, typeof(void));
        }
        public Func<TParam1, TResult> BuildFuncStatic<TParam1, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1) }, typeof(TResult));
        }

        //Two parameters
        public Action<TParam1, TParam2> BuildActionStatic<TParam1, TParam2>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2) }, typeof(void));
        }
        public Func<TParam1, TParam2, TResult> BuildFuncStatic<TParam1, TParam2, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2) }, typeof(TResult));
        }

        //Three parameters
        public Action<TParam1, TParam2, TParam3> BuildActionStatic<TParam1, TParam2, TParam3>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(TResult));
        }

        #endregion Typed Builders
    }
}