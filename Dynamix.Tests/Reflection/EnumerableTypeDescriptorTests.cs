using Dynamix.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ExpectedObjects;
using System.Collections;

namespace Dynamix.Tests.Reflection
{
    [TestFixture]
    public class EnumerableTypeDescriptorTests
    {
        class TestListClass<T> : List<T>//, ITestListClass<T>
        {

        }

        interface ITestListClass<T,G> : IList<T>, IDictionary<T,G>
        {

        }

        interface ITestListClass<T> : ITestListClass<T, object>
        {

        }

        [Test]
        public void ArrayTests()
        {
            //Array
            var enumerableTypeDescriptor = new EnumerableTypeDescriptor(typeof(Array));

            enumerableTypeDescriptor.IsArray
                .Should().BeTrue();

            enumerableTypeDescriptor.IsIndexed
                .Should().BeTrue();

            (enumerableTypeDescriptor.IsCollection ||
                enumerableTypeDescriptor.IsDictionary ||
                enumerableTypeDescriptor.IsGeneric ||
                enumerableTypeDescriptor.IsInterface ||
                enumerableTypeDescriptor.IsList)
                .Should().BeFalse();

            enumerableTypeDescriptor.IsReadOnly
                .Should().BeFalse();

            enumerableTypeDescriptor.ElementType
                .Should().Be(typeof(object));

            //Typed array
            enumerableTypeDescriptor = new EnumerableTypeDescriptor(typeof(int[]));

            enumerableTypeDescriptor.IsArray
                .Should().BeTrue();

            enumerableTypeDescriptor.IsIndexed
                .Should().BeTrue();

            (enumerableTypeDescriptor.IsCollection ||
                enumerableTypeDescriptor.IsDictionary ||
                enumerableTypeDescriptor.IsGeneric ||
                enumerableTypeDescriptor.IsInterface ||
                enumerableTypeDescriptor.IsList)
                .Should().BeFalse();

            enumerableTypeDescriptor.IsReadOnly
                .Should().BeFalse();

            enumerableTypeDescriptor.ElementType
                .Should().Be(typeof(int));

            var data = new int[] { 1, 2, 3, 4, 5 };

            TestEnumerations(enumerableTypeDescriptor, data);
        }

        [Test]
        public void ClassTests()
        {
            var enumerableTypeDescriptor = new EnumerableTypeDescriptor(typeof(TestListClass<int>));

            (enumerableTypeDescriptor.IsList
            && enumerableTypeDescriptor.IsGeneric
            && enumerableTypeDescriptor.IsIndexed)
                .Should().BeTrue();

            (enumerableTypeDescriptor.IsCollection ||
                enumerableTypeDescriptor.IsDictionary ||
                enumerableTypeDescriptor.IsInterface ||
                enumerableTypeDescriptor.IsArray)
                .Should().BeFalse();

            enumerableTypeDescriptor.IsReadOnly
                .Should().BeFalse();

            enumerableTypeDescriptor.ElementType
                .Should().Be(typeof(int));

            var data = new TestListClass<int> { 1, 2, 3, 4, 5 };

            TestEnumerations(enumerableTypeDescriptor, data);
        }

        [Test]
        public void InterfaceTests()
        {
            var enumerableTypeDescriptor = new EnumerableTypeDescriptor(typeof(ITestListClass<int>));

            (enumerableTypeDescriptor.IsInterface
            && enumerableTypeDescriptor.IsGeneric
            && enumerableTypeDescriptor.IsIndexed)
                .Should().BeTrue();

            (enumerableTypeDescriptor.IsList ||
            enumerableTypeDescriptor.IsCollection ||
            enumerableTypeDescriptor.IsDictionary ||
            enumerableTypeDescriptor.IsArray)
                .Should().BeFalse();

            enumerableTypeDescriptor.IsReadOnly
                .Should().BeFalse();

            enumerableTypeDescriptor.ElementType
                .Should().Be(typeof(int));

            var data = new TestListClass<int> { 1, 2, 3, 4, 5 };

            TestEnumerations(enumerableTypeDescriptor, data);
        }

        private void TestEnumerations<T>(EnumerableTypeDescriptor enumerableTypeDescriptor, IEnumerable<T> data)
        {
            data.ToArray().ToExpectedObject()
                .ShouldEqual(enumerableTypeDescriptor.ToArray(data));

            data.ToArray().ToExpectedObject()
                .ShouldEqual(enumerableTypeDescriptor.ToArray<T>(data));

            data.ToList().ToExpectedObject()
                .ShouldEqual(enumerableTypeDescriptor.ToList(data));

            data.ToList().ToExpectedObject()
                .ShouldEqual(enumerableTypeDescriptor.ToList<T>(data));

            new System.Collections.ObjectModel.ReadOnlyCollection<T>(data.ToList()).ToExpectedObject()
                .ShouldEqual(enumerableTypeDescriptor.ToReadOnlyList<T>(data));

            data.ToList().ToExpectedObject()
                .ShouldEqual(enumerableTypeDescriptor.AsEnumerable(data).Cast<T>().ToList());

            data.ToList().ToExpectedObject()
                .ShouldEqual(enumerableTypeDescriptor.AsEnumerable<T>(data).ToList());

            data.ToList().ToExpectedObject()
                .ShouldEqual(enumerableTypeDescriptor.AsEnumerableOfElementType(data).Cast<T>().ToList());

            data.ToList().ToExpectedObject()
                .ShouldEqual(Enumerate(enumerableTypeDescriptor, data).Cast<T>().ToList());

            data.ToList().ToExpectedObject()
                .ShouldEqual(Enumerate<T>(enumerableTypeDescriptor, data).ToList());
        }

        private IEnumerable Enumerate(EnumerableTypeDescriptor enumerableTypeDescriptor, IEnumerable enumerable)
        {
            var e = enumerableTypeDescriptor.GetEnumerator(enumerable);
            while (e.MoveNext())
            {
                yield return e.Current;
            }
        }

        private IEnumerable<T> Enumerate<T>(EnumerableTypeDescriptor enumerableTypeDescriptor, IEnumerable enumerable)
        {
            var e = enumerableTypeDescriptor.GetEnumerator(enumerable);
            while (e.MoveNext())
            {
                yield return (T)e.Current;
            }
        }
    }
}
