using Dynamix.Expressions.LambdaBuilders;
using Dynamix.Helpers;
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
    public class PropertyInfoExIgnoreAttribute : Attribute
    {

    }

    [DebuggerDisplay("Name = {PropertyInfo.Name}, Type = {PropertyInfo.PropertyType}")]
    public class PropertyInfoEx : IValueMemberInfoEx
    {
        public PropertyInfo PropertyInfo { get; private set; }
        public bool IsEnumerable { get; private set; }
        public EnumerableTypeDescriptor EnumerableDescriptor { get; private set; }

        public GenericPropertyGetter Getter { get; private set; }
        public GenericPropertySetter Setter { get; private set; }

        public int IndexerCount { get; private set; }
        public bool CanSet { get; private set; }
        public bool CanGet { get; private set; }
        public bool PublicSet { get; private set; }
        public bool PublicGet { get; private set; }
        public bool IsStatic { get; private set; }

        MemberInfo IMemberInfoEx.MemberInfo => PropertyInfo;
        bool IValueMemberInfoEx.IsField => false;
        MemberInfoExKind IMemberInfoEx.Kind => MemberInfoExKind.Field;
        public Type Type => PropertyInfo.PropertyType;
        public string Name => PropertyInfo.Name;
        public string BackingFieldName { get; private set; }

        public PropertyInfoEx(PropertyInfo property, bool enableDelegateCaching = true)
        {
            this.PropertyInfo = property;

            if (property.PropertyType != typeof(string))
            {
                this.EnumerableDescriptor = EnumerableTypeDescriptor.Get(property.PropertyType);
                this.IsEnumerable = this.EnumerableDescriptor.IsEnumerable;
            }

            this.IndexerCount = PropertyInfo.GetIndexParameters().Count();

            //PrimitiveLike?

            var backingField = property.DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(x => x.HasAttribute<CompilerGeneratedAttribute>()
                    && (x.Name == "<" + property.Name + ">k__BackingField" // Auto property
                        || x.Name == "<" + property.Name + ">i__Field")) //Anonymous type
                .FirstOrDefault();

            if (backingField != null)
                BackingFieldName = backingField.Name;

            InitializeGet(enableDelegateCaching);
            InitializeSet(enableDelegateCaching);
        }

        private void InitializeSet(bool enableDelegateCaching)
        {
            var setMethod = this.PropertyInfo.GetSetMethod(true);
            if (setMethod == null)
                return;

            this.CanSet = true;
            this.IsStatic = setMethod.IsStatic;
            this.PublicSet = setMethod.IsPublic;

            if (enableDelegateCaching)
                this.Setter = MemberAccessorDelegateBuilder.CachedPropertyBuilder.BuildGenericSetter(this.PropertyInfo);
            else
            {
                var builder = new PropertyAccessorLambdaBuilder(false);
                this.Setter = builder.BuildGenericSetter(this.PropertyInfo).Compile();
            }

        }

        private void InitializeGet(bool enableDelegateCaching)
        {
            var getMethod = this.PropertyInfo.GetGetMethod(true);
            if (getMethod == null)
                return;

            this.CanGet = true;
            this.IsStatic = getMethod.IsStatic;
            this.PublicGet = getMethod.IsPublic;

            if (enableDelegateCaching)
                this.Getter = MemberAccessorDelegateBuilder.CachedPropertyBuilder.BuildGenericGetter(this.PropertyInfo);
            else
            {
                var builder = new PropertyAccessorLambdaBuilder(false);
                this.Getter = builder.BuildGenericGetter(this.PropertyInfo).Compile();
            }
        }

        private void ValidateGetter(object[] indexers = null, bool asStatic = false)
        {
            if (!CanGet)
                throw new InvalidOperationException("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " has no get accessor.");
            if (indexers != null && indexers.Count() != IndexerCount)
                throw new InvalidOperationException("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " has " + IndexerCount + " index parameters but " + indexers.Count() + "where provided");
            if (asStatic && !IsStatic)
                throw new InvalidOperationException("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " is not static");
        }

        private void ValidateSetter(object[] indexers = null, bool asStatic = false)
        {
            if (!CanSet)
                throw new InvalidOperationException("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " has no set accessor.");
            if (indexers != null && indexers.Count() != IndexerCount)
                throw new InvalidOperationException("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " has " + IndexerCount + " index parameters but " + indexers.Count() + "where provided");
            if (asStatic && !IsStatic)
                throw new InvalidOperationException("Property " + this.PropertyInfo.Name + " of " + this.PropertyInfo.DeclaringType.FullName + " is not static");
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
            this.Setter(instance, ConvertEx.ConvertTo(value, this.Type));
        }

        public T Get<T>(object instance)
        {
            return ConvertEx.Convert<T>(Get(instance));
        }

        //Static
        public object Get(bool allowPrivate = false)
        {
            ValidateGetter(null, true);
            return this.Getter(null);
        }

        public void Set(object value)
        {
            ValidateSetter(null, true);
            this.Setter(null, ConvertEx.ConvertTo(value, this.Type));
        }

        public T Get<T>(bool allowPrivate = false)
        {
            return ConvertEx.Convert<T>(Get(null, allowPrivate));
        }


        //Indexers
        public object Get(object instance, object[] indexers)
        {
            ValidateGetter(indexers);
            return this.Getter(instance, indexers);
        }

        public void Set(object instance, object[] indexers, object value)
        {
            ValidateSetter(indexers);
            this.Setter(instance, ConvertEx.ConvertTo(value, this.Type), indexers);
        }

        public T Get<T>(object instance, object[] indexers)
        {
            return ConvertEx.Convert<T>(Get(instance, indexers));
        }


        //1 Indexer

        public object Get(object instance, object indexer)
        {
            ValidateGetter();
            return this.Getter(instance, indexer);
        }

        public void Set(object instance, object value, object indexer)
        {
            ValidateSetter();
            this.Setter(instance, ConvertEx.ConvertTo(value, this.Type), indexer);
        }

        public T Get<T>(object instance, object indexer)
        {
            return ConvertEx.Convert<T>(Get(instance, indexer));
        }

        //2 Indexers

        public object Get(object instance, object indexer1, object indexer2)
        {
            ValidateGetter();
            return this.Getter(instance, indexer1, indexer2);
        }

        public void Set(object instance, object value, object indexer1, object indexer2)
        {
            ValidateSetter();
            this.Setter(instance, ConvertEx.ConvertTo(value, this.Type), indexer1, indexer2);
        }

        public T Get<T>(object instance, object indexer1, object indexer2)
        {
            return ConvertEx.Convert<T>(Get(instance, indexer1, indexer2));
        }

        //3 Indexers

        public object Get(object instance, object indexer1, object indexer2, object indexer3)
        {
            ValidateGetter();
            return this.Getter(instance, indexer1, indexer2, indexer3);
        }

        public void Set(object instance, object value, object indexer1, object indexer2, object indexer3)
        {
            ValidateSetter();
            this.Setter(instance, ConvertEx.ConvertTo(value, this.Type), indexer1, indexer2, indexer3);
        }

        public T Get<T>(object instance, object indexer1, object indexer2, object indexer3)
        {
            return ConvertEx.Convert<T>(Get(instance, indexer1, indexer2, indexer3));
        }

        public static implicit operator PropertyInfo(PropertyInfoEx propertyInfoEx)
        {
            return propertyInfoEx.PropertyInfo;
        }
    }
}
