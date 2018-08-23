using Dynamix.DynamicProjection;
using Dynamix.Expressions.PredicateBuilder;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Tests
{
    [TestFixture]
    class DynamicProjectionTests
    {
        class Model
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Active { get; set; }

            public Model(bool active)
            {
                Active = active;
            }
        }
        [Test]
        public void TestDynamicProjection()
        {
            var data = new List<Model>
            {
                new Model(true) { Id = 1, Name = "Name1" },
                new Model(false) { Id = 2, Name = "Name2" },
                new Model(true) { Id = 3, Name = "Name3" },
            };

            var dp = new ProjectionBuilder(typeof(Model), null)
                .Auto("Active")
                .Member("Id", map => map
                        .FromExpression("Id"))
                .Member("Descr", map => map
                        .FromExpression("Name"))
                .Member("CodeDescr", map => map
                        .FromExpression("Id.ToString() + \" - \" + Name"))
                .Member("Constant", map => map
                        .FromValue(5))
                .BuildWithDynamicType("DynamicProjectionTestDto");


            var predicateTreeBuilder = 
                new PredicateTreeBuilder(typeof(Model))
                .Has("Id", ExpressionOperator.Equals, 1)
                .Or("CodeDescr", ExpressionOperator.IsNotNullOrEmpty, null)
                .Or("Descr.Length", ExpressionOperator.GreaterThan, 1)
                .Or("Active", ExpressionOperator.DoesNotEqual, false);

            var q = dp.BuildQuery(
                    data.AsQueryable(), 
                    new[] { "Id","Descr" }, 
                    predicateTreeBuilder.RootNode)
                .ToList();
        }
    }
}
