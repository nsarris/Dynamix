using System;

namespace Dynamix.DynamicProjection
{
    public class ProjectionBuilder<TSource> : ProjectionBuilderBase<ProjectionBuilder<TSource>, MemberTargetBuilder<TSource>, CtorParamTargetBuilder<TSource>>
    {
        public ProjectionBuilder(Type projectedType)
            : base(typeof(TSource), projectedType)
        {
        }
    }
}
