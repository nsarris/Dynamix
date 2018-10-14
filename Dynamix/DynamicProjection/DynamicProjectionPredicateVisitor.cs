using Dynamix.Expressions;
using Dynamix.Expressions.PredicateBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.DynamicProjection
{


    internal class DynamicProjectionPredicateVisitor : INodeVisitor<Expression, ExpressionNodeVisitorInput>
    {
        private readonly DynamicProjection dynamicProjection;

        public DynamicProjectionPredicateVisitor(DynamicProjection dynamicProjection)
        {
            this.dynamicProjection = dynamicProjection;
        }

        Expression INodeVisitor<Expression, ExpressionNodeVisitorInput>.VisitComplexNode(ComplexNode node, ExpressionNodeVisitorInput input)
        {
            Expression expression = null;
            foreach (var childNode in node.Nodes)
            {
                var childResult = childNode.Accept(this, input);

                expression =
                    expression == null
                        ? childResult
                        : childNode.LogicalOperator == LogicalOperator.And
                            ? ExpressionEx.AndAlso(expression, childResult)
                            : ExpressionEx.OrElse(expression, childResult);
            }

            return expression;
        }

        Expression INodeVisitor<Expression, ExpressionNodeVisitorInput>.VisitBinaryNode(BinaryNode node, ExpressionNodeVisitorInput input)
        {
            return VisitBinaryNode(node, input);
        }

        protected Expression VisitBinaryNode(BinaryNode node, ExpressionNodeVisitorInput input)
        {
            if (!input.Configurations.TryGetValue(node.Expression, out var configuration))
                configuration = input.DefaultConfiguration;

            return BuildUnaryNodeExpression(node, input.ItParameterExpression, configuration);
        }

        protected virtual Expression BuildUnaryNodeExpression(BinaryNode node, ParameterExpression itParameter, PredicateBuilderConfiguration nodeConfiguration)
        {
            if (dynamicProjection.CompiledConfiguration.CompiledMembers.TryGetValue(node.Expression, out var memberMap))
                return PredicateBuilder.GetPredicateExpression(memberMap.Source.SourceExpression, node.Operator, node.Value, nodeConfiguration);
            else
                throw new InvalidOperationException($"Expression '{node.Expression}' is not a projection member and cannot be mapped to a source expression");
        }

        public Expression Visit(NodeBase root, ExpressionNodeVisitorInput input)
        {
            return root.Accept(this, input);
        }

        public LambdaExpression VisitLambda(NodeBase root, PredicateBuilderConfiguration configuration = null, IDictionary<string, PredicateBuilderConfiguration> configurations = null)
        {
            var input = new ExpressionNodeVisitorInput(dynamicProjection.CompiledConfiguration.It, configuration, configurations);
            return Expression.Lambda(Visit(root, input), input.ItParameterExpression);
        }
    }


}
