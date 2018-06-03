using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{


    public class FieldInfoExWrapper
    {
        public object WrappedObject { get; private set; }
        public IReadOnlyDictionary<string, FieldInfoEx> Fields { get; private set; }

        public FieldInfoExWrapper(object wrappedObject, BindingFlagsEx bindingFlags = BindingFlagsEx.All, bool enableFieldCaching = true)
            : this(wrappedObject, (BindingFlags)bindingFlags, enableFieldCaching)
        {
        }

        public FieldInfoExWrapper(object wrappedObject, BindingFlags bindingFlags, bool enableFieldCaching = true)
        {
            this.WrappedObject = wrappedObject ?? throw new ArgumentNullException(nameof(wrappedObject));
            this.Fields = wrappedObject.GetType().GetFieldsExDic(bindingFlags, enableFieldCaching);
        }
        
        public object this[string fieldName]
        {
            get
            {
                if (Fields.TryGetValue(fieldName, out var field))
                    return field.Get(WrappedObject);

                else throw new MissingFieldException("Field " + fieldName + " does not exist in Type " + WrappedObject.GetType());
            }
            set
            {
                if (Fields.TryGetValue(fieldName, out var field))
                    field.Set(WrappedObject, value);
                
                else throw new MissingFieldException("Field " + fieldName + " does not exist in Type " + WrappedObject.GetType());
            }
        }

        public TField Get<TField>(string fieldName)
        {
            var v = this[fieldName];
            return (TField)Convert.ChangeType(v, typeof(TField));
        }

        public object Get(string fieldName)
        {
            return this[fieldName];
        }

        public void Set(string fieldName, object value)
        {
            this[fieldName] = value;
        }
    }

    public class FieldInfoExWrapper<T> : FieldInfoExWrapper
    {
        public FieldInfoExWrapper(object wrappedObject, BindingFlagsEx bindingFlags = BindingFlagsEx.All, bool enableFieldCaching = true)
            : base(wrappedObject, bindingFlags, enableFieldCaching)
        {
        }

        public FieldInfoExWrapper(object wrappedObject, BindingFlags bindingFlags, bool enableFieldCaching = true)
            :base(wrappedObject,bindingFlags,enableFieldCaching)
        {

        }

        public new T WrappedObject { get { return (T)base.WrappedObject; } }
    }
}
