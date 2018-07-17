using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Tests.DynamicType
{
    public class TestPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public TestPropertyAttribute(string name, string value)
        {
            Name = name; Value = value;
        }
    }

    [TestFixture]
    class DynamicTypeTests
    {
        
        [Test]
        public void TestDynamicTypeBuilder()
        {
            var descriptor = new DynamicTypeDescriptorBuilder()
                .HasName("SomeDynamicType")
                .AddProperty<int>("A", config => config
                    .HasAttribute(() => new TestPropertyAttribute("name", "value"))
                );

            var type = DynamicTypeBuilder.Instance.CreateType(descriptor);

            
            var ctor = Expression.Lambda<Func<object[],object>>(Expression.New(type.GetConstructor(new Type[] { })), Expression.Parameter(typeof(object[]))).Compile();
            var o = ctor(new object[] { });
        }
    }
}
