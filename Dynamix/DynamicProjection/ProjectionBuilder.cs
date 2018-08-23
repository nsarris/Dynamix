using System;

namespace Dynamix.DynamicProjection
{
    public class ProjectionBuilder : ProjectionBuilderBase<ProjectionBuilder, MemberTargetBuilder, CtorParamTargetBuilder>
    {
        public ProjectionBuilder(Type sourceType, Type projectedType)
            : base(sourceType, projectedType)
        {

        }
    }
}
