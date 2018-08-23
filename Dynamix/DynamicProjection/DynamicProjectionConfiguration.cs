using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.DynamicProjection
{
    internal class DynamicProjectionConfiguration
    {
        public Type SourceType { get; }
        public Type ProjectedType { get; set; }
        public string ProjectedTypeName { get; set; }
        public ParameterExpression It { get; }
        public List<MemberTargetConfiguration> Members { get; set; } = new List<MemberTargetConfiguration>();
        public List<CtorParamTargetConfiguration> CtorParameters { get; set; } = new List<CtorParamTargetConfiguration>();
        public IMemberNameMatchStrategy MemberNameMatchStrategy { get; set; } = new DefaultMemberNameMatchStrategy();
        public ConstructorInfo Ctor { get; set; }

        public DynamicProjectionConfiguration(
            Type sourceType, Type projectedType
            )
        {
            SourceType = sourceType;
            ProjectedType = projectedType;
            It = Expression.Parameter(SourceType);
        }

        public DynamicProjectionConfiguration Clone(Type sourceType, Type projectedType, bool dynamicProjectedType,string dynamicTypeName)
        {
            return new DynamicProjectionConfiguration(sourceType ?? SourceType, dynamicProjectedType ? null : projectedType ?? ProjectedType)
            {
                Members = Members.ToList(),
                ProjectedTypeName = dynamicTypeName,
                CtorParameters = CtorParameters.ToList(),
                MemberNameMatchStrategy = MemberNameMatchStrategy,
                Ctor = Ctor
            };
        }
    }
}
