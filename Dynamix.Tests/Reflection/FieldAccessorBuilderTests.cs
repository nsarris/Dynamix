using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamix;
using Dynamix.Reflection.Emit;
using Dynamix.Expressions;
using Dynamix.Expressions.LambdaBuilders;
using Dynamix.Reflection;

namespace Dynamix.Tests.Reflection
{
    [TestFixture]
    class FieldAccessorBuilderTests
    {
        private class TestClassBase
        {
            protected int ID;
            protected static string StaticText;
        }
        private class TestClass : TestClassBase
        {
            static TestClass()
            {
                //SR = 4;// new Random().Next(0, 10);
            }
            public TestClass(int ID)
            {
                this.ID = ID;

            }

            public int GetID() { return ID; }

            public bool BoolProp;

            public static readonly int SR = 5;


            public int I;
            public string S;
            public string TestFind { get; set; }
        }

        private class TestClass<T> : TestClass
        {
            public TestClass(int ID)
                : base(ID)
            {

            }

            public int GenericTypeFoo;
            public T GenericFoo;
        }

        [Test]
        public void TestFieldAccessorBuilders()
        {
            var bf = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;

            var pID = typeof(TestClass).GetField("ID", bf);
            var pBool = typeof(TestClass).GetField("BoolProp", bf);

            var pgtf = typeof(TestClass<>).GetField("GenericTypeFoo", bf);
            var pgf = typeof(TestClass<>).GetField("GenericFoo", bf);

            var builder = new FieldAccessorLambdaBuilder();

            var getterID = builder.BuildInstanceGetter<object, object>(pID).Compile();
            var setterID = builder.BuildInstanceSetter<object, object>(pID).Compile();
            //var getterID = builder.BuildGetterExpression(pID).Compile();
            //var setterID = builder.BuildSetterExpression(pID).Compile();

            var o = new TestClass(1)
            {
                BoolProp = true,
            };

            var id = getterID.Invoke(o);
            Assert.True(id.Equals(1));

            setterID.Invoke(o, 2);
            Assert.True(o.GetID() == 2);

            var getterBool = builder.BuildInstanceGetter<object, bool>(pBool).Compile();
            var setterBool = builder.BuildInstanceSetter<TestClass, bool>(pBool).Compile();

            var b = getterBool.Invoke(o);
            Assert.True(b);

            setterBool.Invoke(o, false);
            Assert.True(!o.BoolProp);


            var anon = new
            {
                ID = 1
            };

            var anonField = anon.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).First();
            var anonGetterID = builder.BuildInstanceGetter<object, int>(anonField).Compile();
            var anonSetterID = builder.BuildInstanceSetter<object, int>(anonField).Compile();

            var idanon = anonGetterID.Invoke(anon);
            Assert.True(1.Equals(idanon));

            anonSetterID.Invoke(anon, 2);
            Assert.True(anon.ID == 2);


            var staticReadOnlyField = typeof(TestClass).GetField("SR", bf);
            var sroGetter = builder.BuildStaticGetter<int>(staticReadOnlyField).Compile();
            var sroSetter = builder.BuildStaticSetter<int>(staticReadOnlyField).Compile();

            var sro1 = sroGetter.Invoke();
            Assert.True(5.Equals(sro1));

            sroSetter.Invoke(2);
            Assert.True(TestClass.SR == 2);


            var emmitedGetter = FieldAccessorMethodEmitter.GetFieldGetterMethod(anonField);
            var idanon1 = emmitedGetter.Invoke(null, new object[] { anon });
            Assert.True(1.Equals(idanon));

            //var sr = 5;// TestClass.SR;

            var emmitedStaticReadonlyGetter = FieldAccessorMethodEmitter.GetFieldGetterMethod(staticReadOnlyField);
            var sro = emmitedStaticReadonlyGetter.Invoke(null, new object[] { });
            Assert.True(2.Equals(sro));
            var tmp = (int)sro;

            var emmitedStaticReadonlySetter = FieldAccessorMethodEmitter.GetFieldSetterMethod(staticReadOnlyField);
            emmitedStaticReadonlySetter.Invoke(null, new object[] { 6 });

            Assert.True(TestClass.SR == 6);

        }

        [Test]
        public void TestFieldInfoEx()
        {
            var obj = new TestClass(1)
            {
                BoolProp = false,
                I = 11,
                S = "bb"
            };

            var fields = typeof(TestClass).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .ToDictionary(x => x.Name, x => new FieldInfoEx(x));

            fields["ID"].Set(obj, 2);
            fields["BoolProp"].Set(obj, true);
            fields["I"].Set(obj, 10);
            fields["S"].Set(obj, "dd");


            Assert.True(fields["ID"].Get(obj).Equals(2));
            Assert.True(fields["BoolProp"].Get(obj).Equals(true));
            Assert.True(fields["I"].Get(obj).Equals(10));
            Assert.True(fields["S"].Get(obj).Equals("dd"));

            fields["StaticText"].Set("text2");
            Assert.True(fields["StaticText"].Get().Equals("text2"));


        }
    }
}
