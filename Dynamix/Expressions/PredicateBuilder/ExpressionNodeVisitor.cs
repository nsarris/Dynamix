﻿using Dynamix.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions.PredicateBuilder
{

    public class ExpressionNodeVisitor : INodeVisitor<Expression, ExpressionNodeVisitorInput>
    {
        Expression INodeVisitor<Expression, ExpressionNodeVisitorInput>.VisitComplexNode(ComplexNode node, ExpressionNodeVisitorInput input)
        {
            Expression expression = null;

            foreach (var childNode in node.Nodes)
            {
                expression =
                    expression == null 
                        ? childNode.Accept(this, input) 
                        : childNode.LogicalOperator == LogicalOperator.And 
                            ? ExpressionEx.AndAlso(expression, childNode.Accept(this, input))
                            : ExpressionEx.OrElse(expression, childNode.Accept(this, input));
            }

            return expression;
        }

        Expression INodeVisitor<Expression, ExpressionNodeVisitorInput>.VisitUnaryNode(UnaryNode node, ExpressionNodeVisitorInput input)
        {
            return VisitUnaryNode(node, input);
        }

        protected virtual Expression VisitUnaryNode(UnaryNode node, ExpressionNodeVisitorInput input)
        {
            if (!input.Configurations.TryGetValue(node.Expression, out var configuration))
                configuration = input.DefaultConfiguration;

            return PredicateBuilder.GetPredicateExpression(input.ItParameterExpression, node.Expression, node.Operator, node.Value, configuration);
        }

        public Expression Visit(NodeBase root, ExpressionNodeVisitorInput input)
        {
            return root.Accept(this, input);
        }

        public LambdaExpression VisitLambda(NodeBase root, Type instanceType, PredicateBuilderConfiguration configuration = null)
        {
            var input = new ExpressionNodeVisitorInput(instanceType, configuration);
            return Expression.Lambda(Visit(root, input), input.ItParameterExpression);
        }

        public Expression<Func<T,bool>> VisitLambda<T>(NodeBase root, PredicateBuilderConfiguration configuration = null)
        {
            var input = new ExpressionNodeVisitorInput(typeof(T), configuration);
            return Expression.Lambda<Func<T,bool>>(Visit(root, input), input.ItParameterExpression);
        }
    }
}
