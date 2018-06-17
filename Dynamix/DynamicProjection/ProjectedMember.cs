using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix.DynamicProjection
{
    internal abstract class ProjectedMember
    {
        public abstract MemberInfo GetMember(Type sourceType);
    }

    internal class StringProjectedMember : ProjectedMember
    {
        public string MemberName { get; }
        public StringProjectedMember(string memberName)
        {
            MemberName = memberName;
        }
        public override MemberInfo GetMember(Type sourceType)
        {
            var memberInfo = sourceType.GetMember(MemberName).First();
            if (memberInfo.MemberType != MemberTypes.Property && memberInfo.MemberType != MemberTypes.Field)
                throw new InvalidOperationException("The expession is not a field or property expression");
            return memberInfo;
        }
    }

    internal class LambdaExpressionProjectedMember : ProjectedMember
    {
        public LambdaExpression PropertyExpression { get; }
        public LambdaExpressionProjectedMember(LambdaExpression propertyExpression)
        {
            PropertyExpression = propertyExpression;
        }
        public override MemberInfo GetMember(Type sourceType)
        {
            if (PropertyExpression.Body is MemberExpression memberExpression
                && memberExpression.Expression.Type == sourceType)
                return memberExpression.Member;
            else
                throw new InvalidOperationException("The expession is not a valid member expression");
        }
    }
}
