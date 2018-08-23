using Dynamix.Expressions.PredicateBuilder;
using System.Collections.Generic;

namespace Dynamix.DynamicProjection
{
    internal class DynamicProjectionPredicateMemberVisitor : INodeVisitor<List<string>, object>
    {
        public DynamicProjectionPredicateMemberVisitor()
        {
        }

        public List<string> VisitComplexNode(ComplexNode node, object input)
        {
            var result = new List<string>();
            foreach (var childNode in node.Nodes)
            {
                childNode.Accept(this, input).ForEach(x => { if (!result.Contains(x)) result.Add(x); });
            }
            return result;
        }

        public List<string> VisitBinaryNode(BinaryNode node, object input)
        {
            return new List<string>() { node.Expression };
        }

        public List<string> Visit(NodeBase root)
        {
            return root.Accept(this, null);
        }
    }

    
}
