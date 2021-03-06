﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Expressions.LambdaBuilders;

namespace Dynamix.Reflection.DelegateBuilders
{
    public partial class ConstructorInvokerDelegateBuilder
    {
        #region Ctor

        public ConstructorInvokerDelegateBuilder()
        {
            builder = new ConstructorInvokerLambdaBuilder(EnableCaching);
        }

        public ConstructorInvokerDelegateBuilder(bool enableCaching)
        {
            this.EnableCaching = enableCaching;
            builder = new ConstructorInvokerLambdaBuilder(EnableCaching);
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
        static ConcurrentDictionary<HashKey, Delegate> cache = new ConcurrentDictionary<HashKey, Delegate>();
        static ConcurrentDictionary<Type, Delegate> byDelegateTypeCache = new ConcurrentDictionary<Type, Delegate>();
        static ConcurrentDictionary<ConstructorInfo, Delegate> genericCache = new ConcurrentDictionary<ConstructorInfo, Delegate>();

        public bool EnableCaching { get; set; }
        private readonly ConstructorInvokerLambdaBuilder builder;

        #endregion

        #region GenericBuild

        public GenericStaticInvoker BuildGeneric(ConstructorInfo ctorInfo)
        {
            if (EnableCaching
                && genericCache.TryGetValue(ctorInfo, out var cachedDelegate))
                return (GenericStaticInvoker)cachedDelegate;

            var delegate_ = builder.BuildGeneric(ctorInfo).Compile();

            if (EnableCaching)
                genericCache.TryAdd(ctorInfo, delegate_);

            return delegate_;
        }

        #endregion

        #region Typed Builders

        public Delegate BuildFromTypes(ConstructorInfo ctorInfo, Type instanceType = null, IEnumerable<Type> parameterTypes = null)
        {
            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(ctorInfo, instanceType, parameterTypes);
                if (cache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            var delegate_ = builder.BuildFromTypes(ctorInfo, instanceType, parameterTypes).Compile();

            if (EnableCaching)
                cache.TryAdd(cacheKey, delegate_);

            return delegate_;
        }

        public Delegate BuildFromDelegate(ConstructorInfo ctorInfo, Type delegateType)
        {
            if (delegateType == null)
                return BuildFromTypes(ctorInfo);

            if (EnableCaching && byDelegateTypeCache.TryGetValue(delegateType, out var cachedDelegate))
                    return cachedDelegate;
            
            var delegate_ = builder.BuildFromDelegate(ctorInfo, delegateType).Compile();

            if (EnableCaching)
                byDelegateTypeCache.TryAdd(delegateType, delegate_);

            return delegate_;
        }

        public TDelegate BuildFromDelegate<TDelegate>(ConstructorInfo ctorInfo)
            where TDelegate : class
        {
            return (TDelegate)Convert.ChangeType(BuildFromDelegate(ctorInfo, typeof(TDelegate)), typeof(Delegate));
        }


        public Func<TResult> Build<TResult>(ConstructorInfo ctorInfo)
        {
            return (Func<TResult>)BuildFromTypes(ctorInfo, typeof(TResult), null);
        }

        #endregion Typed Builders
    }
}
