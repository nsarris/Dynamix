using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{


    public class PropertyInfoExWrapper
    //where T : class
    {
        IReadOnlyDictionary<string, PropertyInfoEx> props;
        protected object wrappedObject;
        bool caching;

        public object WrappedObject { get { return wrappedObject; } }

        public PropertyInfoExWrapper(object wrappedObject, bool EnablePropertyCaching = true)
        {
            caching = EnablePropertyCaching;
            this.wrappedObject = wrappedObject;
        }

        public IReadOnlyDictionary<string, PropertyInfoEx> Properties
        {
            get
            {
                if (caching)
                    props = wrappedObject.GetType().GetPropertiesExDic();
                else
                    if (props == null)
                        props = wrappedObject.GetType().GetProperties().ToDictionary(x => x.Name, x => new PropertyInfoEx(x));

                return props;
            }
        }

        public object this[string PropertyName, bool allowPrivate = false]
        {
            get
            {
                PropertyInfoEx prop;
                if (Properties.TryGetValue(PropertyName, out prop))
                    return prop.Get(wrappedObject, allowPrivate);

                else throw new ArgumentException("Property " + PropertyName + " does not exist in Type " + wrappedObject.GetType());
            }
            set
            {
                PropertyInfoEx prop;
                if (Properties.TryGetValue(PropertyName, out prop))
                    prop.Set(wrappedObject, value, allowPrivate);

                else throw new ArgumentException("Property " + PropertyName + " does not exist in Type " + wrappedObject.GetType());
            }
        }

        public TProperty Get<TProperty>(string PropertyName, bool allowPrivate = false)
        {
            var v = this[PropertyName,allowPrivate];
            return (TProperty)Convert.ChangeType(v, typeof(TProperty));
        }

        public object Get(string PropertyName, bool allowPrivate = false)
        {
            return this[PropertyName,allowPrivate];
        }

        public void Set(string PropertyName, object Value, bool allowPrivate = false)
        {
            this[PropertyName,allowPrivate] = Value;
        }

        public void RemoveFromCache()
        {
            PropertyInfoExCache.RemoveFromCache(wrappedObject.GetType());
        }

        public bool EqualsByPropertyValues(PropertyInfoExWrapper other)
        {
            if (other == null
                || this.wrappedObject.GetType() != other.wrappedObject.GetType())
                return false;

            foreach (var prop in Properties.Values)
            {
                var v1 = prop.Get(this.wrappedObject);
                var v2 = prop.Get(other.wrappedObject);

                if ((v1 == null && v2 != null)
                    || (v1 != null && v2 == null))
                    return false;

                if (v1 != null && v2 != null)
                    if (!v1.Equals(v2))
                        return false;
            }

            return true;
        }
    }

    public class PropertyInfoExWrapper<T> : PropertyInfoExWrapper
    {
        public PropertyInfoExWrapper(T wrappedObject, bool EnablePropertyCaching = true)
            : base(wrappedObject, EnablePropertyCaching)
        {

        }

        public new T WrappedObject { get { return (T)wrappedObject; } }
    }
}
