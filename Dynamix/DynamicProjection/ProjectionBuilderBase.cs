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

        internal DynamicProjectionConfiguration Configuration { get; }

        //public Type SourceType { get; }
        //public Type ProjectedType { get; }

        internal ProjectionTarget DefaultMemberTarget { get; set; }
        //public IMemberNameMatchStrategy MemberNameMatchStrategy { get; internal set; } = new DefaultMemberNameMatchStrategy();

        //internal readonly List<MemberTargetConfiguration> members = new List<MemberTargetConfiguration>();
        //internal readonly List<CtorParamTargetConfiguration> ctorParameters = new List<CtorParamTargetConfiguration>();
        
        protected ProjectionBuilderBase(Type sourceType, Type projectedType)
        {
            Configuration = new DynamicProjectionConfiguration(sourceType, projectedType);
            //SourceType = sourceType;
            //ProjectedType = projectedType;
        }

        public DynamicProjection Build(Type sourceType, Type projectedType)
        {
            return new DynamicProjection(Configuration.Clone(sourceType, projectedType, false));
        }

        public DynamicProjection Build(Type sourceType)
        {
            return new DynamicProjection(Configuration.Clone(sourceType, null, false));
        }

        public DynamicProjection Build()
        {
            return new DynamicProjection(Configuration.Clone(null, null, false));
        }

        public DynamicProjection BuildWithDynamicType()
        {
            return new DynamicProjection(Configuration.Clone(null, null, true));
        }


        public T WithDefaultMemberTarget(ProjectionTarget defaultMemberTarget)
        {
            DefaultMemberTarget = defaultMemberTarget;
            return (T)this;
        }

        public T WithDefaultMemberTargetComparer(IMemberNameMatchStrategy memberNameMatchStrategy)
        {
            Configuration.MemberNameMatchStrategy = memberNameMatchStrategy;
            return (T)this;
        }

        public T Member(string member, Func<TMemberTargetBuilder, ConfiguredMemberTargetBuilder> map)
        {
            var mapTarget = map(GetMemberTargetBuilder(new MemberTargetConfiguration(new StringProjectedMember(member))));
            Configuration.Members.Add(mapTarget.Configuration);
            return (T)this;
        }

        public T CtorParameter(string parameterName, Func<TCtorParamTargetBuilder, ConfiguredCtorParamTargetBuilder> map)
        {
            Configuration.CtorParameters.Add(map(GetCtorTargetBuilder(new CtorParamTargetConfiguration(parameterName))).Configuration);
            return (T)this;
        }

        public T Auto(string member)
        {
            Configuration.Members.Add(new MemberTargetConfiguration(new StringProjectedMember(member))
            {
                ProjectionTarget = DefaultMemberTarget
            });
            return (T)this;
        }

        //TODO: Using Ctor

        internal TMemberTargetBuilder GetMemberTargetBuilder(MemberTargetConfiguration configuration) 
            => (TMemberTargetBuilder)memberTargetBuilderCtor.Invoke(configuration);
        internal TCtorParamTargetBuilder GetCtorTargetBuilder(CtorParamTargetConfiguration configuration) 
            => (TCtorParamTargetBuilder)ctorParamTargetBuilderCtor.Invoke(configuration);
    }
}
