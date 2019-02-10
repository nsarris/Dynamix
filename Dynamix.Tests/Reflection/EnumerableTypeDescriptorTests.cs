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
        class TestListClass<T> : List<T>, ITestListClass<T>
        {
            object IDictionary<T, object>.this[T key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            ICollection<T> IDictionary<T, object>.Keys => throw new NotImplementedException();

            ICollection<object> IDictionary<T, object>.Values => throw new NotImplementedException();

            int ICollection<KeyValuePair<T, object>>.Count => throw new NotImplementedException();

            bool ICollection<KeyValuePair<T, object>>.IsReadOnly => throw new NotImplementedException();

            void IDictionary<T, object>.Add(T key, object value)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<T, object>>.Add(KeyValuePair<T, object> item)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<T, object>>.Clear()
            {
                throw new NotImplementedException();
            }

            bool ICollection<KeyValuePair<T, object>>.Contains(KeyValuePair<T, object> item)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<T, object>.ContainsKey(T key)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<T, object>>.CopyTo(KeyValuePair<T, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            IEnumerator<KeyValuePair<T, object>> IEnumerable<KeyValuePair<T, object>>.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            bool IDictionary<T, object>.Remove(T key)
            {
                throw new NotImplementedException();
            }

            bool ICollection<KeyValuePair<T, object>>.Remove(KeyValuePair<T, object> item)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<T, object>.TryGetValue(T key, out object value)
            {
                throw new NotImplementedException();
            }
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
            var enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(typeof(Array));

            enumerableTypeDescriptor.ExplicitImplementation.Kind
                .Should().Be(EnumerableInterfaceKind.List);

            enumerableTypeDescriptor.ExplicitImplementation.IsReadOnly
                .Should().Be(false);

            enumerableTypeDescriptor.ExplicitImplementation.ElementType
                .Should().Be(typeof(object));

            //Typed array
            enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(typeof(int[]));

            enumerableTypeDescriptor.ExplicitImplementation.Kind
                .Should().Be(EnumerableInterfaceKind.List);

            enumerableTypeDescriptor.ExplicitImplementation.IsReadOnly
                .Should().Be(false);

            enumerableTypeDescriptor.ExplicitImplementation.ElementType
                .Should().Be(typeof(int));

            
        }

        [Test]
        public void ClassTests()
        {
            var enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(typeof(TestListClass<int>));

            enumerableTypeDescriptor.ExplicitImplementation.Kind
                .Should().Be(EnumerableInterfaceKind.List);

            enumerableTypeDescriptor.ExplicitImplementation.IsReadOnly
                .Should().Be(false);

            enumerableTypeDescriptor.ExplicitImplementation.ElementType
                .Should().Be(typeof(int));

        }

        [Test]
        public void InterfaceTests()
        {
            var enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(typeof(ITestListClass<>));

            enumerableTypeDescriptor.ExplicitImplementation
                .Should().BeNull();
        }

        [Test]
        public void CloningTests()
        {
            var data = Enumerable.Range(0, 10).ToArray();
            var test1 = ((IEnumerable)data).Clone();
            data.ToExpectedObject().ShouldEqual(test1);

            var test2 = ((IEnumerable)data).CloneTo(typeof(CloneableListWithAdd));

            var cl = new CloneableListWithAdd();
            foreach (var item in data)
                cl.Add(item);
            cl.ToExpectedObject().ShouldEqual(test2);

            var test3 = ((IEnumerable)data).CloneTo(typeof(CloneableList));
            var cl2 = new CloneableList(data);
            cl2.ToExpectedObject().ShouldEqual(test3);
        }

        [Test]
        public void CreateMatchingInstanceTests()
        {
            AssertMatchingType<IList, List<object>>();
            AssertMatchingType(typeof(IList<>), typeof(List<int>), new[] { typeof(int) });

            AssertMatchingType<int[], int[]>();

            AssertMatchingType<CloneableListBase, CloneableListWithAdd>();
        }

        public void AssertMatchingType<TSource, TDestination>(Type[] genericTypeArguments = null)
            => AssertMatchingType(typeof(TSource), typeof(TDestination), genericTypeArguments);

        public void AssertMatchingType(Type sourceType, Type destinationType, Type[] genericTypeArguments = null)
        {
            EnumerableTypeDescriptor.Get(sourceType).CreateBestMatchingInstance(genericTypeArguments).GetType().Should().Be(destinationType);

        }

        public interface ICloneableList : IEnumerable { }

        public abstract class CloneableListBase : ICloneableList
        {
            protected readonly List<object> items = new List<object>();

            protected CloneableListBase()
            {

            }
        
            public IEnumerator GetEnumerator()
            {
                return items.GetEnumerator();
            }
        }

        public class CloneableListWithAdd : CloneableListBase
        {
            public void Add(object item)
            {
                items.Add(item);
            }
        }

        public class CloneableList : CloneableListWithAdd
        {
            public CloneableList(IEnumerable<int> data) 
            {
                foreach (var item in data)
                    items.Add(item);
            }
        }
    }
}
