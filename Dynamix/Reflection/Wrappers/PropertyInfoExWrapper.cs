using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public class PropertyInfoExWrapper
    {
        public object WrappedObject { get; private set; }
        public IReadOnlyDictionary<string, PropertyInfoEx> Properties { get; private set; }

        public PropertyInfoExWrapper(object wrappedObject, BindingFlagsEx bindingFlags = BindingFlagsEx.All, bool enablePropertyCaching = true)
            : this(wrappedObject, (BindingFlags)bindingFlags, enablePropertyCaching)
        {
        }

        public PropertyInfoExWrapper(object wrappedObject, BindingFlags bindingFlags, bool enablePropertyCaching = true)
        {
            this.WrappedObject = wrappedObject ?? throw new ArgumentNullException(nameof(wrappedObject));
            this.Properties = wrappedObject.GetType().GetPropertiesExDic(bindingFlags, enablePropertyCaching);
        }


        public object this[string propertyName]
        {
            get
            {
                if (Properties.TryGetValue(propertyName, out var prop))
                    return prop.Get(WrappedObject);

                else throw new MissingMemberException("Property " + propertyName + " does not exist in Type " + WrappedObject.GetType());
            }
            set
            {
                if (Properties.TryGetValue(propertyName, out var prop))
                    prop.Set(WrappedObject, value);

                else throw new MissingMemberException("Property " + propertyName + " does not exist in Type " + WrappedObject.GetType());
            }
        }

        public TProperty Get<TProperty>(string propertyName)
        {
            var v = this[propertyName];
            return (TProperty)Convert.ChangeType(v, typeof(TProperty));
        }

        public object Get(string propertyName)
        {
            return this[propertyName];
        }

        public void Set(string propertyName, object value)
        {
            this[propertyName] = value;
        }
    }

    public class PropertyInfoExWrapper<T> : PropertyInfoExWrapper
    {
        public PropertyInfoExWrapper(object wrappedObject, BindingFlagsEx bindingFlags = BindingFlagsEx.All, bool enableFieldCaching = true)
            : base(wrappedObject, bindingFlags, enableFieldCaching)
        {
        }

        public PropertyInfoExWrapper(object wrappedObject, BindingFlags bindingFlags, bool enableFieldCaching = true)
            : base(wrappedObject, bindingFlags, enableFieldCaching)
        {

        }

        public new T WrappedObject { get { return (T)base.WrappedObject; } }
    }
}
