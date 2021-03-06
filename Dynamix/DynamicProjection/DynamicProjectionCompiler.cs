﻿using Dynamix.Expressions;
using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix.DynamicProjection
{
    internal class DynamicProjectionCompiler
    {
        readonly DynamicProjectionConfiguration configuration;
        readonly Lazy<CompiledDynamicProjectionConfiguration> compiledConfigurationLazy;
        CompiledDynamicProjectionConfiguration CompiledConfiguration => compiledConfigurationLazy.Value;

        public DynamicProjectionCompiler(DynamicProjectionConfiguration configuration)
        {
            this.configuration = configuration;
            compiledConfigurationLazy = new Lazy<CompiledDynamicProjectionConfiguration>(Compile);
        }

        public CompiledDynamicProjectionConfiguration GetCompiledConfiguration() => compiledConfigurationLazy.Value;

        private CompiledMemberTargetConfiguration.CompiledMemberConfiguration CompileMember(MemberTargetConfiguration memberConfiguration)
        {
            var memberInfo = GetMemberInfo(configuration.SourceType, configuration.ProjectedType, memberConfiguration.ProjectedMember);

            var parameter =
                memberConfiguration.ProjectionTarget == ProjectionTarget.CtorParameter ?
                    memberConfiguration.CtorParameterName.IsNullOrEmpty() ?
                        GetCtorParameterFromMemberName(memberInfo.Name) :
                        GetCtorParameter(memberConfiguration.CtorParameterName) :
                    null;

            if (memberConfiguration.ValueMap != null)
            {
                //var reverse = configuration.ValueMap.Values.ToLookup(x => x.Value, x => x.Key);
            }
            //apply valuemap

            return new CompiledMemberTargetConfiguration.CompiledMemberConfiguration()
            {
                MemberInfo = memberInfo,
                MemberType = GetMemberInfoType(memberInfo),
                CtorParameterName = parameter?.Name,
                CtorParameterType = parameter?.ParameterType,
                ProjectionTarget = memberConfiguration.ProjectionTarget
            };
        }

        private CompiledMemberTargetConfiguration.CompiledSourceConfiguration CompileMemberSource(
            CompiledMemberTargetConfiguration.CompiledMemberConfiguration member, MemberTargetConfiguration memberConfiguration)
        {
            var source = memberConfiguration.Source ?? new StringProjectionSource(GetSourceMemberNameFromMemberName(member.MemberInfo.Name));
            var sourceExpression = GetExpression(source, configuration.It);

            return new CompiledMemberTargetConfiguration.CompiledSourceConfiguration()
            {
                SourceExpression = sourceExpression,
                Source = source,
                SourceKey = memberConfiguration.SourceKey,
                ValueMap = memberConfiguration.ValueMap,
            };
        }

        private CompiledCtorParamTargetConfiguration.CompiledCtorParamConfiguration CompileCtorParameter(CtorParamTargetConfiguration ctorParamconfiguration)
        {
            return new CompiledCtorParamTargetConfiguration.CompiledCtorParamConfiguration
            {
                ParameterInfo = GetCtorParameter(ctorParamconfiguration.ParameterName)
            };
        }

        private CompiledCtorParamTargetConfiguration.CompiledSourceConfiguration CompileCtorParameterSource(
            CompiledCtorParamTargetConfiguration.CompiledCtorParamConfiguration ctorParam, CtorParamTargetConfiguration ctorParamconfiguration)
        {
            var source = ctorParamconfiguration.Source ?? new StringProjectionSource(GetSourceMemberNameFromCtorParameter(ctorParam.ParameterInfo.Name));
            var sourceExpression = GetExpression(source, configuration.It);
            //apply valuemap

            return new CompiledCtorParamTargetConfiguration.CompiledSourceConfiguration()
            {
                SourceExpression = sourceExpression,
                Source = source,
                ValueMap = ctorParamconfiguration.ValueMap,
            };
        }

        private CompiledDynamicProjectionConfiguration Compile()
        {
            configuration.ProjectedType =
                configuration.ProjectedType ??
#if NETSTANDARD2_0
                throw new InvalidOperationException("Projected type not set. Dynamic type building in not supported in net standard.");
#else
                BuildProjectedType();
#endif
            configuration.Ctor = configuration.Ctor ?? FindMatchingConstructor();

            if (configuration.Ctor == null)
                throw new InvalidOperationException($"No matching constructor found for type {configuration.ProjectedType.Name}");

            var compiledMembers = configuration.Members.Select(x => (Configuration: x, CompiledMember: CompileMember(x))).ToList();
            var compiledCtorParams = configuration.CtorParameters.Select(x => (Configuration: x, CompiledCtorParam: CompileCtorParameter(x))).ToList();

            var compiledMemberTargets = compiledMembers.Select(x => new CompiledMemberTargetConfiguration(x.CompiledMember, CompileMemberSource(x.CompiledMember, x.Configuration))).ToList();
            var compiledCtorParamTargets = compiledCtorParams.Select(x => new CompiledCtorParamTargetConfiguration(x.CompiledCtorParam, CompileCtorParameterSource(x.CompiledCtorParam, x.Configuration))).ToList();

            var ctorParameterAssignments =
                configuration.Ctor.GetParameters().Select(p =>
                        compiledCtorParamTargets
                            .Where(x => x.CtorParameter.ParameterInfo.Name == p.Name)
                            .Select(x => new CtorParameterAssignment(null, x.Source.SourceExpression))
                            .FirstOrDefault() ??
                        compiledMemberTargets
                            .Where(x => x.Member.ProjectionTarget == ProjectionTarget.CtorParameter && x.Member.CtorParameterName == p.Name)
                            .Select(x => new CtorParameterAssignment(x.Member.MemberInfo.Name, x.Source.SourceExpression))
                            .FirstOrDefault() ??
                        (p.HasDefaultValue
                                ? new CtorParameterAssignment(null, Expression.Constant(p.DefaultValue))
                                : throw new InvalidOperationException($"No source given for non optional constructor parameter {p.Name} on type {configuration.ProjectedType.Name}")))
                .ToList();

            var memberInitAssignments =
                compiledMemberTargets
                .Where(x => x.Member.ProjectionTarget == ProjectionTarget.Member)
                .Select(p => new MemberInitAssignment(
                        memberName:
                            p.Member.MemberInfo.Name,
                        memberAssignement:
                            Expression.Bind(
                            p.Member.MemberInfo,
                            ExpressionEx.ConvertIfNeeded(p.Source.SourceExpression, p.Member.MemberType))))
                        .ToList();

            var defaultProjection = Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(configuration.Ctor, ctorParameterAssignments.Select(x => x.SourceExpression)),
                        memberInitAssignments.Select(x => x.MemberAssignement)),
                    configuration.It);

            return new CompiledDynamicProjectionConfiguration(
                configuration,
                compiledCtorParamTargets,
                compiledMemberTargets, memberInitAssignments, ctorParameterAssignments, defaultProjection);
        }

#if !NETSTANDARD2_0
        private Type BuildProjectedType()
        {
            if (configuration.CtorParameters.Any())
                throw new InvalidOperationException("Cannot build projected type with explicit constructor parameter mapping");

            var typeBuilder = new DynamicTypeDescriptorBuilder(configuration.ProjectedTypeName);

            foreach (var member in configuration.Members)
                typeBuilder.AddProperty(
                    member.ProjectedMember.GetName(),
                        //overriden type
                        member.AsType ??
                        //specified member type
                        member.ProjectedMember.GetMemberType() ??
                        //auto
                        (member.Source == null ?
                        GetMemberInfoType(
                            configuration.SourceType.GetMember(
                                GetSourceMemberNameFromMemberName(
                                    member.ProjectedMember.GetName())).First()) :
                         //expression
                         GetExpression(member.Source, configuration.It)?.Type) ??
                        //fallback
                        typeof(object),
                    config =>
                {
                    if (member.ProjectionTarget == ProjectionTarget.CtorParameter)
                        config.IsInitializedInConstructorOptional(member.CtorParameterName);

                    config.AsNullable(member.AsNullable);
                });

            return DynamicTypeBuilder.Instance.CreateType(typeBuilder);
        }
#endif
        private ConstructorInfo FindMatchingConstructor()
        {
            var ctorParameters = configuration
                    .CtorParameters
                        .Select(x => x.ParameterName)
                    .Concat(configuration.Members
                        .Where(x => !string.IsNullOrEmpty(x.CtorParameterName)
                            || x.ProjectionTarget == ProjectionTarget.CtorParameter)
                        .Select(x => !string.IsNullOrEmpty(x.CtorParameterName)
                            ? x.CtorParameterName
                            : GetSourceMemberName(x.Source)))
                    .ToList();

            return
                configuration.ProjectedType
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Select(x => new
                    {
                        Ctor = x,
                        Parameters = x.GetParameters()
                    })
                    .FirstOrDefault(x =>
                        x.Parameters.Length >= ctorParameters.Count &&
                        x.Parameters.All(p =>
                            p.HasDefaultValue ||
                            ctorParameters.Any(cp => configuration.MemberNameMatchStrategy.CtorParameterMatchesSourceMember(p.Name, cp))))
                    ?.Ctor
                    ??
                    throw new InvalidOperationException($"No matching constructor found for type {configuration.ProjectedType.Name} with parameters {string.Join(",", ctorParameters)}");
        }

        private string GetSourceMemberNameFromCtorParameter(string parameterName)
        {
            return configuration.SourceType.GetProperties().Select(x => x.Name).FirstOrDefault(x => configuration.MemberNameMatchStrategy.CtorParameterMatchesSourceMember(parameterName, x))
            ??
            throw new InvalidOperationException($"Cannot infer source member from constructor parameter {parameterName} for source type {configuration.SourceType.Name}");
        }


        private string GetSourceMemberNameFromMemberName(string memberName)
        {
            return configuration.SourceType.GetProperties().Select(x => x.Name).FirstOrDefault(x => configuration.MemberNameMatchStrategy.MemberMatchesSourceMember(memberName, x))
            ??
            throw new InvalidOperationException($"Cannot infer source member from projected member {memberName} for source type {configuration.SourceType.Name}");
        }

        private ParameterInfo GetCtorParameterFromMemberName(string memberName)
        {
            return
                 string.IsNullOrEmpty(memberName) ?
                 null :
                 configuration.Ctor.GetParameters().FirstOrDefault(x => configuration.MemberNameMatchStrategy.CtorParameterMatchesMember(x.Name, memberName))
                 ??
                 throw new InvalidOperationException($"Cannot infer constructor parameter for member {memberName} for selected constructor of type {configuration.ProjectedType.Name}");
        }

        private ParameterInfo GetCtorParameter(string parameterName)
        {
            return string.IsNullOrEmpty(parameterName) ?
                null :
                configuration.Ctor.GetParameters().FirstOrDefault(x => x.Name == parameterName)
                ??
                throw new InvalidOperationException($"Cannot find constructor parameter {parameterName} for selected constructor of type {configuration.ProjectedType.Name}");
        }


        private Type GetMemberInfoType(MemberInfo memberInfo)
        {
            return memberInfo == null ?
                    null :
                    memberInfo.MemberType == MemberTypes.Property ? ((PropertyInfo)memberInfo).PropertyType : ((FieldInfo)memberInfo).FieldType;
        }

        private Expression GetExpression(IProjectionSource source, ParameterExpression itParameter)
        {
            switch (source)
            {
                case StringProjectionSource stringProjectionSource:
                    return MemberExpressionBuilder.GetExpression(itParameter, stringProjectionSource.SourceExpression);
                case ExpressionProjectionSource expressionProjectionSource:
                    return Expressions.ExpressionParameterReplacer.Replace(expressionProjectionSource.SourceExpression, itParameter.Type, itParameter);
                case LambdaExpressionProjectionSource lambdaExpressionProjectionSource:
                    return Expressions.ExpressionParameterReplacer.Replace(lambdaExpressionProjectionSource.SourceExpression, lambdaExpressionProjectionSource.SourceExpression.Parameters.First(), itParameter);
                case ConstantProjectionSource constantProjectionSource:
                    return Expression.Constant(constantProjectionSource.Value);
            }

            throw new InvalidOperationException("Unexpected projection source type");
        }

        private string GetSourceMemberName(IProjectionSource source)
        {
            switch (source)
            {
                case StringProjectionSource stringProjectionSource:
                    return stringProjectionSource.SourceExpression;
                case ExpressionProjectionSource expressionProjectionSource:
                    return expressionProjectionSource.SourceExpression is MemberExpression memberExpression ? memberExpression.Member.Name
                        : throw new InvalidOperationException("Source expression is not a member expression. Cannot infer name.");
                case LambdaExpressionProjectionSource lambdaExpressionProjectionSource:
                    return lambdaExpressionProjectionSource.SourceExpression.Body is MemberExpression lambdaMemberExpression ? lambdaMemberExpression.Member.Name
                        : throw new InvalidOperationException("Source lambda expression is not a member expression. Cannot infer name.");
                case ConstantProjectionSource constantProjectionSource:
                    throw new InvalidOperationException("Cannot infer member name from constant source expression.");
            }

            throw new InvalidOperationException("Unexpected projection source type");
        }

        private MemberInfo GetMemberInfo(Type sourceType, Type projectedType, ProjectedMember projectedMember)
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
                valueMap.UnmappedValueType == UnmappedValueType.Constant ?
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

        public LambdaExpression BuildSelector(IEnumerable<string> selectedProperties = null)
        {
            if (selectedProperties == null)
                return CompiledConfiguration.DefaultSelector;

            //filter assignments
            var ctorAssignments = CompiledConfiguration.CtorParameterAssignments.Where(x => string.IsNullOrEmpty(x.MemberName) || selectedProperties.Any(p => p == x.MemberName)).Select(x => x.SourceExpression);
            var memberAssignments = CompiledConfiguration.MemberInitAssignments.Where(x => selectedProperties.Any(p => p == x.MemberName)).Select(x => x.MemberAssignement);

            return Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(CompiledConfiguration.Ctor, ctorAssignments),
                        memberAssignments),
                    CompiledConfiguration.It);
        }
    }
}
