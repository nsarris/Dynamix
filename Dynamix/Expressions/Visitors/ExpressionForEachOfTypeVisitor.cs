using System;
using System.Linq.Expressions;

namespace Dynamix.Expressions.Visitors
{
    public class ExpressionForEachOfTypeVisitor<TExpression> : ExpressionVisitor 
        where TExpression : Expression
    {
        private readonly Func<TExpression, Expression> visitor;
        
        public ExpressionForEachOfTypeVisitor(Func<TExpression, Expression> visitor)
        {
            this.visitor = visitor;
        }

        public ExpressionForEachOfTypeVisitor(Action<TExpression> visitor)
        {
            this.visitor = x => { visitor(x); return x; };
        }

        public override Expression Visit(Expression node)
        {
            if (visitor != null 
                && node is TExpression expression
                && visitor(expression) == null)
                    return node;
            
            return base.Visit(node);
        }

        public static Expression Visit(Expression expression, Func<TExpression, Expression> visitor)
        {
            return new ExpressionForEachOfTypeVisitor<TExpression>(visitor).Visit(expression);
        }

        public static Expression<TDelegate> Visit<TDelegate>(Expression<TDelegate> expression, Func<TExpression, Expression> visitor)
        {
            return (Expression<TDelegate>)new ExpressionForEachOfTypeVisitor<TExpression>(visitor).Visit(expression);
        }

        public static Expression Visit(Expression expression, Action<TExpression> visitor)
        {
            return new ExpressionForEachOfTypeVisitor<TExpression>(visitor).Visit(expression);
        }

        public static Expression<TDelegate> Visit<TDelegate>(Expression<TDelegate> expression, Action<TExpression> visitor)
        {
            return (Expression<TDelegate>)new ExpressionForEachOfTypeVisitor<TExpression>(visitor).Visit(expression);
        }
    }


}
