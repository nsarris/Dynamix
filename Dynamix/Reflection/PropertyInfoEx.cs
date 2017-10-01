using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public class PropertyInfoExIgnoreAttribute : Attribute
    {

    }

    [DebuggerDisplay("Name = {PropertyInfo.Name}, Type = {PropertyInfo.PropertyType}")]
    public class PropertyInfoEx : IMemberInfoEx
    {
        public PropertyInfo PropertyInfo { get; private set; }
        public bool IsEnumerable { get; private set; }
        public EnumerableTypeDescriptor EnumerableDescriptor { get; private set; }
        public Func<object, object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }

        public bool CanSet { get; private set; }
        public bool CanGet { get; private set; }
        public bool PublicSet { get; private set; }
        public bool PublicGet { get; private set; }

        Type IMemberInfoEx.Type => PropertyInfo.PropertyType;
        MemberInfo IMemberInfoEx.MemberInfo => PropertyInfo;
        bool IMemberInfoEx.IsField => true;

        public Type Type => PropertyInfo.PropertyType;
        public PropertyInfoEx(PropertyInfo property)
        {
            this.PropertyInfo = property;

            if (property.PropertyType != typeof(string))
            {
                this.EnumerableDescriptor = EnumerableTypeDescriptor.Get(property.PropertyType);
                this.IsEnumerable = this.EnumerableDescriptor != null;
            }

            //PrimitiveLike?

            InitializeGet();
            InitializeSet();
        }

        private void InitializeSet()
        {
            var setMethod = this.PropertyInfo.GetSetMethod(true);
            PublicSet = setMethod.IsPublic;
            
            if (setMethod == null)
                return;

            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            // value as T is slightly faster than (T)value, so if it's not a value type, use that
            UnaryExpression instanceCast = (!this.PropertyInfo.DeclaringType.IsValueType) ? Expression.TypeAs(instance, this.PropertyInfo.DeclaringType) : Expression.Convert(instance, this.PropertyInfo.DeclaringType);
            UnaryExpression valueCast = (!this.PropertyInfo.PropertyType.IsValueType) ? Expression.TypeAs(value, this.PropertyInfo.PropertyType) : Expression.Convert(value, this.PropertyInfo.PropertyType);
            this.Setter = Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, setMethod, valueCast), new ParameterExpression[] { instance, value }).Compile();
            this.CanSet = true;
        }

        private void InitializeGet()
        {
            var getMethod = this.PropertyInfo.GetGetMethod(true);
            PublicGet = getMethod.IsPublic;

            if (getMethod == null)
                return;

            var instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast = (!this.PropertyInfo.DeclaringType.IsValueType) ? Expression.TypeAs(instance, this.PropertyInfo.DeclaringType) : Expression.Convert(instance, this.PropertyInfo.DeclaringType);
            this.Getter = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instanceCast, this.PropertyInfo.GetGetMethod()), typeof(object)), instance).Compile();
            this.CanGet = true;
        }

        public object Get(object instance,bool allowPrivateGet = false)
        {
            if (!allowPrivateGet && !PublicGet)
                throw new Exception("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " has no public get accessor.");
            if (!CanGet)
                throw new Exception("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " has no get accessor.");
            
            return this.Getter(instance);
        }

        public void Set(object instance, object value, bool allowPrivateSet = false)
        {
            if (!allowPrivateSet && !PublicSet)
                throw new Exception("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " has no public set accessor.");
            if (!CanSet)
                throw new Exception("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " has no set accessor.");
            
            this.Setter(instance, value);
        }

    }

    }
