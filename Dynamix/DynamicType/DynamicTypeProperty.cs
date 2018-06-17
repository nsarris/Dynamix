using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Dynamix
{
    public class DynamicTypeProperty
    {
        public string Name { get; }
        public Type Type { get; }

        public IReadOnlyList<CustomAttributeBuilder> AttributeBuilders => attributeBuilders;

        private readonly List<CustomAttributeBuilder> attributeBuilders = new List<CustomAttributeBuilder>();
        public DynamicTypeProperty()
        {

        }

        public DynamicTypeProperty(string Name, Type Type)
        {
            this.Name = Name;
            this.Type = Type;
        }

        public DynamicTypeProperty HasAttribute(Expression<Func<Attribute>> builderExpression)
        {
            attributeBuilders.Add(CustomAttributeBuilderFactory.FromExpression(builderExpression));
            return this;
        }
    }
}
