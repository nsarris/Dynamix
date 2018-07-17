using Dynamix.Expressions;
using Dynamix.Expressions.PredicateBuilder;
using System;
using System.Linq.Expressions;

namespace Dynamix.DynamicProjection
{
    public class DynamicProjectionPredicate
    {
        public Expression SourcePredicate { get; }
        public Expression ProjectionPredicate { get; }

        public DynamicProjectionPredicate(Expression sourcePredicate, Expression projectionPredicate)
        {
            SourcePredicate = sourcePredicate;
            ProjectionPredicate = projectionPredicate;
        }
    }

    public class DynamicProjectionPredicateLambdas
    {
        public LambdaExpression SourcePredicate { get; }
        public LambdaExpression ProjectionPredicate { get; }

        public DynamicProjectionPredicateLambdas(LambdaExpression sourcePredicate, LambdaExpression projectionPredicate)
        {
            SourcePredicate = sourcePredicate;
            ProjectionPredicate = projectionPredicate;
        }
    }

    public class DynamicProjectionPredicateLambdas<TSource,TProjection>
    {
        public Expression<Func<TSource, bool>> SourcePredicate { get; }
        public Expression<Func<TProjection, bool>> ProjectionPredicate { get; }

        public DynamicProjectionPredicateLambdas(Expression<Func<TSource, bool>> sourcePredicate, Expression<Func<TProjection, bool>> projectionPredicate)
        {
            SourcePredicate = sourcePredicate;
            ProjectionPredicate = projectionPredicate;
        }
    }

    public class DynamicProjectionPredicateVisitor : INodeVisitor<DynamicProjectionPredicate, ExpressionNodeVisitorInput>
    {
        private readonly DynamicProjection dynamicProjection;

        public DynamicProjectionPredicateVisitor(DynamicProjection dynamicProjection)
        {
            this.dynamicProjection = dynamicProjection;
        }

        DynamicProjectionPredicate INodeVisitor<DynamicProjectionPredicate, ExpressionNodeVisitorInput>.VisitComplexNode(ComplexNode node, ExpressionNodeVisitorInput input)
        {
            Expression sourceExpression = null;
            Expression projectionExpression = null;

            foreach (var childNode in node.Nodes)
            {
                var childResult = childNode.Accept(this, input);

                sourceExpression =
                    sourceExpression == null
                        ? childResult.SourcePredicate
                        : childNode.LogicalOperator == LogicalOperator.And
                            ? ExpressionEx.AndAlso(sourceExpression, childResult.SourcePredicate)
                            : ExpressionEx.OrElse(sourceExpression, childResult.SourcePredicate);

                projectionExpression =
                    projectionExpression == null
                        ? childResult.ProjectionPredicate
                        : childNode.LogicalOperator == LogicalOperator.And
                            ? ExpressionEx.AndAlso(projectionExpression, childResult.ProjectionPredicate)
                            : ExpressionEx.OrElse(projectionExpression, childResult.ProjectionPredicate);
            }

            return new DynamicProjectionPredicate(sourceExpression, projectionExpression);
        }

        DynamicProjectionPredicate INodeVisitor<DynamicProjectionPredicate, ExpressionNodeVisitorInput>.VisitUnaryNode(BinaryNode node, ExpressionNodeVisitorInput input)
        {
            return VisitUnaryNode(node, input);
        }

        protected DynamicProjectionPredicate VisitUnaryNode(BinaryNode node, ExpressionNodeVisitorInput input)
        {
            if (!input.Configurations.TryGetValue(node.Expression, out var configuration))
                configuration = input.DefaultConfiguration;

            return BuildUnaryNodeExpression(node, input.ItParameterExpression, configuration);
        }

        protected virtual DynamicProjectionPredicate BuildUnaryNodeExpression(BinaryNode node, ParameterExpression itParameter, PredicateBuilderConfiguration nodeConfiguration)
        {
            if (dynamicProjection.CompiledConfiguration.CompiledMembers.TryGetValue(node.Expression, out var memberMap))
            {
                //Source it parameter
                return new DynamicProjectionPredicate(PredicateBuilder.GetPredicateExpression(itParameter, node.Expression, node.Operator, node.Value, nodeConfiguration), null);
            }
            else
                return new DynamicProjectionPredicate(null, PredicateBuilder.GetPredicateExpression(itParameter, node.Expression, node.Operator, node.Value, nodeConfiguration));
        }

        public DynamicProjectionPredicate Visit(NodeBase root, ExpressionNodeVisitorInput input)
        {
            return root.Accept(this, input);
        }

        public DynamicProjectionPredicateLambdas VisitLambda(NodeBase root, Type instanceType, PredicateBuilderConfiguration configuration = null)
        {
            var input = new ExpressionNodeVisitorInput(instanceType, configuration);
            var result = Visit(root, input);

            return new DynamicProjectionPredicateLambdas(
                Expression.Lambda(result.SourcePredicate, input.ItParameterExpression),
                Expression.Lambda(result.ProjectionPredicate, input.ItParameterExpression)
                );
                
        }

        public DynamicProjectionPredicateLambdas<TSource, TProjection> VisitLambda<TSource,TProjection>(NodeBase root, PredicateBuilderConfiguration configuration = null)
        {
            var input = new ExpressionNodeVisitorInput(typeof(TProjection), configuration);

            var result = Visit(root, input);

            return new DynamicProjectionPredicateLambdas<TSource, TProjection>(
                Expression.Lambda<Func<TSource, bool>>(result.SourcePredicate, input.ItParameterExpression),
                Expression.Lambda<Func<TProjection, bool>>(result.ProjectionPredicate, input.ItParameterExpression)
                );
        }
    }
}
