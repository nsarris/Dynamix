using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix
{
    public enum ParseExpressionAs
    {
        Property,
        NestedProperty,
        DynamicLinqExpression
    }
    public class OrderItem
    {
        public string Expression { get; }
        public bool IsDescending { get; }
        public ParseExpressionAs ParseExpressionAs { get; }

        public OrderItem(string expression, bool isDescending, ParseExpressionAs parseExpressionAs = ParseExpressionAs.NestedProperty)
        {
            Expression = expression;
            IsDescending = isDescending;
            ParseExpressionAs = parseExpressionAs;
        }
    }

}
