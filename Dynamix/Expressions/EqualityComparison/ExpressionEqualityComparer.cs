using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        public static readonly ExpressionEqualityComparer Instance = new ExpressionEqualityComparer();

        public bool Equals(Expression x, Expression y)
        {
            return new ExpressionComparisonVisitor(x, y).AreEqual;
        }

        public int GetHashCode(Expression obj)
        {
            return new ExpressionHashCodeCalculationVisitor(obj).HashCode;
        }
    }
}
