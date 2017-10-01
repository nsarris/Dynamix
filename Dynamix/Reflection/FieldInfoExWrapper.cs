using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{


    public class FieldInfoExWrapper
    {
        IReadOnlyDictionary<string, FieldInfoEx> fields;
        protected object wrappedObject;
        bool caching;

        public object WrappedObject { get { return wrappedObject; } }

        public FieldInfoExWrapper(object wrappedObject, bool EnableFieldCaching = true)
        {
            caching = EnableFieldCaching;
            this.wrappedObject = wrappedObject;
        }

        public IReadOnlyDictionary<string, FieldInfoEx> Fields
        {
            get
            {
                if (fields == null)
                    fields = wrappedObject.GetType().GetFieldsExDic(caching);
                
                return fields;
            }
        }

        public object this[string FieldName, bool allowPrivate = false]
        {
            get
            {
                if (Fields.TryGetValue(FieldName, out var field))
                    return field.Get(wrappedObject, allowPrivate);

                else throw new ArgumentException("Field " + FieldName + " does not exist in Type " + wrappedObject.GetType());
            }
            set
            {
                if (Fields.TryGetValue(FieldName, out var field))
                    field.Set(wrappedObject, value, allowPrivate);
                
                else throw new ArgumentException("Field " + FieldName + " does not exist in Type " + wrappedObject.GetType());
            }
        }

        public TField Get<TField>(string FieldName, bool allowPrivate = false)
        {
            var v = this[FieldName, allowPrivate];
            return (TField)Convert.ChangeType(v, typeof(TField));
        }

        public object Get(string FieldName, bool allowPrivate = false)
        {
            return this[FieldName, allowPrivate];
        }

        public void Set(string FieldName, object Value, bool allowPrivate = false)
        {
            this[FieldName, allowPrivate] = Value;
        }

        public void RemoveFromCache()
        {
            FieldInfoExCache.RemoveFromCache(wrappedObject.GetType());
        }

        public bool EqualsByFieldValues(FieldInfoExWrapper other)
        {
            if (other == null
                || this.wrappedObject.GetType() != other.wrappedObject.GetType())
                return false;

            foreach (var field in Fields.Values)
            {
                var v1 = field.Get(this.wrappedObject);
                var v2 = field.Get(other.wrappedObject);

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

    public class FieldInfoExWrapper<T> : FieldInfoExWrapper
    {
        public FieldInfoExWrapper(T wrappedObject, bool EnableCaching = true)
            : base(wrappedObject, EnableCaching)
        {

        }

        public new T WrappedObject { get { return (T)wrappedObject; } }
    }
}
