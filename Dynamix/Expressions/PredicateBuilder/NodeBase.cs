using Dynamix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions.PredicateBuilder
{
    public abstract class NodeBase
    {
        protected NodeBase(LogicalOperator logicalOperator)
        {
            LogicalOperator = logicalOperator;
        }
        public LogicalOperator LogicalOperator { get; set; }
        public abstract bool HasChildren { get; }
        public abstract string GetStringExpression();
        public override string ToString() => GetStringExpression();

        public abstract T Accept<T>(INodeVisitor<T> visitor);
        public abstract T Accept<T, TInput>(INodeVisitor<T, TInput> visitor, TInput input);
        public abstract T Accept<T, TInput, TState>(INodeVisitor<T, TInput, TState> visitor, TInput input, TState state);

        public LambdaExpression GetLambdaExpression(Type type, PredicateBuilderConfiguration configuration = null)
        {
            return new ExpressionNodeVisitor().VisitLambda(this, type, configuration);
        }

        public Expression<Func<T, bool>> GetLambdaExpression<T>(PredicateBuilderConfiguration configuration = null)
        {
            return new ExpressionNodeVisitor().VisitLambda<T>(this, configuration);
        }
    }
}
