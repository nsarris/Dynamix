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
    public class FieldInfoExIgnoreAttribute : Attribute
    {

    }

    [DebuggerDisplay("Name = {FieldInfo.Name}, Type = {FieldInfo.FieldType}")]
    public class FieldInfoEx : IMemberInfoEx
    {
        public FieldInfo FieldInfo { get; private set; }
        public bool IsEnumerable { get; private set; }
        public EnumerableTypeDescriptor EnumerableDescriptor { get; private set; }
        public Func<object, object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }
        bool IMemberInfoEx.PublicGet => FieldInfo.IsPublic;
        bool IMemberInfoEx.PublicSet => FieldInfo.IsPublic;

        Type IMemberInfoEx.Type => FieldInfo.FieldType;

        MemberInfo IMemberInfoEx.MemberInfo => FieldInfo;

        bool IMemberInfoEx.IsField => true;
        public bool IsPublic { get; private set; }
        public Type Type => FieldInfo.FieldType;

        public string AnonymousPropertyName { get; private set; }
        //public string ToAnonymousFieldName(string FieldName) { return "<" + FieldName + ">i__FieldName"; }

        public FieldInfoEx(FieldInfo field)
        {
            this.FieldInfo = field;

            if (field.FieldType != typeof(string))
            {
                this.EnumerableDescriptor = EnumerableTypeDescriptor.Get(field.FieldType);
                this.IsEnumerable = this.EnumerableDescriptor != null;
            }
            IsPublic = this.FieldInfo.IsPublic;

            if (FieldInfo.IsInitOnly && FieldInfo.Name.StartsWith("<") && FieldInfo.Name.EndsWith(">i__Field"))
                AnonymousPropertyName = FieldInfo.Name.Substring(1, FieldInfo.Name.Length - 9);

            //PrimitiveLike?

            InitializeGet();
            InitializeSet();
        }

        private void InitializeSet()
        {

            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            if (!FieldInfo.IsInitOnly)
            {
                var instanceCast = (!this.FieldInfo.DeclaringType.IsValueType) ? Expression.TypeAs(instance, this.FieldInfo.DeclaringType) : Expression.Convert(instance, this.FieldInfo.DeclaringType);
                var valueCast = (!this.FieldInfo.FieldType.IsValueType) ? Expression.TypeAs(value, this.FieldInfo.FieldType) : Expression.Convert(value, this.FieldInfo.FieldType);
                this.Setter = Expression.Lambda<Action<object, object>>(Expression.Assign(Expression.Field(instanceCast, this.FieldInfo), valueCast), new ParameterExpression[] { instance, value }).Compile();
            }
            else
            {
                this.Setter = (object obj, object v) => FieldInfo.SetValue(obj, v);
            }
        }

        private void InitializeGet()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var instanceCast = (!this.FieldInfo.DeclaringType.IsValueType) ? Expression.TypeAs(instance, this.FieldInfo.DeclaringType) : Expression.Convert(instance, this.FieldInfo.DeclaringType);
            this.Getter = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.PropertyOrField(instanceCast, this.FieldInfo.Name), typeof(object)), instance).Compile();
        }

        public object Get(object instance, bool allowPrivate = false)
        {
            if (!allowPrivate && !IsPublic)
                throw new Exception("Field " + this.FieldInfo.Name + " of " + this.FieldInfo.DeclaringType.FullName + " is not public.");

            return this.Getter(instance);
        }

        public void Set(object instance, object value, bool allowPrivate = false)
        {
            if (!allowPrivate && !IsPublic)
                throw new Exception("Field " + this.FieldInfo.Name + " of " + this.FieldInfo.DeclaringType.FullName + " is not public.");

            this.Setter(instance, value);
        }

        object IMemberInfoEx.Get(object instance, bool allowPrivate)
        {
            throw new NotImplementedException();
        }

        void IMemberInfoEx.Set(object instance, object value, bool allowPrivate)
        {
            throw new NotImplementedException();
        }
    }
}
