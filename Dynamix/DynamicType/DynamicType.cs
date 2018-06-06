using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public abstract class DynamicType
    {
        [NonSerialized]
        IReadOnlyDictionary<string, PropertyInfoEx> props;
        
        public IEnumerable<PropertyInfoEx> GetProperties()
        {
            return Properties.Values;
        }

        public IReadOnlyDictionary<string, PropertyInfoEx> GetPropertiesDic()
        {
            return Properties;
        }

        public Dictionary<string, object> ToDictionary()
        {
            return Properties.ToDictionary(x => x.Key, x => x.Value.Get(this));
        }

        [PropertyInfoExIgnore]
        protected IReadOnlyDictionary<string, PropertyInfoEx> Properties
        {
            get
            {
                if (props == null)
                    props = PropertyInfoExCache.GetPropertiesExDic(this.GetType());
                
                return props;
            }
        }

        [PropertyInfoExIgnore]
        public object this[string PropertyName]
        {
            get
            {
                if (Properties.TryGetValue(PropertyName, out PropertyInfoEx prop))
                {
                    return prop.Get(this);
                }
                else throw new ArgumentException("Property " + PropertyName + " does not exist in Type " + this.GetType());
            }
            set
            {
                if (Properties.TryGetValue(PropertyName, out PropertyInfoEx prop))
                {
                    prop.Set(this, value);
                }
                else throw new ArgumentException("Property " + PropertyName + " does not exist in Type " + this.GetType());
            }
        }

        public T Get<T>(string PropertyName)
        {
            var v = this[PropertyName];
            var nullableOf = Nullable.GetUnderlyingType(typeof(T));
            if (nullableOf == null)
                return (T)Convert.ChangeType(v, typeof(T));
            else if (v == null)
                return default;
            else if (nullableOf == v.GetType())
                return (T)v;
            else 
                return (T)Convert.ChangeType(v, nullableOf);
        }

        //TODO Get Path e.g. o.Get("Customer.City.Name")
        //Wrap each object node with PropertyInfoExWrapper 
    }
}
