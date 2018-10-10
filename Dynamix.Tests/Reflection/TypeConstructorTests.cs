using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Reflection;

namespace Dynamix.Tests.Reflection
{

    public class Customer
    {

        public int Id { get; private set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public Customer(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class Customer1 : Customer
    {
        public Customer1(int id, string name) : base(id, name)
        {
        }
    }

    [TestFixture]
    public class TypeConstructorTests
    {
        [Test]
        public void TypeConstructorTest()
        {
            var id = 1;
            var name = "someone";
            var code = "somecode";

            var ctor1 = new TypeConstructor<Customer, int, string>(typeof(Customer1), initializer: x => x.Code = code);


            var c1 = ctor1.Construct(new object[] { id, name });
            Assert.IsTrue(c1.Id == id && c1.Name == name && c1.Code == code);

            c1 = ctor1.Construct(("id", id), ("name", name));
            Assert.IsTrue(c1.Id == id && c1.Name == name && c1.Code == code);

            c1 = ctor1.Construct(1, name);
            Assert.IsTrue(c1.Id == id && c1.Name == name && c1.Code == code);

            c1 = ctor1.ConstructWithDefaults(("id", id));
            Assert.IsTrue(c1.Id == id && c1.Name == null && c1.Code == code);

            Assert.Throws<InvalidOperationException>(() => c1 = ctor1.Construct(("id", id)));
        }
    }
}
