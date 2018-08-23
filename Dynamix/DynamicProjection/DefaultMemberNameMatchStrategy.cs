namespace Dynamix.DynamicProjection
{
    public class DefaultMemberNameMatchStrategy : IMemberNameMatchStrategy
    {
        public bool CtorParameterMatchesMember(string parameterName, string memberName)
        {
            return memberName?.ToCamelCase() == parameterName;
        }

        public bool CtorParameterMatchesSourceMember(string parameterName, string memberName)
        {
            return memberName?.ToCamelCase() == parameterName;
        }

        public bool MemberMatchesSourceMember(string memberName, string sourceMemberName)
        {
            return memberName == sourceMemberName;
        }
    }
}
