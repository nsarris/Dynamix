using Dynamix.Expressions;
using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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


    internal class TargetConfigurationCompiler
    {
        readonly Type sourceType;
        readonly Type projectedType;
        readonly IMemberNameMatchStrategy memberNameMatchStrategy;
        readonly ConstructorInfo ctor;

        public TargetConfigurationCompiler(Type sourceType, Type projectedType, IMemberNameMatchStrategy memberNameMatchStrategy, ConstructorInfo ctor)
        {
            this.sourceType = sourceType;
            this.projectedType = projectedType;
            this.memberNameMatchStrategy = memberNameMatchStrategy;
            this.ctor = ctor;
        }

        public CompiledMemberTargetConfiguration Compile(MemberTargetConfiguration configuration, ParameterExpression itParameter)
        {
            var memberInfo = GetMemberInfo(configuration.ProjectedMember);

            var parameter = 
                configuration.ProjectionTarget == ProjectionTarget.CtorParameter ?
                    configuration.CtorParameterName.IsNullOrEmpty() ?
                        GetCtorParameterFromMemberName(memberInfo.Name) :
                        GetCtorParameter(configuration.CtorParameterName) :
                    null;

            var source = configuration.Source ?? new StringProjectionSource(GetSourceMemberNameFromMemberName(memberInfo.Name));
            var sourceExpression = source.GetExpression(itParameter);

            if (configuration.ValueMap != null)
            {
                //var reverse = configuration.ValueMap.Values.ToLookup(x => x.Value, x => x.Key);
            }
            //apply valuemap

            return new CompiledMemberTargetConfiguration()
            {
                MemberInfo = memberInfo,
                MemberType = GetMemberInfoType(memberInfo),
                ProjectedMember = configuration.ProjectedMember,
                SourceExpression = sourceExpression,
                Source = source,
                SourceKey = configuration.SourceKey,
                ValueMap = configuration.ValueMap,
                CtorParameterName = parameter?.Name,
                CtorParameterType = parameter?.ParameterType,
                ProjectionTarget = configuration.ProjectionTarget
            };
        }

        public CompiledCtorParamTargetConfiguration Compile(CtorParamTargetConfiguration configuration, ParameterExpression itParameter)
        {
            var parameter = GetCtorParameter(configuration.ParameterName);

            var source = configuration.Source ?? new StringProjectionSource(GetSourceMemberNameFromCtorParameter(configuration.ParameterName));
            var sourceExpression = source.GetExpression(itParameter);
            //apply valuemap

            return new CompiledCtorParamTargetConfiguration()
            {
                SourceExpression = sourceExpression,
                Source = source, 
                ValueMap = configuration.ValueMap,
                ParameterName = parameter.Name,
            };
        }

        private string GetSourceMemberNameFromCtorParameter(string parameterName)
        {
            var name = sourceType.GetProperties().Select(x => x.Name).FirstOrDefault(x => memberNameMatchStrategy.CtorParameterMatchesSourceMember(parameterName, x));
            if (name != null)
                return name;

            throw new InvalidOperationException($"Cannot infer member from constructor parameter {parameterName} for source type {sourceType.Name}");
        }

        private string GetSourceMemberNameFromMemberName(string memberName)
        {
            var name =  sourceType.GetProperties().Select(x => x.Name).FirstOrDefault(x => memberNameMatchStrategy.MemberMatchesSourceMember(memberName, x));
            if (name != null)
                return name;

            throw new InvalidOperationException($"Cannot infer member from project member {memberName} for source type {sourceType.Name}");
        }

        private ParameterInfo GetCtorParameterFromMemberName(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
                return null;

            var parameter = ctor.GetParameters().FirstOrDefault(x => memberNameMatchStrategy.CtorParameterMatchesMember(x.Name, memberName));
            if (parameter == null)
                throw new InvalidOperationException($"Cannot infer constructor parameter for member {memberName} for selected constructor of type {projectedType.Name}");
            return parameter;
        }

        private ParameterInfo GetCtorParameter(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return null;

            var parameter = ctor.GetParameters().FirstOrDefault(x => x.Name == parameterName);
            if (parameter == null)
                throw new InvalidOperationException($"Cannot find constructor parameter {parameterName} for selected constructor of type {projectedType.Name}");
            return parameter;
        }

        private Type GetMemberInfoType(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                return null;

            return memberInfo.MemberType == MemberTypes.Property ? ((PropertyInfo)memberInfo).PropertyType : ((FieldInfo)memberInfo).FieldType;
        }

        private MemberInfo GetMemberInfo(ProjectedMember projectedMember)
        {
            if (projectedMember == null)
                return null;

            if (projectedMember is StringProjectedMember stringProjectedMember)
            {
                var memberInfo = projectedType
                    .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => x.Name == stringProjectedMember.MemberName);

                if (memberInfo == null)
                    throw new InvalidOperationException($"Member {stringProjectedMember.MemberName} not found in type {sourceType.Name}");

                if (memberInfo.MemberType != MemberTypes.Property && memberInfo.MemberType != MemberTypes.Field)
                    throw new InvalidOperationException("The expession is not a field or property expression");

                return memberInfo;
            }
            else if (projectedMember is LambdaExpressionProjectedMember lambdaExpressionProjectedMember)
            {
                if (lambdaExpressionProjectedMember.PropertyExpression.Body is MemberExpression memberExpression
                    && memberExpression.Expression.Type == sourceType)
                    return memberExpression.Member;
                else
                    throw new InvalidOperationException("The expession is not a valid member expression");
            }
            else
                throw new InvalidOperationException("Unexpected ProjectedMember type");
        }


        internal Expression CreateValueMapExpression(Expression sourceExpression, ValueMap valueMap, Type targetType)
        {
            var defaultValue = Expression.Constant(
                valueMap.UnmappedValueType == UnmappedValueType.TypeDefault ?
                    targetType.IsNullable() ? null : targetType.DefaultOf() :
                valueMap.UnmappedValueType == UnmappedValueType.SetValue ?
                    valueMap.UnmappedValue :
                //else
                sourceExpression);

            return valueMap.Values.Reverse().Aggregate(
                ExpressionEx.ConvertIfNeeded(defaultValue, targetType),
                (current, next) => Expression.Condition(
                                        Expression.Equal(sourceExpression, ExpressionEx.ConvertIfNeeded(Expression.Constant(next.Key), sourceExpression.Type)),
                                        ExpressionEx.ConvertIfNeeded(Expression.Constant(next.Value), targetType),
                                        current));
        }
    }

    internal class CompiledMemberTargetConfiguration
    {
        public MemberInfo MemberInfo { get; set; }
        public Type MemberType { get; set; }
        public string CtorParameterName { get; set; }
        public Type CtorParameterType { get; set; }
        public ProjectionTarget ProjectionTarget { get; set; }
        public ProjectedMember ProjectedMember { get; set; }
        public ProjectionSource Source { get; set; }
        public ProjectionSource SourceKey { get; set; }
        public ValueMap ValueMap { get; set; }
        //public ValueMap ReverseValueMap { get; set; }
        public Expression SourceExpression { get; set; }
    }

    internal class CompiledCtorParamTargetConfiguration
    {
        public string ParameterName { get; set; }
        public Type ParameterType { get; set; }
        public ProjectionSource Source { get; set; }
        public ValueMap ValueMap { get; set; }
        //public ValueMap ReverseValueMap { get; set; }
        public Expression SourceExpression { get; set; }
    }

    internal class CompiledValueMap
    {
        public ValueMap ValueMap { get; set; }
        public Dictionary<object, object> ReverseValues { get; set; }
    }

    internal class CtorParameterAssignment
    {
        public string MemberName { get; }
        public Expression SourceExpression { get; }
        public CtorParameterAssignment(string memberName, Expression sourceExpression)
        {
            MemberName = memberName;
            SourceExpression = sourceExpression;
        }
    }

    internal class MemberInitAssignment
    {
        public string MemberName { get; }
        public MemberAssignment MemberAssignement { get; }
        public MemberInitAssignment(string memberName, MemberAssignment memberAssignement)
        {
            MemberName = memberName;
            MemberAssignement = memberAssignement;
        }
    }


    public class DynamicProjection
    {
        public Type SourceType { get; }
        public Type ProjectedType { get; }

        readonly ParameterExpression it;

        readonly List<MemberTargetConfiguration> members;
        readonly List<CtorParamTargetConfiguration> ctorParameters;
        readonly IMemberNameMatchStrategy memberNameMatchStrategy;

        ConstructorInfo ctor;
        List<CompiledCtorParamTargetConfiguration> compiledCtorParams;
        List<CompiledMemberTargetConfiguration> compiledMembers;
        
        List<MemberInitAssignment> memberInitAssignments;
        List<CtorParameterAssignment> ctorParameterAssignments;

        LambdaExpression defaultProjection;

        internal DynamicProjection(
            Type sourceType, Type projectedType,
            IMemberNameMatchStrategy memberNameMatchStrategy,
            IEnumerable<MemberTargetConfiguration> members,
            IEnumerable<CtorParamTargetConfiguration> ctorParameters)
        {
            SourceType = sourceType;
            ProjectedType = projectedType;

            it = Expression.Parameter(SourceType, ""); 

            this.members = members.ToList();
            this.ctorParameters = ctorParameters.ToList();
            this.memberNameMatchStrategy = memberNameMatchStrategy;

            Compile();
        }

        internal void Compile()
        {
            ctor = ProjectedType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(x => x.GetParameters().Length)
                .FirstOrDefault();
                
            if (ctor == null)
                throw new InvalidOperationException($"No constructor found for type {ProjectedType.Name}");

            var compiler = new TargetConfigurationCompiler(SourceType, ProjectedType, memberNameMatchStrategy, ctor);

            compiledMembers = members.Select(x => compiler.Compile(x, it)).ToList();
            compiledCtorParams = ctorParameters.Select(x => compiler.Compile(x, it)).ToList();

            ctorParameterAssignments =
                ctor.GetParameters().Select(p =>
                {
                    var ctorExpression =
                        compiledCtorParams
                            .Where(x => x.ParameterName == p.Name)
                            .Select(x => new CtorParameterAssignment(null, x.SourceExpression))
                            .FirstOrDefault() ??
                        compiledMembers
                            .Where(x => x.ProjectionTarget == ProjectionTarget.CtorParameter && x.CtorParameterName == p.Name)
                            .Select(x => new CtorParameterAssignment(x.MemberInfo.Name, x.SourceExpression))
                            .FirstOrDefault();

                    if (ctorExpression == null && !p.IsOptional)
                        throw new InvalidOperationException($"No source given for non optional constructor parameter {p.Name} on type {ProjectedType.Name}");

                    return ctorExpression;
                }) 
                .ToList();

            memberInitAssignments =
                compiledMembers
                .Where(x => x.ProjectionTarget == ProjectionTarget.Member)
                .Select(p => new MemberInitAssignment(
                        memberName: 
                            p.MemberInfo.Name,
                        memberAssignement:
                            Expression.Bind(
                            p.MemberInfo,
                            ExpressionEx.ConvertIfNeeded(p.SourceExpression, p.MemberType))))
                        .ToList();

            defaultProjection = Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(ctor, ctorParameterAssignments.Select(x => x.SourceExpression)),
                        memberInitAssignments.Select(x => x.MemberAssignement)),
                    it);

        }

        public LambdaExpression BuildSelector(IEnumerable<string> selectedProperties = null)
        {
            if (selectedProperties == null)
                return defaultProjection;

            //filter assignments
            var ctorAssignments = ctorParameterAssignments.Where(x => string.IsNullOrEmpty(x.MemberName) || selectedProperties.Any(p => p == x.MemberName)).Select(x => x.SourceExpression);
            var memberAssignments = memberInitAssignments.Where(x => selectedProperties.Any(p => p == x.MemberName)).Select(x => x.MemberAssignement);

            return Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(ctor, ctorAssignments),
                        memberAssignments),
                    it);
        }


        public DynamicQueryable BuildQuery(
            IQueryable queryable,
            IEnumerable<string> selectedColumns = null
            )
        {
            var d = new DynamicQueryable(queryable)
                .Select(selectedColumns == null ? defaultProjection : BuildSelector(selectedColumns));

            return d;
        }
    }
}
