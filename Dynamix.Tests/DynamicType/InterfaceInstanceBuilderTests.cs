//using Dynamix.DynamicType;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Tests.DynamicType
{
    public interface ITest
    {
        int Id { get; }
        string Name { get; }
    }

    [TestFixture]
    public class InterfaceInstanceBuilderTests
    {
        [Test]
        public void Build_DynamicType_From_Interface_With_All_Members()
        {
            var builder = new InterfaceInstanceBuilder<ITest>();

            var instance = builder.Build(x =>
                x.Member(m => m.Id, 2)
                .Member(m => m.Name, "Test"));

            Assert.AreEqual(2, instance.Id);
            Assert.AreEqual("Test", instance.Name);
        }

        [Test]
        public void Build_DynamicType_From_Interface_With_One_Member()
        {
            var builder = new InterfaceInstanceBuilder<ITest>();

            var instance = builder.Build(x => x
                .Member(m => m.Name, "Test"));

            Assert.AreEqual(0, instance.Id);
            Assert.AreEqual("Test", instance.Name);
        }
    }
}
