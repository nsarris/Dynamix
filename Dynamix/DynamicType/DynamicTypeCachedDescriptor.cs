using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamix
{
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
}
