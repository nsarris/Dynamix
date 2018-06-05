//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;

//namespace Dynamix
//{
//    public class DynamicOrderedEnumerable : DynamicEnumerable
//    {
//        internal DynamicOrderedEnumerable(IEnumerable enumerable)
//            : base(enumerable)
//        {
            
//        }
    
//        public DynamicOrderedEnumerable ThenBy(LambdaExpression ordering)
//        {
//            if (ordering == null) throw new ArgumentNullException("ordering");
//            if (Source is IOrderedQueryable)
//                return new DynamicOrderedEnumerable((IOrderedQueryable)Execute(Source, Functions.ThenBy, ordering));
//            else
//                return OrderBy(ordering);
//        }

//        public DynamicOrderedEnumerable ThenByDescending(LambdaExpression ordering)
//        {
//            if (ordering == null) throw new ArgumentNullException("ordering");
//            if (Source is IOrderedQueryable)
//                return new DynamicOrderedEnumerable((IOrderedQueryable)Execute(Source, Functions.ThenByDescending, ordering));
//            else
//                return OrderByDescending(ordering);
//        }

//        public DynamicOrderedEnumerable ThenBy(IEnumerable<OrderItem> orderItems)
//        {
//            if (orderItems == null || !orderItems.Any()) throw new ArgumentNullException("Order items cannot be null or empty");
//            DynamicOrderedEnumerable res = null;
//            foreach (var item in orderItems)
//            {
//                var l = CreateMemberLambda(Source.ElementType, item.PropertyName);
//                if (res != null)
//                    res = (item.IsDescending ? res.ThenByDescending(l) : res.ThenBy(l));
//                else
//                    res = (item.IsDescending ? OrderByDescending(l) : OrderBy(l));
//            }
//            return res;
//        }
//    }

//}
