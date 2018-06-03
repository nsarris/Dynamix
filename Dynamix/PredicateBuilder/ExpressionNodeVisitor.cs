using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.PredicateBuilder
{
    public class ExpressionNodeVisitor : INodeVisitor<Expression, ParameterExpression>
    {
        Expression INodeVisitor<Expression, ParameterExpression>.VisitComplexNode(ComplexNode complexNode, ParameterExpression input)
        {
            Expression expression = null;

            foreach (var node in complexNode.Nodes)
            {
                expression =
                    expression == null 
                        ? node.Accept(this, input) 
                        : node.LogicalOperator == LogicalOperator.And 
                            ? Expression.AndAlso(expression, node.Accept(this, input))
                            : Expression.OrElse(expression, node.Accept(this, input));
            }

            return expression;
        }

        Expression INodeVisitor<Expression, ParameterExpression>.VisitUnaryNode(UnaryNode node, ParameterExpression input)
        {
            return VisitUnaryNode(node, input);
        }

        protected virtual Expression VisitUnaryNode(UnaryNode node, ParameterExpression input)
        {
            return PredicateBuilder.GetPredicateExpression(input, node.Expression, node.Operator, node.Value);
        }

        public Expression Visit(NodeBase root, ParameterExpression instanceParameter)
        {
            return root.Accept(this, instanceParameter);
        }

        public LambdaExpression VisitLambda(NodeBase root, Type instanceType)
        {
            var instanceParameter = Expression.Parameter(instanceType, "x");
            return Expression.Lambda(Visit(root, instanceParameter), instanceParameter);
        }

        public Expression<Func<T,bool>> VisitLambda<T>(NodeBase root)
        {
            var instanceParameter = Expression.Parameter(typeof(T), "x");
            return Expression.Lambda<Func<T,bool>>(Visit(root, instanceParameter), instanceParameter);
        }
    }
}
