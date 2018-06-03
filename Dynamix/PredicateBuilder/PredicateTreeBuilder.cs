using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.PredicateBuilder
{
    public interface IPredicateTreeBuilderStart
    {
        IPredicateTreeBuilderNext Has(string expression, ExpressionOperator expressionOperator, object value);
        IPredicateTreeBuilderNext Has(Func<IPredicateTreeBuilderStart, IPredicateTreeBuilderNext> nestedPredicate);
    }

    public interface IPredicateTreeBuilderNext
    {
        ComplexNode RootNode { get; }
        IPredicateTreeBuilderNext And(string expression, ExpressionOperator expressionOperator, object value);
        IPredicateTreeBuilderNext Or(string expression, ExpressionOperator expressionOperator, object value);
        IPredicateTreeBuilderNext And(Func<IPredicateTreeBuilderStart, IPredicateTreeBuilderNext> nestedPredicate);
        IPredicateTreeBuilderNext Or(Func<IPredicateTreeBuilderStart, IPredicateTreeBuilderNext> nestedPredicate);
    }

    public class PredicateTreeBuilder : IPredicateTreeBuilderStart, IPredicateTreeBuilderNext
    {
        readonly Type instanceType;
        public ComplexNode RootNode { get; } = new ComplexNode(LogicalOperator.And);

        public PredicateTreeBuilder(Type instanceType)
        {
            this.instanceType = instanceType;
        }

        public IPredicateTreeBuilderNext Has(string expression, ExpressionOperator expressionOperator, object value)
        {
            return ((IPredicateTreeBuilderNext)this).And(expression, expressionOperator, value);
        }

        public IPredicateTreeBuilderNext Has(Func<IPredicateTreeBuilderStart, IPredicateTreeBuilderNext> nestedPredicate)
        {
            return ((IPredicateTreeBuilderNext)this).And(nestedPredicate);
        }

        IPredicateTreeBuilderNext IPredicateTreeBuilderNext.And(string expression, ExpressionOperator expressionOperator, object value)
        {
            RootNode.Nodes.Add(new UnaryNode(expression, expressionOperator, value, LogicalOperator.And));
            
            return this;
        }

        IPredicateTreeBuilderNext IPredicateTreeBuilderNext.Or(string expression, ExpressionOperator expressionOperator, object value)
        {
            RootNode.Nodes.Add(new UnaryNode(expression, expressionOperator, value, LogicalOperator.Or));

            return this;
        }

        IPredicateTreeBuilderNext IPredicateTreeBuilderNext.And(Func<IPredicateTreeBuilderStart, IPredicateTreeBuilderNext> nestedPredicate)
        {
            var innerBuilder = new PredicateTreeBuilder(instanceType);
            nestedPredicate(innerBuilder);
            var innerNode = new ComplexNode(LogicalOperator.And);
            innerNode.Nodes.Add(innerBuilder.RootNode);
            
            RootNode.Nodes.Add(innerNode);

            return this;
        }

        IPredicateTreeBuilderNext IPredicateTreeBuilderNext.Or(Func<IPredicateTreeBuilderStart, IPredicateTreeBuilderNext> nestedPredicate)
        {
            var innerBuilder = new PredicateTreeBuilder(instanceType);
            nestedPredicate(innerBuilder);
            var innerNode = new ComplexNode(LogicalOperator.Or);
            innerNode.Nodes.Add(innerBuilder.RootNode);

            RootNode.Nodes.Add(innerNode);

            return this;
        }
    }
}
