using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix.DynamicProjection
{
    internal abstract class ProjectedMember
    {
        public abstract MemberInfo GetMember(Type projectedType);
        public abstract string GetName();
        public abstract Type GetMemberType();
    }

    internal class StringProjectedMember : ProjectedMember
    {
        public string MemberName { get; }
        public StringProjectedMember(string memberName)
        {
            MemberName = memberName;
        }

        public override string GetName()
        {
            return MemberName;
        }

        public override Type GetMemberType()
        {
            return null;
        }

        public override MemberInfo GetMember(Type projectedType)
        {
            var memberInfo = projectedType.GetMember(MemberName).First();
            if (memberInfo.MemberType != MemberTypes.Property && memberInfo.MemberType != MemberTypes.Field)
                throw new InvalidOperationException("The expession is not a field or property expression");
            return memberInfo;
        }
    }

    internal class LambdaExpressionProjectedMember : ProjectedMember
    {
        private readonly MemberInfo memberInfo;
        private readonly Type memberType;
        public LambdaExpression PropertyExpression { get; }

        public LambdaExpressionProjectedMember(LambdaExpression propertyExpression)
        {
            PropertyExpression = propertyExpression;
            memberInfo = GetMemberInfo();
            memberType = memberInfo.MemberType == MemberTypes.Field ? ((FieldInfo)memberInfo).FieldType : ((PropertyInfo)memberInfo).PropertyType;
        }

        public override string GetName()
        {
            return memberInfo.Name;
        }

        public override Type GetMemberType()
        {
            return memberType;
        }

        public override MemberInfo GetMember(Type projectedType)
        {
            return GetMemberInfo(projectedType);
        }

        private MemberInfo GetMemberInfo(Type projectedType = null)
        {
            if (PropertyExpression.Body is MemberExpression memberExpression
                && (projectedType == null || memberExpression.Expression.Type == projectedType)
                && (memberExpression.Member.MemberType == MemberTypes.Field || memberExpression.Member.MemberType == MemberTypes.Property))
                return memberExpression.Member;
            else
                throw new InvalidOperationException("The expession is not a valid member expression");
        }
    }
}
