using Dynamix.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.PredicateBuilder
{

    public class ExpressionNodeVisitor : INodeVisitor<Expression, ExpressionNodeVisitorInput>
    {
        Expression INodeVisitor<Expression, ExpressionNodeVisitorInput>.VisitComplexNode(ComplexNode complexNode, ExpressionNodeVisitorInput input)
        {
            Expression expression = null;

            foreach (var node in complexNode.Nodes)
            {
                expression =
                    expression == null 
                        ? node.Accept(this, input) 
                        : node.LogicalOperator == LogicalOperator.And 
                            ? ExpressionEx.AndAlso(expression, node.Accept(this, input))
                            : ExpressionEx.OrElse(expression, node.Accept(this, input));
            }

            return expression;
        }

        Expression INodeVisitor<Expression, ExpressionNodeVisitorInput>.VisitUnaryNode(UnaryNode node, ExpressionNodeVisitorInput input)
        {
            return VisitUnaryNode(node, input);
        }

        protected virtual Expression VisitUnaryNode(UnaryNode node, ExpressionNodeVisitorInput input)
        {
            return PredicateBuilder.GetPredicateExpression(input.ItParameterExpression, node.Expression, node.Operator, node.Value, input.Configuration);
        }

        public Expression Visit(NodeBase root, ExpressionNodeVisitorInput input)
        {
            return root.Accept(this, input);
        }

        public LambdaExpression VisitLambda(NodeBase root, Type instanceType, PredicateBuilderConfiguration configuration = null)
        {
            var instanceParameter = Expression.Parameter(instanceType, "x");
            return Expression.Lambda(Visit(root, new ExpressionNodeVisitorInput(instanceParameter, configuration)), instanceParameter);
        }

        public Expression<Func<T,bool>> VisitLambda<T>(NodeBase root, PredicateBuilderConfiguration configuration = null)
        {
            var instanceParameter = Expression.Parameter(typeof(T), "x");
            return Expression.Lambda<Func<T,bool>>(Visit(root, new ExpressionNodeVisitorInput(instanceParameter, configuration)), instanceParameter);
        }
    }
}
