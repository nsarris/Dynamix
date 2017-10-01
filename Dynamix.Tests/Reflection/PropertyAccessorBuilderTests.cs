using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamix;
using Dynamix.Expressions;
using Dynamix.Expressions.LambdaBuilders;
using Dynamix.Reflection;

namespace Dynamix.Tests.Reflection
{
    [TestFixture]
    class PropertyAccessorBuilderTests
    {
        private class TestClassBase
        {
            protected int ID { get; set; }
            protected static string StaticText { get; set; }
            static TestClassBase()
            {
                StaticText = "test";
            }
        }
        private class TestClass : TestClassBase
        {
            public TestClass(int ID)
            {
                this.ID = ID;
            }

            public int GetID() { return ID; }

            public bool BoolProp { get; set; }
            public int this[int i, string s]
            {
                get { return I; }
                set { I = i; S = s; ID = value; }
            }

            public int I { get; set; }
            public string S { get; set; }
        }

        [Test]
        public void TestPropertyAccessorBuilders()
        {
            var pID = typeof(TestClass).GetProperty("ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            var pBool = typeof(TestClass).GetProperty("BoolProp", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            var pIndexed = typeof(TestClass).GetProperty("Item", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            var pStatic = typeof(TestClass).GetProperty("StaticText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);

            var builder = new PropertyAccessorLambdaBuilder();
            
            var getterID = builder.BuildInstanceGetter<TestClass,int>(pID).Compile();
            var setterID = builder.BuildInstanceSetter<TestClass, int>(pID).Compile();
            //var getterID = builder.BuildGetterExpression(pID).Compile();
            //var setterID = builder.BuildSetterExpression(pID).Compile();

            var o = new TestClass(1)
            {
                BoolProp = true,
                I = 9
            };

            var id = getterID.Invoke(o);
            Assert.True(id.Equals(1));

            setterID.Invoke(o, 2);
            Assert.True(o.GetID() == 2);
            
            var getterBool = builder.BuildInstanceGetter<TestClass, bool>(pBool).Compile();
            var setterBool = builder.BuildInstanceSetter<TestClass, bool>(pBool).Compile();

            var b = getterBool.Invoke(o);
            Assert.True(b);

            setterBool.Invoke(o, false);
            Assert.True(!o.BoolProp);
            
            var getterIndex = builder.BuildInstanceGetter<TestClass,long,object,object>(pIndexed).Compile();
            var setterIndex = builder.BuildInstanceSetter<TestClass,long, object,object>(pIndexed).Compile();

            var idx = getterIndex.Invoke(o, 10,"test");
            Assert.True((int)idx == 9);

            setterIndex.Invoke(o, 10, "test", 20);
            Assert.True(o.I == 10 & o.S == "test" & o.GetID() == 20);


            var getterStatic = builder.BuildStaticGetter<string>(pStatic).Compile();
            var setterStatic = builder.BuildStaticSetter<string>(pStatic).Compile();


            var t = getterStatic.Invoke();
            Assert.True(t == "test");

            setterStatic.Invoke("test2");
            t = getterStatic.Invoke();
            Assert.True(t == "test2");
        }

        [Test]
        public void TestPropertyInfoEx()
        {
            var obj = new TestClass(1)
            {
                BoolProp = false,
                I = 11,
                S = "bb"
            };

            var props = typeof(TestClass).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .ToDictionary(x => x.Name, x => new PropertyInfoEx(x));

            props["ID"].Set(obj, 2);
            props["BoolProp"].Set(obj, true);
            props["I"].Set(obj, 10);
            props["S"].Set(obj, "dd");
            

            Assert.True(props["ID"].Get(obj).Equals(2));
            Assert.True(props["BoolProp"].Get(obj).Equals(true));
            Assert.True(props["I"].Get(obj).Equals(10));
            Assert.True(props["S"].Get(obj).Equals("dd"));
            Assert.True(props["Item"].Get(obj,0,"e").Equals(obj.I));

            props["Item"].Set(obj, 12, 0, "e");
            Assert.True(props["ID"].Get(obj).Equals(12));
            Assert.True(props["I"].Get(obj).Equals(0));
            Assert.True(props["S"].Get(obj).Equals("e"));

            props["StaticText"].Set("text2");
            Assert.True(props["StaticText"].Get().Equals("text2"));
        }
    }
}
