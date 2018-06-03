using System;
using System.Linq.Expressions;

namespace Dynamix.Expressions
{
    public class ExpressionForEachOfTypeVisitor<TExpression> : ExpressionVisitor 
        where TExpression : Expression
    {
        private readonly Func<TExpression, Expression> _visitor;

        public ExpressionForEachOfTypeVisitor(Func<TExpression, Expression> visitor)
        {
            _visitor = visitor;
        }

        public override Expression Visit(Expression expression)
        {
            if (expression is TExpression && _visitor != null)
            {
                var e = _visitor(expression as TExpression);
                if (e == null)
                    return expression;
            }

            return base.Visit(expression);
        }

        public static Expression Visit(Expression expression, Func<TExpression, Expression> visitor)
        {
            return new ExpressionForEachOfTypeVisitor<TExpression>(visitor).Visit(expression);
        }

        public static Expression<TDelegate> Visit<TDelegate>(Expression<TDelegate> expression, Func<TExpression, Expression> visitor)
        {
            return (Expression<TDelegate>)new ExpressionForEachOfTypeVisitor<TExpression>(visitor).Visit(expression);
        }
    }


}
