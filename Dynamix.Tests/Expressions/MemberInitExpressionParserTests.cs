using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Tests.Expressions
{

    [TestFixture]
    class MemberInitExpressionParserTests
    {
        public class SimpleInputClass
        {
            public int A { get; set; }
            public string B { get; set; }
            public DateTime C { get; set; }
            public List<SimpleInputInnerClass> Children {get;set;}
        }

        public class SimpleInputInnerClass
        {
            public int A { get; set; }
        }

        public class SimpleTargetClass
        {
            public int A { get; set; }
            public string B { get; set; }
            public DateTime C { get; set; }
            public List<SimpleTargetInnerClass> Children { get; set; }
        }

        public class SimpleTargetInnerClass
        {
            public int A { get; set; }
        }

        public class ComplexTargetClass
        {
            public int A { get; set; }
            public string B { get; set; }
            public DateTime C { get; set; }

            
            public long AA { get; set; }
            public string BB { get; set; }

            public List<SimpleTargetInnerClass> ListA { get; set; }
        }

        [Test]
        public void TestMemberInitExpressionParser_WithWrongExpression()
        {
            Expression<Func<SimpleInputClass, SimpleInputClass>>
                mapExpression1 = (SimpleInputClass i) => i;

            Assert.Throws<ArgumentException>(
                () =>
                {
                    var p1 = Dynamix.Expressions.InitExpressionParser.ParseInitExpression(mapExpression1);
                });
        }

        [Test]
        public void TestMemberInitExpressionParser()
        {
            var c = new SimpleInputClass()
            {
                A = 1,
                B = "1"
            };
            Expression<Func<SimpleInputClass, SimpleTargetClass>>
                mapExpression1 = (SimpleInputClass i) => new SimpleTargetClass
                {
                    A = i.A,
                    B = i.B,
                    C = i.C,
                    Children = new List<SimpleTargetInnerClass>()
                    {
                        new SimpleTargetInnerClass
                        {
                            A = 1
                        }
                    }
                };

            var p1 = Dynamix.Expressions.InitExpressionParser.ParseInitExpression(mapExpression1);

            Assert.AreEqual(p1.Members.Count, 4);
            Assert.AreEqual(p1.Members[0].MappedProperty.Name, "A");
            Assert.AreEqual(p1.Members[1].MappedProperty.Name, "B");
            Assert.AreEqual(p1.Members[2].MappedProperty.Name, "C");

            Expression<Func<SimpleInputClass, ComplexTargetClass>>
                mapExpression2 = (SimpleInputClass i) => new ComplexTargetClass
                {
                    A = i.A,
                    B = i.B,
                    C = i.C,

                    AA = i.A,
                    BB = i.B.Trim(),
                    ListA = i.Children.Where(x => true).OrderBy(x => x.A)
                        .Select(x => new SimpleTargetInnerClass
                        {
                            A = x.A
                        }).ToList()
                };

            var p2 = Dynamix.Expressions.InitExpressionParser.ParseInitExpression(mapExpression2);
            
            Assert.AreEqual(p2.Members.Count, 6);
            Assert.AreEqual(p2.Members[0].MappedProperty.Name, "A");
            Assert.AreEqual(p2.Members[1].MappedProperty.Name, "B");
            Assert.AreEqual(p2.Members[2].MappedProperty.Name, "C");

            var p3 = Dynamix.Expressions.InitExpressionParser.ParseNewExpression((SimpleInputClass i) => new
            {
                i.A,
                BB = i.B,
                CC = i.C,
            });

            Assert.AreEqual(p3.Members.Count, 3);
            Assert.AreEqual(p3.Members[0].MappedProperty.Name, "A");
            Assert.AreEqual(p3.Members[1].MappedProperty.Name, "B");
            Assert.AreEqual(p3.Members[2].MappedProperty.Name, "C");
        }
    }
}
