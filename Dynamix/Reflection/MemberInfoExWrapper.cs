using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{


    public class MemberInfoExWrapper
    //where T : class
    {
        PropertyInfoExWrapper propWrapper;
        FieldInfoExWrapper fieldWrapper;
        protected object wrappedObject;
        
        public object WrappedObject { get { return wrappedObject; } }

        public MemberInfoExWrapper(object wrappedObject, bool EnableCaching = true)
        {
            this.propWrapper = new PropertyInfoExWrapper(wrappedObject, EnableCaching);
            this.fieldWrapper = new FieldInfoExWrapper(wrappedObject, EnableCaching);
        }

        public IReadOnlyDictionary<string, PropertyInfoEx> Properties => propWrapper.Properties;
        public IReadOnlyDictionary<string, FieldInfoEx> Fields => fieldWrapper.Fields;

        public IMemberInfoEx GetMember(string MemberName)
        {
            propWrapper.Properties.TryGetValue(MemberName, out var prop);
            fieldWrapper.Fields.TryGetValue(MemberName, out var field);
            if (prop != null && field != null)
                throw new ArgumentException("Ambiguous memer name " + MemberName + " in Type " + wrappedObject.GetType());
            if (prop == null && field == null)
                throw new ArgumentException("Member " + MemberName + " does not exist in Type " + wrappedObject.GetType());

            if (prop != null) return prop; else return field;
        }

        public object this[string MemberName, bool allowPrivate = false]
        {
            get
            {
                return GetMember(MemberName).Get(wrappedObject, allowPrivate);
            }
            set
            {
                GetMember(MemberName).Set(wrappedObject, value, allowPrivate);
            }
        }

        public TMember GetMember<TMember>(string MemberName, bool allowPrivate = false)
        {
            var v = GetMember(MemberName).Get(wrappedObject, allowPrivate);
            return (TMember)Convert.ChangeType(v, typeof(TMember));
        }

        public object GetMember(string MemberName, bool allowPrivate = false)
        {
            return GetMember(MemberName).Get(wrappedObject, allowPrivate);
        }

        public void SetMember(string MemberName, object Value, bool allowPrivate = false)
        {
            GetMember(MemberName).Set(wrappedObject, Value, allowPrivate);
        }

        public TField GetField<TField>(string FieldName, bool allowPrivate = false)
        {
            return fieldWrapper.Get<TField>(FieldName, allowPrivate);
        }

        public object GetField(string FieldName, bool allowPrivate = false)
        {
            return fieldWrapper.Get(FieldName, allowPrivate);
        }

        public void SetField(string FieldName, object Value, bool allowPrivate = false)
        {
            fieldWrapper.Set(FieldName, Value, allowPrivate);
        }

        public TProperty GetProperty<TProperty>(string PropertyName, bool allowPrivate = false)
        {
            return propWrapper.Get<TProperty>(PropertyName, allowPrivate);
        }

        public object GetProperty(string PropertyName, bool allowPrivate = false)
        {
            return propWrapper.Get(PropertyName, allowPrivate);
        }

        public void SetProperty(string PropertyName, object Value, bool allowPrivate = false)
        {
            propWrapper.Set(PropertyName, Value, allowPrivate);
        }

        public void RemoveFromCache()
        {
            PropertyInfoExCache.RemoveFromCache(wrappedObject.GetType());
            FieldInfoExCache.RemoveFromCache(wrappedObject.GetType());
        }

        public bool EqualsByPropertyValues(PropertyInfoExWrapper other)
        {
            return propWrapper.EqualsByPropertyValues(other);
        }

        public bool EqualsByFieldValues(FieldInfoExWrapper other)
        {
            return fieldWrapper.EqualsByFieldValues(other);
        }
    }

    public class MemberInfoExWrapper<T> : MemberInfoExWrapper
    {
        public MemberInfoExWrapper(T wrappedObject, bool EnableCaching = true)
            : base(wrappedObject, EnableCaching)
        {

        }

        public new T WrappedObject { get { return (T)wrappedObject; } }
    }
}
