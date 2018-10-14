using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Expressions;
using System.Collections.Concurrent;
using Dynamix.Expressions.LambdaBuilders;

namespace Dynamix.Reflection.DelegateBuilders
{
    public class PropertyAccessorDelegateBuilder
    {
        #region Ctor

        public PropertyAccessorDelegateBuilder()
        {
            builder = new PropertyAccessorLambdaBuilder(EnableCaching);
        }

        public PropertyAccessorDelegateBuilder(bool enableCaching)
        {
            this.EnableCaching = enableCaching;
            builder = new PropertyAccessorLambdaBuilder(enableCaching);
        }

        #endregion Ctor

        #region Cache
        private class CacheKey : HashKey
        {
            public CacheKey(PropertyInfo propertyInfo, Type instanceType, Type valueType)
                : base(new object[] { propertyInfo, instanceType, valueType })
            {
            }
        }

        static ConcurrentDictionary<PropertyInfo, Delegate> genericGetterCache = new ConcurrentDictionary<PropertyInfo, Delegate>();
        static ConcurrentDictionary<PropertyInfo, Delegate> genericSetterCache = new ConcurrentDictionary<PropertyInfo, Delegate>();
        static ConcurrentDictionary<HashKey, Delegate> getterCache = new ConcurrentDictionary<HashKey, Delegate>();
        static ConcurrentDictionary<HashKey, Delegate> setterCache = new ConcurrentDictionary<HashKey, Delegate>();

        private bool EnableCaching { get; set; } = true;

        private PropertyAccessorLambdaBuilder builder;

        #endregion

        #region Generic Builders

        public GenericPropertyGetter BuildGenericGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (EnableCaching)
                if (genericGetterCache.TryGetValue(propertyInfo, out var cachedDelegate))
                    return (GenericPropertyGetter)cachedDelegate;

            var delegate_ = builder.BuildGenericGetter(propertyInfo).Compile();

            if (EnableCaching)
                genericGetterCache.TryAdd(propertyInfo, delegate_);

            return delegate_;
        }

        public GenericPropertySetter BuildGenericSetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (EnableCaching)
                if (genericSetterCache.TryGetValue(propertyInfo, out var cachedDelegate))
                    return (GenericPropertySetter)cachedDelegate;

            var delegate_ = builder.BuildGenericSetter(propertyInfo).Compile();

            if (EnableCaching)
                genericSetterCache.TryAdd(propertyInfo, delegate_);

            return delegate_;
        }

        #endregion

        #region Instance Builders

        public Delegate BuildInstanceGetter(PropertyInfo propertyInfo, Type instanceType = null, Type valueType = null, IEnumerable<Type> indexerTypes = null)
        {
            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(propertyInfo, instanceType, valueType);
                if (getterCache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            var _delegate = builder.BuildInstanceGetter(propertyInfo, instanceType, valueType).Compile();

            if (EnableCaching)
                getterCache.TryAdd(cacheKey, _delegate);

            return _delegate;
        }


        public Delegate BuildInstanceSetter(PropertyInfo propertyInfo, Type instanceType = null, Type valueType = null, IEnumerable<Type> indexerTypes = null)
        {
            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(propertyInfo, instanceType, valueType);
                if (setterCache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            var _delegate = builder.BuildInstanceSetter(propertyInfo, instanceType, valueType).Compile();

            if (EnableCaching)
                setterCache.TryAdd(cacheKey, _delegate);

            return _delegate;
        }

        #endregion Instance Builders

        #region Static Builders

        public Delegate BuildStaticGetter(PropertyInfo propertyInfo, Type valueType = null)
        {
            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(propertyInfo, null, valueType);
                if (getterCache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            var _delegate = builder.BuildStaticGetter(propertyInfo, valueType).Compile();

            if (EnableCaching)
                getterCache.TryAdd(cacheKey, _delegate);

            return _delegate;
        }


        public Delegate BuildStaticSetter(PropertyInfo propertyInfo, Type valueType = null)
        {
            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(propertyInfo, null, valueType);
                if (setterCache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            var _delegate = builder.BuildStaticSetter(propertyInfo, valueType).Compile();

            if (EnableCaching)
                setterCache.TryAdd(cacheKey, _delegate);

            return _delegate;
        }

        #endregion Static Builders

        #region Instance Getters and Setters

        public Func<T, object> BuildInstanceGetter<T>(PropertyInfo propertyInfo)
        {
            return (Func<T, object>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(object));
        }

        public Action<T, object> BuildInstanceSetter<T>(PropertyInfo propertyInfo)
        {
            return (Action<T, object>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(object));
        }

        public Func<T, TProp> BuildInstanceGetter<T, TProp>(PropertyInfo propertyInfo)
        {
            return (Func<T, TProp>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp));
        }

        public Action<T, TProp> BuildInstanceSetter<T, TProp>(PropertyInfo propertyInfo)
        {
            return (Action<T, TProp>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp));
        }

        #endregion Instance Getters and Setters

        #region Static Getters and Setters

        public Func<object> BuildStaticGetter(PropertyInfo propertyInfo)
        {
            return (Func<object>)BuildStaticGetter(propertyInfo, typeof(object));
        }

        public Action<object> BuildStaticSetter(PropertyInfo propertyInfo)
        {
            return (Action<object>)BuildStaticSetter(propertyInfo, typeof(object));
        }

        public Func<TProp> BuildStaticGetter<TProp>(PropertyInfo propertyInfo)
        {
            return (Func<TProp>)BuildStaticGetter(propertyInfo, typeof(TProp));
        }

        public Action<TProp> BuildStaticSetter<TProp>(PropertyInfo propertyInfo)
        {
            return (Action<TProp>)BuildStaticSetter(propertyInfo, typeof(TProp));
        }

        #endregion Static Getters and Setters

        #region Indexed Getters

        public Func<T, TIndex, TProp> BuildInstanceGetter<T, TIndex, TProp>(PropertyInfo propertyInfo)
        {
            return (Func<T, TIndex, TProp>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex) });
        }
        public Func<T, TIndex1, TIndex2, TProp> BuildInstanceGetter<T, TIndex1, TIndex2, TProp>(PropertyInfo propertyInfo)
        {
            return (Func<T, TIndex1, TIndex2, TProp>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2) });
        }
        public Func<T, TIndex1, TIndex2, TIndex3, TProp> BuildInstanceGetter<T, TIndex1, TIndex2, TIndex3, TProp>(PropertyInfo propertyInfo)
        {
            return (Func<T, TIndex1, TIndex2, TIndex3, TProp>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3) });
        }
        public Func<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp> BuildInstanceGetter<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>(PropertyInfo propertyInfo)
        {
            return (Func<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>)BuildInstanceGetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3), typeof(TIndex4) });
        }

        #endregion Indexed Getters

        #region Indexed Setters

        public Action<T, TIndex, TProp> BuildInstanceSetter<T, TIndex, TProp>(PropertyInfo propertyInfo)
        {
            return (Action<T, TIndex, TProp>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex) });
        }
        public Action<T, TIndex1, TIndex2, TProp> BuildInstanceSetter<T, TIndex1, TIndex2, TProp>(PropertyInfo propertyInfo)
        {
            return (Action<T, TIndex1, TIndex2, TProp>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2) });
        }
        public Action<T, TIndex1, TIndex2, TIndex3, TProp> BuildInstanceSetter<T, TIndex1, TIndex2, TIndex3, TProp>(PropertyInfo propertyInfo)
        {
            return (Action<T, TIndex1, TIndex2, TIndex3, TProp>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3) });
        }
        public Action<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp> BuildInstanceSetter<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>(PropertyInfo propertyInfo)
        {
            return (Action<T, TIndex1, TIndex2, TIndex3, TIndex4, TProp>)BuildInstanceSetter(propertyInfo, typeof(T), typeof(TProp), new[] { typeof(TIndex1), typeof(TIndex2), typeof(TIndex3), typeof(TIndex4) });
        }

        #endregion Indexed Setters
    }
}
