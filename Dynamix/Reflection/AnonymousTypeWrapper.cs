using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{


    public class AnonymousTypeWrapper
    {
        IReadOnlyDictionary<string, FieldInfoEx> fields;
        protected object wrappedObject;
        bool caching;

        public object WrappedObject { get { return wrappedObject; } }

        public AnonymousTypeWrapper(object wrappedObject, bool EnableFieldCaching = true)
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
        
        private string TranslateFieldName(string fieldname)
        {
            return "<" + fieldname + ">i__Field";
        }

        public object this[string FieldName]
        {
            get
            {
                if (Fields.TryGetValue(TranslateFieldName(FieldName), out var field))
                    return field.Get(wrappedObject, true);

                else throw new ArgumentException("Field " + FieldName + " does not exist in Type " + wrappedObject.GetType());
            }
            set
            {
                if (Fields.TryGetValue(TranslateFieldName(FieldName), out var field))
                    field.Set(wrappedObject, value, true);
                
                else throw new ArgumentException("Field " + FieldName + " does not exist in Type " + wrappedObject.GetType());
            }
        }

        public TField Get<TField>(string FieldName)
        {
            var v = this[TranslateFieldName(FieldName)];
            return (TField)Convert.ChangeType(v, typeof(TField));
        }

        public object Get(string FieldName)
        {
            return this[TranslateFieldName(FieldName)];
        }

        public void Set(string FieldName, object Value)
        {
            this[TranslateFieldName(FieldName)] = Value;
        }

        public void RemoveFromCache()
        {
            FieldInfoExCache.RemoveFromCache(wrappedObject.GetType());
        }

        //public bool EqualsByFieldValues(AnonymousTypeWrapper other)
        //{
        //    if (other == null
        //        || this.wrappedObject.GetType() != other.wrappedObject.GetType())
        //        return false;

        //    foreach (var field in Fields.Values)
        //    {
        //        var v1 = field.Get(this.wrappedObject);
        //        var v2 = field.Get(other.wrappedObject);

        //        if ((v1 == null && v2 != null)
        //            || (v1 != null && v2 == null))
        //            return false;

        //        if (v1 != null && v2 != null)
        //            if (!v1.Equals(v2))
        //                return false;
        //    }

        //    return true;
        //}

        public static AnonymousTypeWrapper<T> Create<T>(T anonymousObject)
        {
            return new AnonymousTypeWrapper<T>(anonymousObject);
        }
    }

    public class AnonymousTypeWrapper<T> : AnonymousTypeWrapper
    {
        public AnonymousTypeWrapper(T wrappedObject, bool EnableCaching = true)
            : base(wrappedObject, EnableCaching)
        {

        }

        public new T WrappedObject { get { return (T)wrappedObject; } }
    }
}
