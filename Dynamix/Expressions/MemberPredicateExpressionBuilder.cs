using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{
    public static class MemberPredicateExpressionBuilder
    {
        public static Expression<Func<T, TMember, bool>> CreateMemberPredicate<T, TMember>(string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var param2 = Expression.Parameter(typeof(TMember), "y");

            var body = MemberExpressionBuilder.GetDeepPropertyExpression(param, propertyName);

            return Expression.Lambda<Func<T, TMember, bool>>(Expression.Equal(body, param2), param, param2);
        }

        public static Expression<Func<object, TMember, bool>> CreateMemberPredicate<TMember>(Type type, string propertyName)
        {
            var param = Expression.Parameter(typeof(object), "x");
            var param2 = Expression.Parameter(typeof(TMember), "y");

            var body = MemberExpressionBuilder.GetDeepPropertyExpression(Expression.Convert(param, type), propertyName);

            return Expression.Lambda<Func<object, TMember, bool>>(Expression.Equal(body, param2), param, param2);
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(IEnumerable<(PropertyInfo PropertyInfo, object Value)> propertyValuePairs)
        {
            var p = Expression.Parameter(typeof(T));
            BinaryExpression e = null;

            foreach (var v in propertyValuePairs)
            {
                var eqEx = Expression.Equal(
                    Expression.Property(p, v.PropertyInfo),
                    Expression.Constant(v.Value, v.PropertyInfo.PropertyType));

                if (e == null)
                    e = eqEx;
                else
                    e = Expression.AndAlso(e, eqEx);
            }

            return Expression.Lambda<Func<T, bool>>(e, p);
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(IEnumerable<Tuple<PropertyInfo, object>> propertyValuePairs)
        {
            return GetPredicate<T>(propertyValuePairs.Select(x => (x.Item1, x.Item2)));
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(IEnumerable<(string PropertyName, object Value)> propertyValuePairs)
        {
            return GetPredicate<T>(propertyValuePairs.Select(x => (typeof(T).GetProperty(x.PropertyName), x.Value)));
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(T obj, IEnumerable<PropertyInfoEx> properties)
        {
            return GetPredicate<T>(properties.Select(x => (x.PropertyInfo, x.Get(obj))));
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(T obj, IEnumerable<PropertyInfo> properties)
        {
            return GetPredicate<T>(properties.Select(x => (x, x.GetPropertyEx().Get(obj))));
        }
        public static Expression<Func<T, bool>> GetPredicate<T>(IEnumerable<T> obj, IEnumerable<PropertyInfoEx> props)
        {
            return GetPredicate(obj, props.Select(x => x.PropertyInfo));
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(IEnumerable<T> obj, IEnumerable<PropertyInfo> props)
        {
            var p = Expression.Parameter(typeof(T));

            if (obj.Count() == 0)
                return Expression.Lambda<Func<T, bool>>(Expression.Constant(false), p);

            Expression e = null;

            var exprops = props.Select(x => PropertyInfoExCache.GetPropertyEx(x)).ToList();

            if (exprops.Count == 0)
                throw new ArgumentException("Properties cannot be empty");
            else if (exprops.Count == 1)
            {
                var prop = exprops.Single();
                if (obj.Count() == 1)
                {
                    e = Expression.Equal(
                        Expression.Property(p, prop.PropertyInfo),
                        Expression.Constant(prop.Get(obj.Single()), prop.PropertyInfo.PropertyType));
                }
                else
                {
                    var ltype = typeof(List<>).MakeGenericType(prop.PropertyInfo.PropertyType);
                    var data = obj.Select(x => prop.Get(x));
                    var cast = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(prop.PropertyInfo.PropertyType);
                    var tolist = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(prop.PropertyInfo.PropertyType);
                    var l1 = cast.Invoke(null, new object[] { data });
                    var l2 = tolist.Invoke(null, new object[] { l1 });

                    e = Expression.Call(Expression.Constant(l2, ltype),
                            "Contains",
                            Type.EmptyTypes,
                            Expression.Property(p, prop.PropertyInfo));
                }
            }
            else
            {
                foreach (var item in obj)
                {
                    Expression itemexp = null;
                    foreach (var prop in exprops)
                    {
                        var eqEx = Expression.Equal(
                           Expression.Property(p, prop.PropertyInfo),
                           Expression.Constant(prop.Get(item), prop.PropertyInfo.PropertyType));

                        if (itemexp == null)
                            itemexp = eqEx;
                        else
                            itemexp = Expression.AndAlso(itemexp, eqEx);
                    }
                    if (e == null)
                        e = itemexp;
                    else
                        e = Expression.OrElse(e, itemexp);
                }
            }

            return Expression.Lambda<Func<T, bool>>(e, p);
        }
    }
}
