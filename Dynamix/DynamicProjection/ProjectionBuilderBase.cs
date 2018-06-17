using Dynamix.Expressions;
using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix.DynamicProjection
{
    public enum ProjectionTarget
    {
        Member,
        CtorParameter
    }

    public abstract class ProjectionBuilderBase<T, TMemberTargetBuilder, TCtorParamTargetBuilder>
        where T : ProjectionBuilderBase<T, TMemberTargetBuilder, TCtorParamTargetBuilder>
    {
        readonly static ConstructorInfoEx memberTargetBuilderCtor = typeof(TMemberTargetBuilder).GetConstructorEx(new Type[] { typeof(MemberTargetConfiguration) }, BindingFlagsEx.All);
        readonly static ConstructorInfoEx ctorParamTargetBuilderCtor = typeof(TCtorParamTargetBuilder).GetConstructorEx(new Type[] { typeof(CtorParamTargetConfiguration) }, BindingFlagsEx.All);

        public Type SourceType { get; }
        public Type ProjectedType { get; }

        public ProjectionTarget DefaultMemberTarget { get; protected set; }
        public IMemberNameMatchStrategy MemberNameMatchStrategy { get; internal set; } = new DefaultMemberNameMatchStrategy();

        internal readonly List<MemberTargetConfiguration> members = new List<MemberTargetConfiguration>();
        internal readonly List<CtorParamTargetConfiguration> ctorParameters = new List<CtorParamTargetConfiguration>();
        
        protected ProjectionBuilderBase(Type sourceType, Type projectedType)
        {
            SourceType = sourceType;
            ProjectedType = projectedType;
        }

        public DynamicProjection Build(Type sourceType, Type projectedType)
        {
            return new DynamicProjection(SourceType, ProjectedType, MemberNameMatchStrategy, members, ctorParameters);
        }


        public T WithDefaultMemberTarget(ProjectionTarget defaultMemberTarget)
        {
            DefaultMemberTarget = defaultMemberTarget;
            return (T)this;
        }

        public T WithDefaultMemberTargetComparer(IMemberNameMatchStrategy memberNameMatchStrategy)
        {
            MemberNameMatchStrategy = memberNameMatchStrategy;
            return (T)this;
        }

        public T Member(string member, Func<TMemberTargetBuilder, ConfiguredMemberTargetBuilder> map)
        {
            var mapTarget = map(GetMemberTargetBuilder(new MemberTargetConfiguration(new StringProjectedMember(member))));
            members.Add(mapTarget.Configuration);
            return (T)this;
        }

        public T CtorParameter(string parameterName, Func<TCtorParamTargetBuilder, ConfiguredCtorParamTargetBuilder> map)
        {
            ctorParameters.Add(map(GetCtorTargetBuilder(new CtorParamTargetConfiguration(parameterName))).Configuration);
            return (T)this;
        }

        public T Auto(string member)
        {
            members.Add(new MemberTargetConfiguration(new StringProjectedMember(member))
            {
                ProjectionTarget = DefaultMemberTarget
            });
            return (T)this;
        }

        internal TMemberTargetBuilder GetMemberTargetBuilder(MemberTargetConfiguration configuration) 
            => (TMemberTargetBuilder)memberTargetBuilderCtor.Invoke(configuration);
        internal TCtorParamTargetBuilder GetCtorTargetBuilder(CtorParamTargetConfiguration configuration) 
            => (TCtorParamTargetBuilder)ctorParamTargetBuilderCtor.Invoke(configuration);
    }
}
