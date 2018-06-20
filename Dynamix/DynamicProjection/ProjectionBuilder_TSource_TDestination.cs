using System;
using System.Linq.Expressions;

namespace Dynamix.DynamicProjection
{
    public class ProjectionBuilder<TSource, TDestination> : ProjectionBuilderBase<ProjectionBuilder<TSource, TDestination>, MemberTargetBuilder<TSource>, CtorParamTargetBuilder<TSource>>
    {
        public ProjectionBuilder()
            : base(typeof(TSource), typeof(TDestination))
        {

        }

        public ProjectionBuilder<TSource, TDestination> Member<TMember>(Expression<Func<TDestination, TMember>> memberLambdaExpression, Func<MemberTargetBuilder<TSource>, ConfiguredMemberTargetBuilder> map)
        {
            var mapTarget = map(GetMemberTargetBuilder(new MemberTargetConfiguration(new LambdaExpressionProjectedMember(memberLambdaExpression))));
            Configuration.Members.Add(mapTarget.Configuration);
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> Auto<TMember>(Expression<Func<TDestination, TMember>> memberLambdaExpression)
        {
            Configuration.Members.Add(new MemberTargetConfiguration(new LambdaExpressionProjectedMember(memberLambdaExpression)));
            return this;
        }
    }
}
