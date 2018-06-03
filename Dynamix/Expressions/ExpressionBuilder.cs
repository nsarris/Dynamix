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
    public static class ExpressionBuilder
    {
        public static Expression MakeDeepMemberAccess(Expression expression, string memberPath, bool safe = false)
        {
            return MakeDeepMemberAccess(expression, memberPath.Split('.'), safe);
        }
        public static Expression MakeDeepMemberAccess(Expression expression, IEnumerable<string> members, bool safe = false)
        {
            foreach (var memberName in members)
            {
                if (safe)
                {
                    var member = (MemberInfo)expression.Type.GetMember(memberName).FirstOrDefault();
                    if (member == null)
                        member = (MemberInfo)expression.Type.GetMember(memberName, BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault();
                    if (member == null)
                        member = (MemberInfo)expression.Type.GetMember(memberName, BindingFlags.Static | BindingFlags.Public).FirstOrDefault();
                    if (member == null)
                        member = (MemberInfo)expression.Type.GetMember(memberName, BindingFlags.Static | BindingFlags.NonPublic).FirstOrDefault();
                    if (member == null)
                        throw new Exception("Member " + memberName + " in member path " + string.Join(".", members) + " cannot be found");
                    expression = Expression.MakeMemberAccess(expression, member);
                }
                else
                    expression = Expression.PropertyOrField(expression, memberName);
            }

            return expression;
        }
        public static Expression MakeDeepMemberAccess(Expression expression, IEnumerable<MemberInfo> members)
        {
            foreach (var member in members)
                expression = Expression.MakeMemberAccess(expression, member);

            return expression;
        }
        public static LambdaExpression GetPropertySelector(Type type, string propertyName, out Type resultType, bool safe = false)
        {
            if (!type.IsClass)
                throw new Exception("Property selector builder is only valid on objects");

            var parameter = Expression.Parameter(type);
            var accessor = MakeDeepMemberAccess(parameter, propertyName, safe);
            resultType = accessor.Type;
            return Expression.Lambda(accessor, parameter);
        }

        public static LambdaExpression GetPropertySelector(string typeName, string propertyName, bool safe = false)
        {
            Type type = Type.GetType(typeName);
            Type resultType;
            return GetPropertySelector(type, propertyName, out resultType, safe);
        }

        public static LambdaExpression GetPropertySelector(Type type, string propertyName, bool safe = false)
        {
            Type resultType;
            return GetPropertySelector(type, propertyName, out resultType, safe);
        }

        public static LambdaExpression GetPropertySelector<T>(string propertyName, out Type resultType, bool safe = false) //where T : class
        {
            return GetPropertySelector(typeof(T), propertyName, out resultType, safe);
        }

        public static LambdaExpression GetPropertySelector<T>(string propertyName, bool safe = false)
            where T : class
        {
            Type resultType;
            return GetPropertySelector<T>(propertyName, out resultType, safe);
        }

        public static Expression<Func<T, TProperty>> GetPropertySelector<T, TProperty>(string propertyName, bool safe = false)
            where T : class
        {
            var parameter = Expression.Parameter(typeof(T));
            var accessor = MakeDeepMemberAccess(parameter, propertyName, safe);
            return Expression.Lambda<Func<T, TProperty>>(accessor, parameter);
        }

        //public static LambdaExpression CreateMemberExpression(Type type, string propertyName, bool cast = false)
        //{
        //    var param = Expression.Parameter(type, "x");
        //    //var castedParam = Expression.Variable(type, "castedSource");

        //    Expression body;
        //    if (cast)
        //        body = Expression.Convert(param, type);
        //    else
        //        body = param;

        //    foreach (var member in propertyName.Split('.'))
        //    {
        //        body = Expression.PropertyOrField(body, member);
        //    }
        //    return Expression.Lambda(body, param);
        //}

        //public static Expression<Func<object, TMember>> CreateMemberExpression<TMember>(Type type, string propertyName)
        //{
        //    var param = Expression.Parameter(typeof(object), "x");
        //    //var castedParam = Expression.Variable(type, "castedSource");

        //    Expression body = Expression.Convert(param, type);
        //    foreach (var member in propertyName.Split('.'))
        //    {
        //        body = Expression.PropertyOrField(body, member);
        //    }
        //    return Expression.Lambda<Func<object, TMember>>(body, param);
        //}

        //public static Expression<Func<T, TMember>> CreateMemberExpression<T, TMember>(string propertyName)
        //{
        //    var param = Expression.Parameter(typeof(T), "x");

        //    Expression body = param;
        //    foreach (var member in propertyName.Split('.'))
        //    {
        //        body = Expression.PropertyOrField(body, member);
        //    }
        //    return Expression.Lambda<Func<T, TMember>>(body, param);
        //}

        public static Expression<Func<T, bool>> CreateMemberPredicate<T, TMember>(string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var param2 = Expression.Parameter(typeof(TMember), "y");

            Expression body = param;
            foreach (var member in propertyName.Split('.'))
            {
                body = Expression.PropertyOrField(body, member);
            }

            return Expression.Lambda<Func<T, bool>>(Expression.Equal(body, param2));
        }

        public static Expression<Func<object, TMember, bool>> CreateMemberPredicate<TMember>(Type type, string propertyName)
        {
            var param = Expression.Parameter(typeof(object), "x");
            var param2 = Expression.Parameter(typeof(TMember), "y");

            Expression body = Expression.Convert(param, type);
            foreach (var member in propertyName.Split('.'))
            {
                body = Expression.PropertyOrField(body, member);
            }

            return Expression.Lambda<Func<object, TMember, bool>>(Expression.Equal(body, param2), param, param2);
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(IEnumerable<Tuple<PropertyInfoEx,object>> values)
        {
            //var ids = GetKeyProperties(typeof(T)).Select(x => PropertyInfoEx.GetCached(x));

            var p = Expression.Parameter(typeof(T));
            BinaryExpression e = null;

            foreach (var v in values)
            {
                var eqEx = Expression.Equal(
                    Expression.Property(p, v.Item1.PropertyInfo),
                    Expression.Constant(v.Item2, v.Item1.PropertyInfo.PropertyType));

                if (e == null)
                    e = eqEx;
                else
                    e = Expression.AndAlso(e, eqEx);
            }

            return Expression.Lambda<Func<T, bool>>(e, p);
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(T obj, IEnumerable<PropertyInfoEx> props)
        {
            return GetPredicate(obj, props.Select(x => x.PropertyInfo));
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(IEnumerable<T> obj, IEnumerable<PropertyInfoEx> props)
        {
            return GetPredicate(obj, props.Select(x => x.PropertyInfo));
        }

        public static Expression<Func<T, bool>> GetPredicate<T>(T obj, IEnumerable<PropertyInfo> props)
        {
            //var ids = GetKeyProperties(typeof(T)).Select(x => PropertyInfoEx.GetCached(x));

            var p = Expression.Parameter(typeof(T));
            BinaryExpression e = null;

            foreach (var id in props.Select(x => PropertyInfoExCache.GetPropertyEx(x)))
            {
                var eqEx = Expression.Equal(
                    Expression.Property(p, id.PropertyInfo),
                    Expression.Constant(id.Get(obj), id.PropertyInfo.PropertyType));

                if (e == null)
                    e = eqEx;
                else
                    e = Expression.AndAlso(e, eqEx);
            }

            return Expression.Lambda<Func<T, bool>>(e, p);
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
                            new Type[] { },
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
                    if (itemexp == null)
                        e = itemexp;
                    else
                        e = Expression.OrElse(e, itemexp);
                }
            }

            return Expression.Lambda<Func<T, bool>>(e, p);
        }
    }
}
