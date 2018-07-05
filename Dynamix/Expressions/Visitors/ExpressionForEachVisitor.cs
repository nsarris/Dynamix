using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions.Visitors
{

    public class ExpressionForEachVisitor : ExpressionVisitor
    {
        private readonly Func<Expression, Expression> visitor;

        public ExpressionForEachVisitor(Func<Expression, Expression> visitor)
        {
            this.visitor = visitor;
        }

        public ExpressionForEachVisitor(Action<Expression> visitor)
        {
            this.visitor = x => { visitor(x); return x; };
        }

        public override Expression Visit(Expression node)
        {
            if ( visitor != null)
                node = visitor(node);

            return base.Visit(node);
        }

        public static Expression Visit(Expression expression, Func<Expression, Expression> visitor)
        {
            return new ExpressionForEachVisitor(visitor).Visit(expression);
        }

        public static Expression<TDelegate> Visit<TDelegate>(Expression<TDelegate> expression, Func<Expression, Expression> visitor)
        {
            return (Expression<TDelegate>)new ExpressionForEachVisitor(visitor).Visit(expression);
        }

        public static Expression Visit(Expression expression, Action<Expression> visitor)
        {
            return new ExpressionForEachVisitor(visitor).Visit(expression);
        }

        public static Expression<TDelegate> Visit<TDelegate>(Expression<TDelegate> expression, Action<Expression> visitor)
        {
            return (Expression<TDelegate>)new ExpressionForEachVisitor(visitor).Visit(expression);
        }
    }


}
