using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection.ComponentModel
{
    public class PropertyDescriptorEx
    {
        private static ConcurrentDictionary<PropertyDescriptor, PropertyDescriptorEx> cache
            = new ConcurrentDictionary<PropertyDescriptor, PropertyDescriptorEx>();

        public static PropertyDescriptorEx GetCached(PropertyDescriptor property)
        {
            if (!cache.TryGetValue(property, out var prop))
            {
                prop = new PropertyDescriptorEx(property);
                cache.TryAdd(property, prop);
            }

            return prop;
        }

        public PropertyDescriptor PropertyDescriptor { get; private set; }

        Func<object, object> getter;
        Action<object, object> setter;

        Func<object, object> Getter { get { if (getter == null) InitializeGet(); return getter; } }
        Action<object, object> Setter { get { if (setter == null) InitializeSet(); return setter; } }


        public PropertyDescriptorEx(PropertyDescriptor property)
        {
            this.PropertyDescriptor = property;
        }

        private void InitializeSet()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            // value as T is slightly faster than (T)value, so if it's not a value type, use that
            UnaryExpression instanceCast = (!this.PropertyDescriptor.ComponentType.IsValueType) ? Expression.TypeAs(instance, this.PropertyDescriptor.ComponentType) : Expression.Convert(instance, this.PropertyDescriptor.ComponentType);
            UnaryExpression valueCast = (!this.PropertyDescriptor.PropertyType.IsValueType) ? Expression.TypeAs(value, this.PropertyDescriptor.PropertyType) : Expression.Convert(value, this.PropertyDescriptor.PropertyType);
            this.setter = Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, this.PropertyDescriptor.GetType().GetMethod("SetValue"), valueCast), new ParameterExpression[] { instance, value }).Compile();
        }

        private void InitializeGet()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast = (!this.PropertyDescriptor.ComponentType.IsValueType) ? Expression.TypeAs(instance, this.PropertyDescriptor.ComponentType) : Expression.Convert(instance, this.PropertyDescriptor.ComponentType);
            this.getter = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instanceCast, this.PropertyDescriptor.GetType().GetMethod("GetValue")), typeof(object)), instance).Compile();
        }

        public object Get(object instance)
        {
            return this.Getter(instance);
        }

        public void Set(object instance, object value)
        {
            this.Setter(instance, value);
        }

    }

}
