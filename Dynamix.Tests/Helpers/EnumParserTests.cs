using Dynamix.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Tests.Helpers
{
    public enum TestEnum1 : byte
    {
        Val0 = 0,
        Val1 = 1,
        Val2 = 2,
        Val3 = 3
    }
    public enum TestEnum2 : byte
    {
        Val0 = 0,
        Val1 = 1,
        Val2 = 2
    }

    public class SomeClase
    {
        private readonly int i;

        public SomeClase(int i)
        {
            this.i = i;
        }
        public override string ToString()
        {
            return i.ToString();
        }
    }

    [TestFixture]
    public class EnumParserTests
    {
        [Test]
        public void TestSystemEnumParser()
        {
            var f1 = EnumParser.System.Parse(typeof(TestEnum2), "0");
            Assert.AreEqual(TestEnum2.Val0, f1);

            var f2 = EnumParser.System.Parse<TestEnum2?>("Val0");
            Assert.AreEqual(TestEnum2.Val0, f2);

            var f3 = EnumParser.System.Parse<TestEnum2>("val0", true);
            Assert.AreEqual(TestEnum2.Val0, f3);

            var f4 = EnumParser.System.Parse(typeof(TestEnum2?), "val0", true);
            Assert.AreEqual(TestEnum2.Val0, f4);

            var f5 = EnumParser.System.Parse(typeof(TestEnum2?), "null", true);
            Assert.AreEqual(null, f5);

            var f6 = EnumParser.System.Parse(typeof(TestEnum2?), "");
            Assert.AreEqual(null, f6);


            var bf1 = EnumParser.System.TryParse(typeof(TestEnum2), "0", out var rr1);
            Assert.AreEqual(true, bf1);
            Assert.AreEqual(TestEnum2.Val0, rr1);

            var bf2 = EnumParser.System.TryParse<TestEnum2?>("Val0", out var rr2);
            Assert.AreEqual(true, bf2);
            Assert.AreEqual(TestEnum2.Val0, rr2);

            var bf3 = EnumParser.System.TryParse<TestEnum2>("val0", true, out var rr3);
            Assert.AreEqual(true, bf3);
            Assert.AreEqual(TestEnum2.Val0, rr3);

            var bf4 = EnumParser.System.TryParse(typeof(TestEnum2?), "val0", true, out var rr4);
            Assert.AreEqual(true, bf4);
            Assert.AreEqual(TestEnum2.Val0, rr4);

            var bf5 = EnumParser.System.TryParse(typeof(TestEnum2?), "null", true, out var rr5);
            Assert.AreEqual(true, bf5);
            Assert.AreEqual(null, rr5);

            var bf6 = EnumParser.System.TryParse(typeof(TestEnum2?), "", out var rr6);
            Assert.AreEqual(true, bf6);
            Assert.AreEqual(null, rr6);
        }

        [Test]
        public void TestDefaultEnumParser()
        {
            var f1 = EnumParser.Default.Parse(typeof(TestEnum2), "0");
            Assert.AreEqual(TestEnum2.Val0, f1);

            var f2 = EnumParser.Default.Parse<TestEnum2?>("Val0");
            Assert.AreEqual(TestEnum2.Val0, f2);

            var f3 = new EnumParser(stringComparer: null).Parse<TestEnum2>("Val0");
            Assert.AreEqual(TestEnum2.Val0, f3);

            var f4 = EnumParser.DefaultIgnoreCase.Parse(typeof(TestEnum2?), "val0");
            Assert.AreEqual(TestEnum2.Val0, f4);

            var f5 = EnumParser.DefaultIgnoreCase.Parse(typeof(TestEnum2?), "null");
            Assert.AreEqual(null, f5);

            var f6 = EnumParser.Default.Parse(typeof(TestEnum2?), "");
            Assert.AreEqual(null, f6);

            var f7 = EnumParser.Default.Parse(typeof(TestEnum2?), null);
            Assert.AreEqual(null, f7);

            

            var f8 = EnumParser.DefaultIgnoreCase.Parse<TestEnum2>(new SomeClase(1));
            Assert.AreEqual(TestEnum2.Val1, f8);

            var bf1 = EnumParser.Default.TryParse(typeof(TestEnum2), 0, out var rr1);
            Assert.AreEqual(true, bf1);
            Assert.AreEqual(TestEnum2.Val0, rr1);

            var bf2 = EnumParser.Default.TryParse<TestEnum2?>(TestEnum1.Val0, out var rr2);
            Assert.AreEqual(true, bf2);
            Assert.AreEqual(TestEnum2.Val0, rr2);

            var bf3 = EnumParser.Default.TryParse<TestEnum2?>(TestEnum1.Val3, out var rr3);
            Assert.AreEqual(false, bf3);

            var bf4 = new EnumParser(nullComparer: x => x == "NULL").TryParse<TestEnum2?>("null", out var rr4);
            Assert.AreEqual(false, bf4);

            Assert.Throws<OverflowException>(() => EnumParser.Default.Parse<TestEnum1>("SomeValue"));
            Assert.Throws<ArgumentNullException>(() => EnumParser.Default.Parse(null, "0"));
            Assert.Throws<ArgumentException>(() => EnumParser.Default.Parse(typeof(int), "0"));

        }
    }
}
