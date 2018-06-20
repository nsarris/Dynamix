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

        private CompiledMemberTargetConfiguration CompileMember(MemberTargetConfiguration memberConfiguration)
        {
            var memberInfo = GetMemberInfo(configuration.SourceType, configuration.ProjectedType, memberConfiguration.ProjectedMember);

            var parameter = 
                memberConfiguration.ProjectionTarget == ProjectionTarget.CtorParameter ?
                    memberConfiguration.CtorParameterName.IsNullOrEmpty() ?
                        GetCtorParameterFromMemberName(memberInfo.Name) :
                        GetCtorParameter(memberConfiguration.CtorParameterName) :
                    null;

            var source = memberConfiguration.Source ?? new StringProjectionSource(GetSourceMemberNameFromMemberName(memberInfo.Name));
            var sourceExpression = source.GetExpression(configuration.It);

            if (memberConfiguration.ValueMap != null)
            {
                //var reverse = configuration.ValueMap.Values.ToLookup(x => x.Value, x => x.Key);
            }
            //apply valuemap

            return new CompiledMemberTargetConfiguration()
            {
                MemberInfo = memberInfo,
                MemberType = GetMemberInfoType(memberInfo),
                ProjectedMember = memberConfiguration.ProjectedMember,
                SourceExpression = sourceExpression,
                Source = source,
                SourceKey = memberConfiguration.SourceKey,
                ValueMap = memberConfiguration.ValueMap,
                CtorParameterName = parameter?.Name,
                CtorParameterType = parameter?.ParameterType,
                ProjectionTarget = memberConfiguration.ProjectionTarget
            };
        }

        private CompiledCtorParamTargetConfiguration CompileCtorParameter(CtorParamTargetConfiguration ctorParamconfiguration)
        {
            var parameter = GetCtorParameter(ctorParamconfiguration.ParameterName);

            var source = ctorParamconfiguration.Source ?? new StringProjectionSource(GetSourceMemberNameFromCtorParameter(ctorParamconfiguration.ParameterName));
            var sourceExpression = source.GetExpression(configuration.It);
            //apply valuemap

            return new CompiledCtorParamTargetConfiguration()
            {
                SourceExpression = sourceExpression,
                Source = source, 
                ValueMap = ctorParamconfiguration.ValueMap,
                ParameterName = parameter.Name,
            };
        }

        private CompiledDynamicProjectionConfiguration Compile()
        {
            configuration.ProjectedType =
                configuration.ProjectedType ?? BuildProjectedType();

            configuration.Ctor = configuration.Ctor ?? configuration.ProjectedType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(x => x.GetParameters().Length)
                .FirstOrDefault();

            if (configuration.Ctor == null)
                throw new InvalidOperationException($"No constructor found for type {configuration.ProjectedType.Name}");

            var compiledMembers = configuration.Members.Select(x => CompileMember(x)).ToList();
            var compiledCtorParams = configuration.CtorParameters.Select(x => CompileCtorParameter(x)).ToList();

            var ctorParameterAssignments =
                configuration.Ctor.GetParameters().Select(p =>
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
                        throw new InvalidOperationException($"No source given for non optional constructor parameter {p.Name} on type {configuration.ProjectedType.Name}");

                    return ctorExpression;
                })
                .ToList();

            var memberInitAssignments =
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

            var defaultProjection = Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(configuration.Ctor, ctorParameterAssignments.Select(x => x.SourceExpression)),
                        memberInitAssignments.Select(x => x.MemberAssignement)),
                    configuration.It);

            return new CompiledDynamicProjectionConfiguration(
                configuration, 
                compiledCtorParams, compiledMembers, memberInitAssignments, ctorParameterAssignments, defaultProjection);
        }

        public Type BuildProjectedType()
        {
            if (configuration.CtorParameters.Any())
                throw new InvalidOperationException("Cannot build projected type with explicit constructor parameter mapping");

            var typeBuilder = new DynamicTypeDescriptorBuilder("SomeDynamicType");

            foreach (var member in configuration.Members)
                typeBuilder.AddProperty(
                    member.ProjectedMember.GetName(),
                    member.AsType ?? member.ProjectedMember.GetMemberType() ?? member.Source.GetExpression(configuration.It)?.Type ?? typeof(object), 
                    config =>
                {
                    if (member.ProjectionTarget == ProjectionTarget.CtorParameter)
                        config.IsInitializedInConstructorOptional(member.CtorParameterName);

                    config.AsNullable(member.AsNullable);
                    
                    return config;
                });
            //TODO: fix type registration
            return DynamicTypeBuilder.Instance.CreateAndRegisterType(typeBuilder.Build());
        }


        private string GetSourceMemberNameFromCtorParameter(string parameterName)
        {
            var name = configuration.SourceType.GetProperties().Select(x => x.Name).FirstOrDefault(x => configuration.MemberNameMatchStrategy.CtorParameterMatchesSourceMember(parameterName, x));
            if (name != null)
                return name;

            throw new InvalidOperationException($"Cannot infer member from constructor parameter {parameterName} for source type {configuration.SourceType.Name}");
        }

        private string GetSourceMemberNameFromMemberName(string memberName)
        {
            var name = configuration.SourceType.GetProperties().Select(x => x.Name).FirstOrDefault(x => configuration.MemberNameMatchStrategy.MemberMatchesSourceMember(memberName, x));
            if (name != null)
                return name;

            throw new InvalidOperationException($"Cannot infer member from project member {memberName} for source type {configuration.SourceType.Name}");
        }

        private ParameterInfo GetCtorParameterFromMemberName(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
                return null;

            var parameter = configuration.Ctor.GetParameters().FirstOrDefault(x => configuration.MemberNameMatchStrategy.CtorParameterMatchesMember(x.Name, memberName));
            if (parameter == null)
                throw new InvalidOperationException($"Cannot infer constructor parameter for member {memberName} for selected constructor of type {configuration.ProjectedType.Name}");
            return parameter;
        }

        private ParameterInfo GetCtorParameter(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return null;

            var parameter = configuration.Ctor.GetParameters().FirstOrDefault(x => x.Name == parameterName);
            if (parameter == null)
                throw new InvalidOperationException($"Cannot find constructor parameter {parameterName} for selected constructor of type {configuration.ProjectedType.Name}");
            return parameter;
        }


        private Type GetMemberInfoType(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                return null;

            return memberInfo.MemberType == MemberTypes.Property ? ((PropertyInfo)memberInfo).PropertyType : ((FieldInfo)memberInfo).FieldType;
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
