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
using System.Collections.Concurrent;
using Dynamix.Expressions.LambdaBuilders;

namespace Dynamix.Reflection.DelegateBuilders
{
    public class FieldAccessorDelegateBuilder
    {
        #region Ctor

        public FieldAccessorDelegateBuilder()
        {
            builder = new FieldAccessorLambdaBuilder(EnableCaching);
        }

        public FieldAccessorDelegateBuilder(bool enableCaching)
        {
            this.EnableCaching = enableCaching;
            builder = new FieldAccessorLambdaBuilder(enableCaching);
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
        static ConcurrentDictionary<HashKey, Delegate> getterCache = new ConcurrentDictionary<HashKey, Delegate>();
        static ConcurrentDictionary<HashKey, Delegate> setterCache = new ConcurrentDictionary<HashKey, Delegate>();
        static ConcurrentDictionary<FieldInfo, Delegate> genericGetterCache = new ConcurrentDictionary<FieldInfo, Delegate>();
        static ConcurrentDictionary<FieldInfo, Delegate> genericSetterCache = new ConcurrentDictionary<FieldInfo, Delegate>();

        private bool EnableCaching { get; set; } = true;
        private readonly FieldAccessorLambdaBuilder builder;

        #endregion

        #region Generic Builders

        public Func<object,object> BuildGenericGetter(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            if (EnableCaching)
                if (genericGetterCache.TryGetValue(fieldInfo, out var cachedDelegate))
                    return (Func<object, object>)cachedDelegate;

            var delegate_ = builder.BuildGenericGetter(fieldInfo).Compile();

            if (EnableCaching)
                genericGetterCache.TryAdd(fieldInfo, delegate_);

            return delegate_;
        }

        public Action<object, object> BuildGenericSetter(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            if (EnableCaching)
                if (genericSetterCache.TryGetValue(fieldInfo, out var cachedDelegate))
                    return (Action<object, object>)cachedDelegate;

            var delegate_ = builder.BuildGenericSetter(fieldInfo).Compile();

            if (EnableCaching)
                genericSetterCache.TryAdd(fieldInfo, delegate_);

            return delegate_;
        }

        #endregion

        #region Instance Builders

        public Delegate BuildInstanceGetter(FieldInfo fieldInfo, Type instanceType = null, Type valueType = null)
        {
            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(fieldInfo, instanceType, valueType);
                if (getterCache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            var _delegate = builder.BuildInstanceGetter(fieldInfo, instanceType, valueType).Compile();

            if (EnableCaching)
                getterCache.TryAdd(cacheKey, _delegate);

            return _delegate;
        }


        public Delegate BuildInstanceSetter(FieldInfo fieldInfo, Type instanceType = null, Type valueType = null)
        {
            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(fieldInfo, instanceType, valueType);
                if (setterCache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            var _delegate = builder.BuildInstanceSetter(fieldInfo, instanceType, valueType).Compile();

            if (EnableCaching)
                setterCache.TryAdd(cacheKey, _delegate);

            return _delegate;
        }

        #endregion Instance Builders

        #region Static Builders

        public Delegate BuildStaticGetter(FieldInfo fieldInfo, Type valueType = null)
        {
            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(fieldInfo, null, valueType);
                if (getterCache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            Delegate _delegate;
            if (!fieldInfo.IsInitOnly)
                _delegate = builder.BuildStaticGetter(fieldInfo, valueType).Compile();
            else
                _delegate = FieldAccessorMethodEmitter.GetFieldSetter(fieldInfo, GenericTypeExtensions.GetActionGenericType(valueType ?? fieldInfo.FieldType));

            if (EnableCaching)
                getterCache.TryAdd(cacheKey, _delegate);

            return _delegate;
        }

        public Delegate BuildStaticSetter(FieldInfo fieldInfo, Type valueType = null)
        {
            CacheKey cacheKey = null;
            if (EnableCaching)
            {
                cacheKey = new CacheKey(fieldInfo, null, valueType);
                if (setterCache.TryGetValue(cacheKey, out var cachedDelegate))
                    return cachedDelegate;
            }

            var _delegate = builder.BuildStaticSetter(fieldInfo, valueType).Compile();

            if (EnableCaching)
                setterCache.TryAdd(cacheKey, _delegate);

            return _delegate;
        }

        #endregion Static Builders

        #region Instance Getters and Setters

        public Func<object, object> BuildInstanceGetter(FieldInfo fieldInfo)
        {
            return (Func<object, object>)BuildInstanceGetter(fieldInfo, typeof(object), typeof(object));
        }

        public Action<object, object> BuildInstanceSetter(FieldInfo fieldInfo)
        {
            return (Action<object, object>)BuildInstanceSetter(fieldInfo, typeof(object), typeof(object));
        }

        public Func<T, object> BuildInstanceGetter<T>(FieldInfo fieldInfo)
        {
            return (Func<T, object>)BuildInstanceGetter(fieldInfo, typeof(T), typeof(object));
        }

        public Action<T, object> BuildInstanceSetter<T>(FieldInfo fieldInfo)
        {
            return (Action<T, object>)BuildInstanceSetter(fieldInfo, typeof(T), typeof(object));
        }

        public Func<T, TField> BuildInstanceGetter<T, TField>(FieldInfo fieldInfo)
        {
            return (Func<T, TField>)BuildInstanceGetter(fieldInfo, typeof(T), typeof(TField));
        }

        public Action<T, TField> BuildInstanceSetter<T, TField>(FieldInfo fieldInfo)
        {
            return (Action<T, TField>)BuildInstanceSetter(fieldInfo, typeof(T), typeof(TField));
        }

        #endregion Instance Getters and Setters

        #region Static Getters and Setters

        public Func<object> BuildStaticGetter(FieldInfo fieldInfo)
        {
            return (Func<object>)BuildStaticGetter(fieldInfo, typeof(object));
        }

        public Action<object> BuildStaticSetter(FieldInfo fieldInfo)
        {
            return (Action<object>)BuildStaticSetter(fieldInfo, typeof(object));
        }

        public Func<TField> BuildStaticGetter<TField>(FieldInfo fieldInfo)
        {
            return (Func<TField>)BuildStaticGetter(fieldInfo, typeof(TField));
        }

        public Action<TField> BuildStaticSetter<TField>(FieldInfo fieldInfo)
        {
            return (Action<TField>)BuildStaticSetter(fieldInfo, typeof(TField));
        }

        #endregion Static Getters and Setters
    }
}
