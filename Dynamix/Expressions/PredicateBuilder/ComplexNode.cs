using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Dynamix.Expressions.PredicateBuilder
{
    public class ComplexNode : NodeBase
    {
        public ComplexNode(LogicalOperator logicalOperator)
            :base(logicalOperator)
        {

        }
        public List<NodeBase> Nodes { get; } = new List<NodeBase>();
        public override bool HasChildren { get { return Nodes.Any(); } }

        public override string GetStringExpression()
        {
            return string.Join(" ",
                Nodes.Select(node => 
                $"({node} {node.LogicalOperator})"
                ));
        }

        
        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitComplexNode(this);
        }

        public override T Accept<T, TInput>(INodeVisitor<T, TInput> visitor, TInput input)
        {
            return visitor.VisitComplexNode(this, input);
        }

        public override T Accept<T, TInput, TState>(INodeVisitor<T, TInput, TState> visitor, TInput input, TState state)
        {
            return visitor.VisitComplexNode(this, input, state);
        }
    }
}