using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix.Reflection
{

    public static class ReflectionHelper
    {
        //public static PropertyInfo GetProperty(Type t, string PropertyName)
        //{
        //    //var properties = PropertyName.Split('.').ToList();

        //    if (t.GetProperties().Count(p => p.Name == PropertyName.Split('.')[0]) == 0)
        //        throw new ArgumentNullException(string.Format("Property {0}, does not exist in object {1}", PropertyName, t.ToString()));
        //    if (PropertyName.Split('.').Length == 1)
        //        return t.GetProperty(PropertyName);
        //    else
        //        return GetProperty(t.GetProperty(PropertyName.Split('.')[0]).PropertyType, PropertyName.Split('.')[1]);
        //}

        public static PropertyInfo GetProperty(LambdaExpression expression)
        {
            var member = expression.Body as MemberExpression;

            if (member == null && expression.Body is UnaryExpression)
                member = ((UnaryExpression)expression.Body).Operand as MemberExpression;

            if (member != null)
                return member.Member as PropertyInfo;

            throw new ArgumentException("Expression is not a member accessor", "expression");
        }

        public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> expression)
        {
            return GetProperty<T, object>(expression);
        }

        public static PropertyInfo GetProperty<T, TProp>(Expression<Func<T, TProp>> expression)
        {
            return GetProperty((LambdaExpression)expression);
        }

        public static string GetMemberName<T>(Expression<Func<T, object>> expression)
        {
            var member = expression.Body as MemberExpression;

            if (member == null && expression.Body is UnaryExpression)
                member = ((UnaryExpression)expression.Body).Operand as MemberExpression;

            if (member != null)
                return member.Member.Name;

            throw new ArgumentException("Expression is not a member accessor", "expression");
        }

        public static string GetMemberName<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            var member = expression.Body as MemberExpression;

            if (member == null && expression.Body is UnaryExpression)
                member = ((UnaryExpression)expression.Body).Operand as MemberExpression;

            if (member != null)
                return member.Member.Name;

            throw new ArgumentException("Expression is not a member accessor", "expression");
        }

        public static string GetMemberName<TMember>(Expression<Func<TMember>> expression)
        {
            var member = expression.Body as MemberExpression;

            if (member == null && expression.Body is UnaryExpression)
                member = ((UnaryExpression)expression.Body).Operand as MemberExpression;

            if (member != null)
                return member.Member.Name;

            throw new ArgumentException("Expression is not a member accessor", "expression");
        }

        public static string GetMemberName(Expression<Func<object>> expression)
        {
            var member = expression.Body as MemberExpression;

            if (member == null && expression.Body is UnaryExpression)
                member = ((UnaryExpression)expression.Body).Operand as MemberExpression;

            if (member != null)
                return member.Member.Name;

            throw new ArgumentException("Expression is not a member accessor", "expression");
        }

        public static void SetValue<T, TMember>(T obj, Expression<Func<T, TMember>> property, TMember value)
        {
            var w = new PropertyInfoExWrapper<T>(obj);
            w.Set(GetMemberName(property), value);
        }

        public static TMember GetValue<T, TMember>(T obj, Expression<Func<T, TMember>> property)
        {
            var w = new PropertyInfoExWrapper<T>(obj);
            return w.Get<TMember>(GetMemberName(property));
        }

    }

    public class ReflectionHelper<T>
    {
        public PropertyInfo GetProperty(Expression<Func<T, object>> expression)
        {
            return ReflectionHelper.GetProperty<T, object>(expression);
        }

        public PropertyInfo GetProperty<TProp>(Expression<Func<T, TProp>> expression)
        {
            return ReflectionHelper.GetProperty((LambdaExpression)expression);
        }

        public string GetMemberName(Expression<Func<T, object>> expression)
        {
            return ReflectionHelper.GetMemberName(expression);
        }

        public string GetMemberName<TMember>(Expression<Func<T, TMember>> expression)
        {
            return ReflectionHelper.GetMemberName(expression);
        }

        public void SetValue<TMember>(T obj, TMember value, Expression<Func<T, TMember>> property)
        {
            ReflectionHelper.SetValue(obj, property, value);
        }

        public TMember GetValue<TMember>(T obj, Expression<Func<T, TMember>> property)
        {
            return ReflectionHelper.GetValue(obj, property);
        }
    }
}
