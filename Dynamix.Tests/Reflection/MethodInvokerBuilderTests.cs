using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamix;
using System.Linq.Expressions;
using Dynamix.Expressions.LambdaBuilders;
using Dynamix.Reflection.DelegateBuilders;
using Dynamix.Expressions;
using Dynamix.Reflection;

namespace Dynamix.Tests.Reflection
{
    class TestTargetClass
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public int Foo(int a, int b)
        {
            return a + b;
        }

        public TestTargetClass()
        {

        }

        public TestTargetClass(string name)
        {
            this.FirstName = name;
        }

        public void VoidFoo(string name, DateTime dob) { this.FirstName = name; this.DOB = dob; }
        public static bool StaticFoo(int a, int b) { return a == b; }


        public bool GenericFoo<T>(T a, T b) { return a.ToString() == ToString() && b.ToString() == ToString(); }
        public void GenericVoidFoo<T>(T a, string name) { LastName = a.ToString(); FirstName = name; }
        public static bool StaticGenericFoo<T>(T a, T b) { return a.ToString() == b.ToString(); }
        public static void StaticGenericVoidFoo<T>(T a, string name) { }


        public override string ToString()
        {
            return (this.FirstName + " " + this.LastName).Trim();
        }

    }

    static class TargetTestClassExtensions
    {
        public static void StaticExtensionFooVoid(this TestTargetClass o, string name)
        {
            o.FirstName = name;
        }

        public static bool StaticExtensionFoo(this TestTargetClass o, string name)
        {
            return o.FirstName == name;
        }
    }


    [TestFixture]
    public class MethodInvokerBuilderTests
    {
        [Test]
        public void TestWrongUsage()
        {
            var extm = typeof(List<int>).GetExtensionsMethods(AppDomain.CurrentDomain.GetAssemblies()).ToList();
            var enumerableSumMethod = extm.Where(x => x.Name == "Sum" && !x.IsGenericMethod).FirstOrDefault();
            var builder = new MethodInvokerLambdaBuilder();

            //Assert.Throws<ArgumentNullException>(() => MethodInvokerBuilder<object>.BuildGenericInvoker(null));
            Assert.Throws<ArgumentNullException>(() => builder.BuildFromTypes(null));

            Assert.Throws<ArgumentException>(() => builder.BuildGenericStatic(typeof(TestTargetClass).GetMethod("VoidFoo")));
            //Assert.Throws<ArgumentException>(() => builder.BuildGenericExtension(typeof(TestTargetClass).GetMethod("Foo")));
            Assert.Throws<ArgumentException>(() => builder.BuildGenericStatic(typeof(TestTargetClass).GetMethod("Foo")));
            //Assert.Throws<ArgumentException>(() => builder.BuildGenericInstance(typeof(TestTargetClass).GetMethod("StaticFoo")));
            //Assert.Throws<ArgumentException>(() => MethodInvokerBuilder<object>.BuildInvoker(typeof(TestTargetClass).GetMethod("Foo"), true));

            //Assert.Throws<ArgumentException>(() => builder.BuildTypedInvokerExpression(typeof(TestTargetClass).GetMethod("Foo")));
            //Assert.Throws<ArgumentException>(() => builder.BuildTypedInvokerExpression(typeof(TestTargetClass).GetMethod("VoidFoo"),typeof(TestTargetClass),new[] { typeof(int), typeof(int) }, typeof(int))); //<Func<TestTargetClass, int, int, int>>
            //Assert.Throws<ArgumentException>(() => builder.BuildTypedInvokerExpression(enumerableSumMethod,null,new[] { typeof(int), typeof(int) }, typeof(bool))); //Func<int, int, bool>
            //Assert.Throws<ArgumentException>(() => builder.BuildTypedInvokerExpression(typeof(TestTargetClass).GetMethod("StaticFoo"), typeof(IEnumerable<int>),new[] { typeof(int) })); //Func<IEnumerable<int>, int>
        }

        [Test]
        public void TestNonGenericMethodBuilders()
        {
            var extm = typeof(List<int>).GetExtensionsMethods(AppDomain.CurrentDomain.GetAssemblies()).ToList();
            var enumerableSumMethod = extm.Where(x => x.Name == "Sum" && !x.IsGenericMethod).FirstOrDefault();
            var enumerableAddMethod = extm.Where(x => x.Name == "Add").FirstOrDefault();
            var l = new List<int>() { 1, 2, 3 };

            var extmethods = typeof(TestTargetClass).GetExtensionsMethods();
            var extmethod = extmethods.Where(x => x.Name == nameof(TargetTestClassExtensions.StaticExtensionFoo)).FirstOrDefault();
            var extmethodvoid = extmethods.Where(x => x.Name == nameof(TargetTestClassExtensions.StaticExtensionFooVoid)).FirstOrDefault();


            //var m1 = MethodInvokerBuilder.BuildInstanceInvoker(typeof(TestTargetClass).GetMethod("VoidFoo"));
            //var m2 = MethodInvokerBuilder.BuildInstanceInvoker(typeof(TestTargetClass).GetMethod("Foo"));
            //var m3 = MethodInvokerBuilder.BuildStaticInvoker(typeof(TestTargetClass).GetMethod("StaticFoo"));
            //var m4 = MethodInvokerBuilder.BuildExtensionAsInstanceInvoker(extmethod);
            //var m5 = MethodInvokerBuilder.BuildStaticInvoker(extmethod);
            //var m6 = MethodInvokerBuilder.BuildStaticInvoker(extmethodvoid);
            //var m7 = MethodInvokerBuilder.BuildExtensionAsInstanceInvoker(extmethodvoid);

            var builder = new MethodInvokerLambdaBuilder();
            var m1 = (GenericInstanceInvoker)builder.BuildGenericInstance(typeof(TestTargetClass).GetMethod("VoidFoo")).Compile();
            var m2 = (GenericInstanceInvoker)builder.BuildGenericInstance(typeof(TestTargetClass).GetMethod("Foo")).Compile();
            var m3 = (GenericStaticInvoker)builder.BuildGenericStatic(typeof(TestTargetClass).GetMethod("StaticFoo")).Compile();
            var m3b = (GenericInstanceInvoker)builder.BuildGenericInstance(typeof(TestTargetClass).GetMethod("StaticFoo")).Compile();
            var m4 = (GenericInstanceInvoker)builder.BuildGenericInstance(extmethod).Compile();
            var m5 = (GenericStaticInvoker)builder.BuildGenericStatic(extmethod).Compile();
            var m6 = (GenericStaticInvoker)builder.BuildGenericStatic(extmethodvoid).Compile();
            var m7 = (GenericInstanceInvoker)builder.BuildGenericInstance(extmethodvoid).Compile();

            var e8 = builder.BuildGenericInstance(extmethodvoid);
            var d1 = (GenericInstanceInvoker)e8.CompileCached();
            var d2 = (GenericInstanceInvoker)e8.CompileCached();
            var d3 = (GenericInstanceInvoker)e8.CompileCached();
            var d4 = (GenericInstanceInvoker)e8.Compile();
            Assert.IsTrue((d1 == d2) & (d2 == d3));
            Assert.IsTrue(d4 != d1);


            var o = new TestTargetClass();
            var nameValue = "Jim";
            var dateValue = DateTime.Now;
            var r1 = m1.Invoke(o, nameValue, dateValue);
            Assert.IsTrue(r1 == null & o.FirstName == nameValue & o.DOB == dateValue);

            var r2 = m2.Invoke(o, 2, 3);
            Assert.IsTrue(r2.GetType() == typeof(int) & (int)r2 == 5);

            var r3 = m3.Invoke(2, 2);
            Assert.IsTrue(r3 != null & r3.Equals(true));
            var r3b = m3b.Invoke(null, 3, 3);
            Assert.IsTrue(r3b != null & r3b.Equals(true));

            var r4 = m4.Invoke(o, nameValue);
            Assert.IsTrue(r4 != null & r4.Equals(true));

            var r5 = m5.Invoke(o, nameValue);
            Assert.IsTrue(r5 != null & r5.Equals(true));

            o.FirstName = "";
            var r6 = m6.Invoke(o, nameValue);
            Assert.IsTrue(r6 == null & o.FirstName.Equals(nameValue));

            o.FirstName = "";
            var r7 = m7.Invoke(o, nameValue);
            Assert.IsTrue(r7 == null & o.FirstName.Equals(nameValue));


            var m1t = builder.BuildFromDelegate<Action<TestTargetClass, string, DateTime>>(typeof(TestTargetClass).GetMethod("VoidFoo")).Compile();
            var m2t = builder.BuildFuncInstance<TestTargetClass, int, int, int>(typeof(TestTargetClass).GetMethod("Foo")).Compile();
            var m3t = builder.BuildFromDelegate<Func<int, int, bool>>(typeof(TestTargetClass).GetMethod("StaticFoo")).Compile();
            //var m4t = builder.BuildFuncInstance<IEnumerable<int>, int>(enumerableSumMethod).Compile();
            var m4t = builder.BuildFuncStatic<IEnumerable<int>, int>(enumerableSumMethod).Compile();

            var m2td = builder.BuildFromTypes(typeof(TestTargetClass).GetMethod("Foo")).Compile();
            var m3td = builder.BuildFromTypes(typeof(TestTargetClass).GetMethod("StaticFoo")).Compile();

            o = new TestTargetClass();
            var r1t = m1.Invoke(o, nameValue, dateValue);
            Assert.IsTrue(o.FirstName == nameValue & o.DOB == dateValue);

            var r2t = m2t.Invoke(o, 2, 3);
            Assert.IsTrue(r2t == 5);

            var r3t = m3t.Invoke(2, 2);
            Assert.IsTrue(r3t);

            var r4t = m4t.Invoke(l);
            Assert.IsTrue(r4t == 6);

            var r2td = m2td.DynamicInvoke(o, 2, 3);
            Assert.IsTrue((int)r2td == 5);

            var r3td = m3td.DynamicInvoke(2, 2);
            Assert.IsTrue((bool)r3td);

            var ctorBuilder = new ConstructorInvokerLambdaBuilder();
            var ctors = typeof(TestTargetClass).GetConstructors();
            var ctor1 = ctors.First();
            var ctor2 = ctors.Skip(1).First();

            var c1 = ((Expression<Func<TestTargetClass>>)ctorBuilder.BuildFromTypes(ctor1)).Compile().Invoke();
            Assert.True(c1 != null & c1.GetType() == typeof(TestTargetClass));

            var c2 = ((Expression<Func<string, TestTargetClass>>)ctorBuilder.BuildFromTypes(ctor2)).Compile().Invoke("test");
            Assert.True(c2 != null & c2.GetType() == typeof(TestTargetClass) & c2.FirstName == "test");

            var ctorBuilder2 = new ConstructorInvokerLambdaBuilder();
            var ctors2 = typeof(TestTargetClass).GetConstructors();
            var ctor12 = ctors2.First();
            var m11 = ctorBuilder.BuildFromTypes(ctor12);
        }

        [Test]
        public void TestGenericMethodBuilders()
        {
            var test = new TestTargetClass() { FirstName = "Jim", LastName = "Beam" };
            var testa = new TestTargetClass() { FirstName = "Jim Beam", LastName = "" };
            var testb = new TestTargetClass() { FirstName = "", LastName = "Jim Beam" };
            var builder = new MethodInvokerLambdaBuilder();

            var m = (GenericInstanceInvoker)builder.BuildGenericInstance(typeof(TestTargetClass).GetMethod("GenericFoo").MakeGenericMethodCached(new Type[] { typeof(TestTargetClass) })).Compile();
            var r = m.Invoke(test, testa, testb);
            Assert.IsTrue((bool)r);

            var mt = builder.BuildFuncInstance<TestTargetClass, TestTargetClass, TestTargetClass, bool>(typeof(TestTargetClass).GetMethod("GenericFoo").MakeGenericMethodCached(new Type[] { typeof(TestTargetClass) })).Compile();
            var rt = mt.Invoke(test, testa, testb);
            Assert.IsTrue(rt);
        }
    }
}
