using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Dynamix.Expressions.PredicateBuilder
{
    public class BinaryNode : NodeBase
    {
        public BinaryNode(string expression, ExpressionOperator expressionOperator, object value, LogicalOperator logicalOperator)
            : base(logicalOperator)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentException(nameof(expression) + " cannot be null or empty", nameof(expression));

            this.Expression = expression;
            this.Operator = expressionOperator;
            this.Value = value;
        }
        public string Expression { get; }
        public ExpressionOperator Operator { get; }
        public override bool HasChildren { get { return false; } }
        public object Value { get; }

        public override string GetStringExpression()
        {
            return $"{Expression} {Operator} {Value}";
        }


        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitBinaryNode(this);
        }

        public override T Accept<T, TInput>(INodeVisitor<T, TInput> visitor, TInput input)
        {
            return visitor.VisitBinaryNode(this, input);
        }

        public override T Accept<T, TInput, TState>(INodeVisitor<T, TInput, TState> visitor, TInput input, TState state)
        {
            return visitor.VisitBinaryNode(this, input, state);
        }
    }
}