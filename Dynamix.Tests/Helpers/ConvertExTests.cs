using Dynamix.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Tests.Helpers
{
    [TestFixture]
    public class ConvertExTests
    {
        [Test]
        public void Should_Convert_Int_To_Long()
        {
            var v1 = ConvertEx.ConvertTo(3, typeof(long));
            var v2 = ConvertEx.Convert<long>(3);

            Assert.True(v1 is long l1 && l1 == 3);
            Assert.True(v2 == 3);
        }

        [Test]
        public void Should_Convert_IntNullable_To_Long()
        {
            int? i = 3;

            var v1 = ConvertEx.ConvertTo(i, typeof(long));
            var v2 = ConvertEx.Convert<long>(i);

            Assert.True(v1 is long l1 && l1 == 3);
            Assert.True(v2 == 3);
        }

        [Test]
        public void Should_Convert_IntNullable_To_LongNullable()
        {
            int? i = 3;

            var v1 = ConvertEx.ConvertTo(i, typeof(long?));
            var v2 = ConvertEx.Convert<long?>(i);

            Assert.True(v1 is long l1 && l1 == 3);
            Assert.True(v2 == 3);
        }

        [Test]
        public void Should_Convert_Int_To_LongNullable()
        {
            var v1 = ConvertEx.ConvertTo(3, typeof(long?));
            var v2 = ConvertEx.Convert<long?>(3);

            Assert.True(v1 is long l1 && l1 == 3);
            Assert.True(v2 == 3);
        }
    }
}
