using Dynamix.Expressions.PredicateBuilder;
using Dynamix.Expressions.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Tests.Expressions
{
    class Dto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthdate { get; set; }
        public TimeSpan TimeSince { get; set; }
        public bool Active { get; set; }
    }

    [TestFixture]
    class PredicateTreeTests
    {
        [Test]
        public void TestPredicateTree()
        {
            var builder = new PredicateTreeBuilder(typeof(Dto));

            builder.Has("Id", ExpressionOperator.Equals, 1)
                .And("Name", ExpressionOperator.IsNotNullOrEmpty, null)
                .And(inner => inner
                    .Has("Birthdate.Year", ExpressionOperator.LessThanOrEqual, 2000)
                        .Or("TimeSince.Hours", ExpressionOperator.GreaterThan, 1))
                .Or("Active", ExpressionOperator.DoesNotEqual, false);

            var e = new ExpressionNodeVisitor().VisitLambda<Dto>(
                builder.RootNode, 
                new PredicateBuilderConfiguration()
                    .WithItParameterName("x")
                );

            Expression<Func<Dto, bool>> testExpression = (Dto x) =>
                 ((x.Id == 1 && !(x.Name == "" || x.Name == null)) && (x.Birthdate.Year <= 2000 || x.TimeSince.Hours > 1)) || x.Active != true;

            Assert.AreEqual(
                testExpression.ToString(),
                e.ToString());

            Assert.True(testExpression.CheckEquals(e));
        }
    }
}
