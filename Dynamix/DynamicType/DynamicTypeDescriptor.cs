using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public class DynamicTypeProperty
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public DynamicTypeProperty()
        {

        }

        public DynamicTypeProperty(string Name, Type Type)
        {
            this.Name = Name;
            this.Type = Type;
        }
    }
    internal class DynamicTypeCachedDescriptor
    {
        public DynamicTypeCachedDescriptor(Type Type, IEnumerable<DynamicTypeProperty> fields = null)
        {
            this.Type = Type;
            if (fields == null)
                this.Fields = new List<DynamicTypeProperty>();
            else
                this.Fields = fields.Select(x => new DynamicTypeProperty() { Type = x.Type, Name = x.Name }).ToList();
        }
        public Type Type { get; set; }
        public List<DynamicTypeProperty> Fields { get; set; }

        public DynamicTypeCachedDescriptor AddProperty(string Name, Type Type)
        {
            this.Fields.Add(new DynamicTypeProperty() { Name = Name, Type = Type });
            return this;
        }
    }

    public class DynamicTypeDescriptor
    {
        public DynamicTypeDescriptor(string Name = null, IEnumerable<DynamicTypeProperty> props = null, Type BaseType = null)
        {
            this.Name = Name;
            if (props == null)
                this.Properties = new List<DynamicTypeProperty>();
            else
                this.Properties = props.Select(x => new DynamicTypeProperty() { Type = x.Type, Name = x.Name }).ToList();
        }
        public string Name { get; set; }
        public Type BaseType { get; set; }
        public List<DynamicTypeProperty> Properties { get; set; }

        public DynamicTypeDescriptor AddProperty(string Name, Type Type, bool AsNullable = false)
        {
            this.Properties.Add(new DynamicTypeProperty() { Name = Name, Type = (AsNullable ? ToNullable(Type) : Type) });
            return this;
        }

        public DynamicTypeDescriptor AddProperty<T>(string Name)
        {
            return AddProperty(Name, typeof(T));
        }

        public DynamicTypeDescriptor AddProperty<T>(Expression<Func<T, object>> TemplatePropertyExpression, string OverrideName = null, Type OverrideType = null, bool AsNullable = false)
        {
            var prop = ReflectionHelper.GetProperty(TemplatePropertyExpression);
            return AddProperty(OverrideName == null ? prop.Name : OverrideName, OverrideType == null ? (AsNullable ? ToNullable(prop.PropertyType) : prop.PropertyType) : OverrideType);
        }

        public DynamicTypeDescriptor AddPropertyAsNullable(string Name, Type Type)
        {
            return AddProperty(Name, Type, true);
        }

        public DynamicTypeDescriptor AddPropertyAsNullable<T>(Expression<Func<T, object>> TemplatePropertyExpression, string OverrideName = null, Type OverrideType = null)
        {
            return AddProperty(TemplatePropertyExpression, OverrideName, OverrideType, true);
        }

        public DynamicTypeDescriptor HasBaseType(Type BaseType)
        {
            this.BaseType = BaseType;
            return this;
        }

        public DynamicTypeDescriptor HasBaseType<T>()
        {
            this.BaseType = typeof(T);
            return this;
        }

        public DynamicTypeDescriptor HasName(string Name)
        {
            this.Name = Name;
            return this;
        }

        private static Type ToNullable(Type SourceType)
        {
            var type = Nullable.GetUnderlyingType(SourceType);

            if (type == null)
                type = SourceType;

            if (type.IsValueType)
                type = typeof(Nullable<>).MakeGenericType(type);
            else
                type = SourceType;

            return type;
        }
    }
}
