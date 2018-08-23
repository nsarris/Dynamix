using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{
    internal class ExpressionEnumeration : EqualityExpressionVisitor, IEnumerable<Expression>
    {
        private readonly List<Expression> expressions = new List<Expression>();

        public ExpressionEnumeration(Expression expression)
        {
            Visit(expression);
        }

        protected override void Visit(Expression expression)
        {
            if (expression == null) return;

            expressions.Add(expression);
            base.Visit(expression);
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            return expressions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
