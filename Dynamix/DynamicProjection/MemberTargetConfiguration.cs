﻿using System;

namespace Dynamix.DynamicProjection
{
    internal class MemberTargetConfiguration
    {
        public ProjectedMember ProjectedMember { get; set; }
        public string CtorParameterName { get; set; }
        public ProjectionTarget ProjectionTarget { get; set; } = ProjectionTarget.Member;
        public IProjectionSource Source { get; set; }
        public IProjectionSource SourceKey { get; set; }
        public ValueMap ValueMap { get; set; }
        public Type AsType { get; set; }
        public bool AsNullable { get; set; }

        public MemberTargetConfiguration(ProjectedMember projectedMember)
        {
            ProjectedMember = projectedMember;
        }
    }
}
