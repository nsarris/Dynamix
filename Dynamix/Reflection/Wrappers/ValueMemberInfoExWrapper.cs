using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{


    public class ValueMemberInfoExWrapper
    {
        PropertyInfoExWrapper propWrapper;
        FieldInfoExWrapper fieldWrapper;
        protected object wrappedObject;

        public object WrappedObject { get { return wrappedObject; } }

        public ValueMemberInfoExWrapper(object wrappedObject, BindingFlagsEx bindingFlags = BindingFlagsEx.All, bool enableCaching = true)
            : this(wrappedObject, (BindingFlags)bindingFlags, enableCaching)
        {

        }

        public ValueMemberInfoExWrapper(object wrappedObject, BindingFlags bindingFlags, bool enableCaching = true)
        {
            this.propWrapper = new PropertyInfoExWrapper(wrappedObject, bindingFlags, enableCaching);
            this.fieldWrapper = new FieldInfoExWrapper(wrappedObject, bindingFlags, enableCaching);
        }

        public IReadOnlyDictionary<string, PropertyInfoEx> Properties => propWrapper.Properties;
        public IReadOnlyDictionary<string, FieldInfoEx> Fields => fieldWrapper.Fields;

        public IValueMemberInfoEx GetMemberInfoEx(string MemberName)
        {
            propWrapper.Properties.TryGetValue(MemberName, out var prop);
            fieldWrapper.Fields.TryGetValue(MemberName, out var field);
            if (prop != null && field != null)
                throw new ArgumentException("Ambiguous memer name " + MemberName + " in Type " + wrappedObject.GetType());
            if (prop == null && field == null)
                throw new ArgumentException("Member " + MemberName + " does not exist in Type " + wrappedObject.GetType());

            if (prop != null) return prop; else return field;
        }

        public object this[string MemberName]
        {
            get
            {
                return GetMemberInfoEx(MemberName).Get(wrappedObject);
            }
            set
            {
                GetMemberInfoEx(MemberName).Set(wrappedObject, value);
            }
        }

        public TMember GetValue<TMember>(string MemberName)
        {
            var v = GetMemberInfoEx(MemberName).Get(wrappedObject);
            return (TMember)Convert.ChangeType(v, typeof(TMember));
        }

        public object GetValue(string MemberName)
        {
            return GetMemberInfoEx(MemberName).Get(wrappedObject);
        }

        public void SetValue(string MemberName, object Value)
        {
            GetMemberInfoEx(MemberName).Set(wrappedObject, Value);
        }

        public TField GetField<TField>(string FieldName)
        {
            return fieldWrapper.Get<TField>(FieldName);
        }

        public object GetField(string FieldName)
        {
            return fieldWrapper.Get(FieldName);
        }

        public void SetField(string FieldName, object Value)
        {
            fieldWrapper.Set(FieldName, Value);
        }

        public TProperty GetProperty<TProperty>(string PropertyName)
        {
            return propWrapper.Get<TProperty>(PropertyName);
        }

        public object GetProperty(string PropertyName)
        {
            return propWrapper.Get(PropertyName);
        }

        public void SetProperty(string PropertyName, object Value)
        {
            propWrapper.Set(PropertyName, Value);
        }
    }

    public class ValueMemberInfoExWrapper<T> : ValueMemberInfoExWrapper
    {
        public ValueMemberInfoExWrapper(object wrappedObject, BindingFlagsEx bindingFlags = BindingFlagsEx.All, bool enableCaching = true)
            : base(wrappedObject, (BindingFlags)bindingFlags, enableCaching)
        {

        }

        public ValueMemberInfoExWrapper(object wrappedObject, BindingFlags bindingFlags, bool enableCaching = true)
            : base(wrappedObject, bindingFlags, enableCaching)
        {
        }

        public new T WrappedObject { get { return (T)wrappedObject; } }
    }
}
