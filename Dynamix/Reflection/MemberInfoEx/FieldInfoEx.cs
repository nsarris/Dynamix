using Dynamix.Expressions;
using Dynamix.Expressions.LambdaBuilders;
using Dynamix.Reflection.DelegateBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public class FieldInfoExIgnoreAttribute : Attribute
    {

    }

    [DebuggerDisplay("Name = {FieldInfo.Name}, Type = {FieldInfo.FieldType}")]
    public class FieldInfoEx : IValueMemberInfoEx
    {
        public FieldInfo FieldInfo { get; private set; }
        public bool IsEnumerable { get; private set; }
        public EnumerableTypeDescriptor EnumerableDescriptor { get; private set; }
        bool IValueMemberInfoEx.PublicGet => FieldInfo.IsPublic;
        bool IValueMemberInfoEx.PublicSet => FieldInfo.IsPublic;
        MemberInfoExKind IMemberInfoEx.Kind  => MemberInfoExKind.Field;
        MemberInfo IMemberInfoEx.MemberInfo => FieldInfo;
        bool IValueMemberInfoEx.IsField => true;
        public bool IsPublic => FieldInfo.IsPublic;
        public string Name => FieldInfo.Name;
        public Type Type => FieldInfo.FieldType;
        public Func<object, object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }
        public bool IsStatic => FieldInfo.IsStatic;
        public string AutoPropertyName { get; private set; }
        
        public FieldInfoEx(FieldInfo field, bool enableDelegateCaching = true)
        {
            this.FieldInfo = field;

            if (field.FieldType != typeof(string))
            {
                this.EnumerableDescriptor = EnumerableTypeDescriptor.Get(field.FieldType);
                this.IsEnumerable = this.EnumerableDescriptor != null;
            }

            if (FieldInfo.IsInitOnly && FieldInfo.Name.StartsWith("<") && FieldInfo.Name.EndsWith(">i__Field"))
                AutoPropertyName = FieldInfo.Name.Substring(1, FieldInfo.Name.Length - 9);
            if (FieldInfo.HasAttribute<CompilerGeneratedAttribute>()
                && FieldInfo.Name.StartsWith("<") && FieldInfo.Name.EndsWith(">k__BackingField"))
                AutoPropertyName = FieldInfo.Name.Substring(1, FieldInfo.Name.Length - 17);

            //PrimitiveLike?
            
            if (enableDelegateCaching)
            {
                this.Getter = MemberAccessorDelegateBuilder.CachedFieldBuilder.BuildGenericGetter(this.FieldInfo);
                this.Setter = MemberAccessorDelegateBuilder.CachedFieldBuilder.BuildGenericSetter(this.FieldInfo);
            }
            else
            {
                var builder = new FieldAccessorLambdaBuilder(false);
                this.Getter = builder.BuildGenericGetter(this.FieldInfo).Compile();
                this.Setter = builder.BuildGenericSetter(this.FieldInfo).Compile();
            }
        }

        private void ValidateGetter(bool asStatic = false)
        {
            if (asStatic && !IsStatic)
                throw new Exception("Property " + this.FieldInfo.Name + " of " + this.FieldInfo.DeclaringType.FullName + " is not static");
        }

        private void ValidateSetter(bool asStatic = false)
        {
            if (asStatic && !IsStatic)
                throw new Exception("Property " + this.FieldInfo.Name + " of " + this.FieldInfo.DeclaringType.FullName + " is not static");
        }

        //General purpose
        public object Get(object instance)
        {
            ValidateGetter();
            return this.Getter(instance);
        }

        public void Set(object instance, object value)
        {
            ValidateSetter();
            this.Setter(instance, value);
        }

        public T Get<T>(object instance)
        {
            return (T)Convert.ChangeType(Get(instance), typeof(T));
        }

        //Static
        public object Get(bool allowPrivate = false)
        {
            ValidateGetter(true);
            return this.Getter(null);
        }

        public void Set(object value)
        {
            ValidateSetter(true);
            this.Setter(null, value);
        }

        public T Get<T>(bool allowPrivate = false)
        {
            return (T)Convert.ChangeType(Get(null), typeof(T));
        }

        public static implicit operator FieldInfo(FieldInfoEx fieldInfoEx)
        {
            return fieldInfoEx.FieldInfo;
        }
    }
}
