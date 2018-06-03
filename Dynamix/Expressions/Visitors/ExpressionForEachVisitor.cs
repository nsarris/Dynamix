using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{

    public class ExpressionForEachVisitor : ExpressionVisitor
    {
        private readonly Func<Expression, Expression> _visitor;

        public ExpressionForEachVisitor(Func<Expression, Expression> visitor)
        {
            _visitor = visitor;
        }

        public override Expression Visit(Expression expression)
        {
            if ( _visitor != null)
                expression = _visitor(expression);

            return base.Visit(expression);
        }

        public static Expression Visit(Expression expression, Func<Expression, Expression> visitor)
        {
            return new ExpressionForEachVisitor(visitor).Visit(expression);
        }

        public static Expression<TDelegate> Visit<TDelegate>(Expression<TDelegate> expression, Func<Expression, Expression> visitor)
        {
            return (Expression<TDelegate>)new ExpressionForEachVisitor(visitor).Visit(expression);
        }
    }


}
