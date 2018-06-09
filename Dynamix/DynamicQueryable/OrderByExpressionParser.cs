using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    internal static class OrderByExpressionParser
    {
        internal static List<OrderItem> Parse(string orderByExpression)
        {
            var res = new List<OrderItem>();
            foreach (var item in orderByExpression.Split(','))
            {
                var o = item.Trim().Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                if (o.Length == 0 || o.Length > 2)
                    throw new ArgumentException("Invalid OrderBy Expression: [" + orderByExpression + "]");

                var order = false;

                if (o.Length == 2)
                {
                    var orderStr = o[1].Trim().ToLower();

                    if (orderStr == "desc")
                        order = true;
                    else if (orderStr != "asc")
                        throw new ArgumentException("Invalid OrderBy Expression");
                }

                res.Add(new OrderItem(o[0].Trim(), order));
            }
            return res;
        }
    }
}
