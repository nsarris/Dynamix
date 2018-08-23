namespace Dynamix.DynamicProjection
{
    public interface IMemberNameMatchStrategy
    {
        bool CtorParameterMatchesMember(string parameterName, string memberName);
        bool CtorParameterMatchesSourceMember(string parameterName, string memberName);
        bool MemberMatchesSourceMember(string memberName, string sourceMemberName);
        //string MemberNameToCtorParameter(string memberName);
        //string CtorParameterToMemberName(string parameterName);
    }
}
