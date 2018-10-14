using Dynamix.DynamicLinq;
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
    public static class MemberExpressionBuilder
    {
        #region MemberAccess

        public static MemberExpression MakeDeepMemberAccess(Expression expression, string memberPath, bool safe = false)
        {
            return MakeDeepMemberAccess(expression, memberPath.Split('.'), safe);
        }
        public static MemberExpression MakeDeepMemberAccess(Expression expression, IEnumerable<string> members, bool safe = false)
        {
            return (MemberExpression)members.Aggregate(expression, (aggregate, next) => MakeMemberAccess(aggregate, next, safe));
        }

        public static MemberExpression MakeMemberAccess(Expression expression, string memberName, bool safe = false)
        {
            if (safe)
            {
                var member = expression.Type.GetMember(memberName).FirstOrDefault();
                if (member == null)
                    member = expression.Type.GetMember(memberName, BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault();
                if (member == null)
                    member = expression.Type.GetMember(memberName, BindingFlags.Static | BindingFlags.Public).FirstOrDefault();
                if (member == null)
                    member = expression.Type.GetMember(memberName, BindingFlags.Static | BindingFlags.NonPublic).FirstOrDefault();
                if (member == null)
                    throw new MemberAccessException($"Member {memberName} not found on type {expression.Type.FullName}");

                return Expression.MakeMemberAccess(expression, member);
            }
            else
                return Expression.PropertyOrField(expression, memberName);
        }

        #endregion

        #region Property Selector

        public static LambdaExpression GetPropertySelector(ParameterExpression instanceParameter, string propertyName, bool safe = false)
        {
            return Expression.Lambda(MakeMemberAccess(instanceParameter, propertyName, safe), instanceParameter);
        }

        public static Expression<Func<T, TProperty>> GetPropertySelector<T, TProperty>(ParameterExpression instanceParameter, string propertyName, bool safe = false)
        {
            return Expression.Lambda<Func<T, TProperty>>(MakeMemberAccess(instanceParameter, propertyName, safe), instanceParameter);
        }

        public static LambdaExpression GetPropertySelector(Type type, string propertyName, bool safe = false)
        {
            return GetPropertySelector(Expression.Parameter(type), propertyName, safe);
        }

        public static Expression<Func<T, TProperty>> GetPropertySelector<T, TProperty>(string propertyName, bool safe = false)
        {

            return GetPropertySelector<T, TProperty>(Expression.Parameter(typeof(T)), propertyName, safe);
        }

        public static LambdaExpression GetPropertySelector(string typeName, string propertyName, bool safe = false)
        {
            return GetPropertySelector(Type.GetType(typeName), propertyName, safe);
        }

        public static LambdaExpression GetPropertySelector<T>(string propertyName, bool safe = false)
        {
            return GetPropertySelector(typeof(T), propertyName, safe);
        }

        public static Expression GetPropertyExpression(Expression instanceExpression, string propertyName, bool safe = false)
        {
            return MakeMemberAccess(instanceExpression, propertyName, safe);
        }

        #endregion

        #region DeepPropertySelector

        public static LambdaExpression GetDeepPropertySelector(ParameterExpression instanceParameter, string propertyName, bool safe = false)
        {
            return Expression.Lambda(MakeDeepMemberAccess(instanceParameter, propertyName, safe), instanceParameter);
        }

        public static Expression<Func<T, TProperty>> GetDeepPropertySelector<T, TProperty>(ParameterExpression instanceParameter, string propertyName, bool safe = false)
        {
            return Expression.Lambda<Func<T, TProperty>>(MakeDeepMemberAccess(instanceParameter, propertyName, safe), instanceParameter);
        }

        public static LambdaExpression GetDeepPropertySelector(Type type, string propertyName, bool safe = false)
        {
            return GetDeepPropertySelector(Expression.Parameter(type), propertyName, safe);
        }

        public static Expression<Func<T, TProperty>> GetDeepPropertySelector<T, TProperty>(string propertyName, bool safe = false)
        {

            return GetDeepPropertySelector<T, TProperty>(Expression.Parameter(typeof(T)), propertyName, safe);
        }

        public static LambdaExpression GetDeepPropertySelector(string typeName, string propertyName, bool safe = false)
        {
            return GetDeepPropertySelector(Type.GetType(typeName), propertyName, safe);
        }

        public static LambdaExpression GetDeepPropertySelector<T>(string propertyName, bool safe = false)
        {
            return GetDeepPropertySelector(typeof(T), propertyName, safe);
        }

        public static Expression GetDeepPropertyExpression(Expression instanceExpression, string propertyName, bool safe = false)
        {
            return MakeDeepMemberAccess(instanceExpression, propertyName, safe);
        }

        #endregion

        #region DynamicLinq Expression Selector

        public static LambdaExpression GetExpressionSelector(ParameterExpression instanceParameter, string expression, bool safe = false)
        {
            var hasName = instanceParameter.Name != string.Empty;
            var itParameter = hasName ? Expression.Parameter(instanceParameter.Type) : instanceParameter;
            var l = DynamicExpressionParser.ParseLambda(new[] { itParameter }, null, expression);
            return hasName ? ExpressionParameterReplacer.Replace(l, itParameter, instanceParameter) : l;
        }

        public static Expression<Func<T, TProperty>> GetExpressionSelector<T, TProperty>(ParameterExpression instanceParameter, string expression, bool safe = false)
        {
            var l = DynamicExpressionParser.ParseLambda<T, TProperty>(expression);
            return ExpressionParameterReplacer.Replace(l, l.Parameters[0], instanceParameter);
        }

        public static LambdaExpression GetExpressionSelector(Type type, string expression, bool safe = false)
        {
            return GetExpressionSelector(Expression.Parameter(type), expression, safe);
        }

        public static Expression<Func<T, TProperty>> GetExpressionSelector<T, TProperty>(string expression, bool safe = false)
        {

            return GetExpressionSelector<T, TProperty>(Expression.Parameter(typeof(T)), expression, safe);
        }

        public static LambdaExpression GetExpressionSelector(string typeName, string expression, bool safe = false)
        {
            return GetExpressionSelector(Type.GetType(typeName), expression, safe);
        }

        public static LambdaExpression GetExpressionSelector<T>(string expression, bool safe = false)
        {
            return GetExpressionSelector(typeof(T), expression, safe);
        }

        public static Expression GetExpression(ParameterExpression instanceExpression, string expression, bool safe = false)
        {
            return DynamicExpressionParser.Parse(new[] { instanceExpression }, null, expression);
        }

        #endregion
    }
}
