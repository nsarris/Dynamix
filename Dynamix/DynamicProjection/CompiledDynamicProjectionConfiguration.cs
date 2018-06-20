using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.DynamicProjection
{
    internal class CompiledDynamicProjectionConfiguration
    {
        public Type SourceType { get; }
        public Type ProjectedType { get; }
        public ParameterExpression It { get; }
        public List<MemberTargetConfiguration> Members { get; set; } = new List<MemberTargetConfiguration>();
        public List<CtorParamTargetConfiguration> CtorParameters { get; set; } = new List<CtorParamTargetConfiguration>();
        public IMemberNameMatchStrategy MemberNameMatchStrategy { get; set; } = new DefaultMemberNameMatchStrategy();
        public ConstructorInfo Ctor { get; set; }

        public List<CompiledCtorParamTargetConfiguration> CompiledCtorParams { get; }
        public List<CompiledMemberTargetConfiguration> CompiledMembers { get; }
    
        public List<MemberInitAssignment> MemberInitAssignments { get; }
        public List<CtorParameterAssignment> CtorParameterAssignments { get; }

        public LambdaExpression DefaultSelector { get; }
        public CompiledDynamicProjectionConfiguration(
            DynamicProjectionConfiguration configuration, 
            List<CompiledCtorParamTargetConfiguration> compiledCtorParams,
            List<CompiledMemberTargetConfiguration> compiledMembers,
            List<MemberInitAssignment> memberInitAssignments,
            List<CtorParameterAssignment> ctorParameterAssignments,
            LambdaExpression defaultSelector
            )
        {
            SourceType = configuration.SourceType;
            ProjectedType = configuration.ProjectedType;
            It = configuration.It;
            Members = configuration.Members;
            CtorParameters = configuration.CtorParameters;
            MemberNameMatchStrategy = configuration.MemberNameMatchStrategy;
            Ctor = configuration.Ctor;
            CompiledCtorParams = compiledCtorParams;
            CompiledMembers = compiledMembers;
            MemberInitAssignments = memberInitAssignments;
            CtorParameterAssignments = ctorParameterAssignments;
            DefaultSelector = defaultSelector;
        }
    }
}
