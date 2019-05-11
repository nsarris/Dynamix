using Dynamix.DynamicProjection;
using Dynamix.Expressions.PredicateBuilder;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ExpectedObjects;

namespace Dynamix.Tests
{
    [TestFixture]
    public class DynamicProjectionTests
    {
        class Model
        {
            private readonly bool active2;

            public int Id { get; set; }
            public string Name { get; set; }
            public bool Active { get; set; }

            public Model()
            {
                
            }

            public Model(int id, string name, bool active)
            {
                Id = id;
                Name = name;
                Active = active;
            }
        }

        [Test]
        public void TestDynamicProjection()
        {
            var data = new List<Model>
            {
                new Model() { Id = 1, Name = "Name1" },
                new Model() { Id = 2, Name = "Name2" },
                new Model() { Id = 3, Name = "Name3" },
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

        [Test]
        public void Can_Project_From_Object_To_Type_Using_Property_Assignment()
        {
            var source = new []
            {
                new { Prop1 = 1 , Prop2 = "1", Active = true },
                new { Prop1 = 2 , Prop2 = "2", Active = false },
                new { Prop1 = 3 , Prop2 = "3", Active = true },
            };

            var sourceElement = source.First();

            var projection = new ProjectionBuilder(source.First().GetType(), typeof(Model))
                .Member(nameof(Model.Id), map => map.FromExpression(nameof(sourceElement.Prop1)))
                .Member(nameof(Model.Name), map => map.FromExpression(nameof(sourceElement.Prop2)))
                .Auto(nameof(Model.Active))
                .Build();

            var result = projection
                .BuildQuery(source.AsQueryable())
                .ToList();

            var expectedResult = new List<Model>
            {
                new Model() { Id = 1, Name = "1", Active = true  },
                new Model() { Id = 2, Name = "2", Active = false  },
                new Model() { Id = 3, Name = "3", Active = true  },
            }
            .ToExpectedObject();

            expectedResult.ShouldEqual(result.Cast<Model>().ToList());
        }

        [Test]
        public void Can_Project_From_Object_To_Type_Using_Ctor()
        {
            var source = new[]
            {
                new { Prop1 = 1 , Prop2 = "1", Active = true  },
                new { Prop1 = 2 , Prop2 = "2", Active = false  },
                new { Prop1 = 3 , Prop2 = "3", Active = true  },
            };

            var sourceElement = source.First();

            var projection = new ProjectionBuilder(source.First().GetType(), typeof(Model))
                .Member(nameof(Model.Id), map => map
                    .UsingCtorParameter(nameof(Model.Id).ToLower())
                    .FromExpression(nameof(sourceElement.Prop1)))
                .CtorParameter(nameof(Model.Name).ToLower(), map => map
                    .FromExpression(nameof(sourceElement.Prop2)))
                .CtorParameter(nameof(Model.Active).ToLower(), map => map
                    .FromExpression(nameof(sourceElement.Active)))
                .Build();

            var result = projection
                .BuildQuery(source.AsQueryable())
                .ToList();

            var expectedResult = new List<Model>
            {
                new Model() { Id = 1, Name = "1", Active = true  },
                new Model() { Id = 2, Name = "2", Active = false  },
                new Model() { Id = 3, Name = "3", Active = true  },
            }
            .ToExpectedObject();

            expectedResult.ShouldEqual(result.Cast<Model>().ToList());
        }
    }
}
