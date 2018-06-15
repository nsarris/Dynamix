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


    public static class ProjectionBuilderExtensions
    {
        public static T WithDefaultMemberTarget<T>(this T projectionBuilder, DefaultMemberTarget defaultMemberTarget)
            where T : ProjectionBuilderBase
        {
            projectionBuilder.DefaultMemberTarget = defaultMemberTarget;
            return projectionBuilder;
        }

        public static T WithDefaultMemberTarget<T>(this T projectionBuilder, Func<string, string, bool> defaultMemberNameComparer)
            where T : ProjectionBuilderBase
        {
            projectionBuilder.DefaultMemberNameComparer = defaultMemberNameComparer;
            return projectionBuilder;
        }

        //public static T Auto<T>(this T projectionBuilder, string member)
        //    where T : ProjectionBuilderBase
        //{

        //    return projectionBuilder;
        //}
    }

    public class CtorParameterMap
    {
        public string ParameterName { get; }
        public MapTarget MapTarget { get; }
        public CtorParameterMap(string parameterName, MapTarget mapTarget)
        {
            MapTarget = mapTarget;
            ParameterName = parameterName;
        }
    }

    public class MemberMap
    {
        public MapMember Member { get; }
        public MapTarget MapTarget { get; }
        public MemberMap(MapMember member, MapTarget mapTarget)
        {
            Member = member;
            MapTarget = mapTarget;
        }
    }


    public enum DefaultMemberTarget
    {
        Member,
        CtorParameter
    }

    public abstract class ProjectionBuilderBase
    {
        public Type SourceType { get; }
        public Type ProjectedType { get; }
        //public ParameterExpression ItParameter { get; }
        public DefaultMemberTarget DefaultMemberTarget { get; internal set; }
        public Func<string, string, bool> DefaultMemberNameComparer { get; internal set; }
            = (string x, string y) => StringComparer.Ordinal.Compare(x, y) == 0;

        protected readonly List<MemberMap> mappedMembers = new List<MemberMap>();
        protected readonly List<CtorParameterMap> ctorParameters = new List<CtorParameterMap>();
        //protected readonly ConstructorInfoEx ctor;

        protected ProjectionBuilderBase(Type sourceType, Type projectedType)
        {
            SourceType = sourceType;
            ProjectedType = projectedType;
            //ItParameter = Expression.Parameter(SourceType);

            //ctor = projectedType.GetConstructorsEx()
            //    .OrderByDescending(x => x.Signature.Count())
            //    .FirstOrDefault();
        }

        //protected bool TryGetCtorParamter(string name, out string parameterName, out Type parameterType)
        //{
        //    var item = ctor.Signature.FirstOrDefault(x => DefaultMemberNameComparer(x.Key, name));
        //    parameterName = item.Key;
        //    parameterType = item.Value;
        //    return parameterName != null;
        //}

        //protected bool TryGetMember(string name, out string memberName, out Type memberType)
        //{
        //    memberName = null;
        //    memberType = null;

        //    var prop = ProjectedType.GetPropertiesEx().FirstOrDefault(x => DefaultMemberNameComparer(x.Name, name));
        //    if (prop != null)
        //    {
        //        memberName = prop.Name;
        //        memberType = prop.Type;
        //    }
        //    else
        //    {
        //        var field = ProjectedType.GetFieldsEx().FirstOrDefault(x => DefaultMemberNameComparer(x.Name, name));
        //        if (field != null)
        //        {
        //            memberName = field.Name;
        //            memberType = field.Type;
        //        }
        //    }
        //    return memberName != null;
        //}

        //protected MemberExpression GetMemberExpression(string member)
        //{
        //    return MemberExpressionBuilder.MakeDeepMemberAccess(ItParameter, member);
        //}

        //protected MemberExpression GetMemberExpression(LambdaExpression memberLambdaExpression)
        //{
        //    return LambdaParameterReplacer.Replace(memberLambdaExpression, memberLambdaExpression.Parameters.First(), ItParameter).Body as MemberExpression;
        //}

        public LambdaExpression Build(Type sourceType, Type projectedType)
        //use ctor
        {

            var it = Expression.Parameter(sourceType, "");
            var ctor = projectedType.GetConstructorsEx().OrderByDescending(x => x.Signature.Count).FirstOrDefault();
            //if null error

            var ctorParameterExpressions = new List<Expression>();
            foreach (var p in ctor.Signature)
            {
                var ctorParameter = ctorParameters.FirstOrDefault(x => x.ParameterName == p.Key);
                ctorParameterExpressions.Add(ctorParameter.MapTarget.GetExpression(it));
            }

            //IEnumerable<string> selectedProperties = null;

            var memberAssignments =
                //(selectedProperties == null ? Properties : Properties.Where(x => selectedProperties.Contains(x.TargetProperty.Name)))
                mappedMembers
                    .Select(m =>
                    {
                        var member = m.Member.GetMember(projectedType);
                        var mapTarget = m.MapTarget ?? new StringMapTarget(member.Name);
                        var targetExpression = mapTarget.GetExpression(it);
                        var memberType = member.MemberType == MemberTypes.Property ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;

                        return new
                        {
                            MemberInfo = member,
                            MemberType = memberType,
                            TargetExperssion = targetExpression, //ValueMap
                        };
                    })
                    .Select(p => Expression.Bind(
                            p.MemberInfo,
                            ExpressionEx.ConvertIfNeeded(p.TargetExperssion, p.MemberType)))
                            .ToList();

            var l = Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(ctor.ConstructorInfo, ctorParameterExpressions),
                        memberAssignments),
                    it);



            return l;
        }
    }



    public class ProjectionBuilder : ProjectionBuilderBase
    {

        public ProjectionBuilder(Type sourceType, Type projectedType)
            : base(sourceType, projectedType)
        {

        }



        public ProjectionBuilder Member(string member, Func<MemberTargetBuilder, ConfiguredMemberTargetBuilder> map)
        {
            var mapTarget = map(new MemberTargetBuilder()).MappedMember;
            mappedMembers.Add(new MemberMap(new StringMapMember(member), mapTarget));
            return this;
        }

        public ProjectionBuilder CtorParameter(string parameterName, Func<CtorParamTargetBuilder, ConfiguredCtorParamTargetBuilder> map)
        {
            var ctorParameter = new CtorParameterMap(parameterName, map(new CtorParamTargetBuilder()).MappedMember);
            ctorParameters.Add(ctorParameter);
            return this;
        }

        public ProjectionBuilder Auto(string member)
        {
            mappedMembers.Add(new MemberMap(new StringMapMember(member), null));
            return this;
        }
    }

    public class ProjectionBuilder<TSource> : ProjectionBuilderBase
    {
        public ProjectionBuilder(Type projectedType)
            : base(typeof(TSource), projectedType)
        {
        }

        public ProjectionBuilder<TSource> Member(string member, Func<MemberTargetBuilder<TSource>, ConfiguredMemberTargetBuilder> map)
        {
            var mapTarget = map(new MemberTargetBuilder<TSource>()).MappedMember;
            mappedMembers.Add(new MemberMap(new StringMapMember(member), mapTarget));
            return this;
        }

        public ProjectionBuilder<TSource> CtorParameter(string parameterName, Func<CtorParamTargetBuilder<TSource>, ConfiguredCtorParamTargetBuilder> map)
        {
            var ctorParameter = new CtorParameterMap(parameterName, map(new CtorParamTargetBuilder<TSource>()).MappedMember);
            ctorParameters.Add(ctorParameter);
            return this;
        }

        public ProjectionBuilder<TSource> Auto(string member)
        {
            mappedMembers.Add(new MemberMap(new StringMapMember(member), null));
            return this;
        }
    }

    public class ProjectionBuilder<TSource, TDestination> : ProjectionBuilderBase
    {
        public ProjectionBuilder()
            : base(typeof(TSource), typeof(TDestination))
        {

        }

        public ProjectionBuilder<TSource, TDestination> Member(string member, Func<MemberTargetBuilder<TSource>, ConfiguredMemberTargetBuilder> map)
        {
            var mapTarget = map(new MemberTargetBuilder<TSource>()).MappedMember;
            mappedMembers.Add(new MemberMap(new StringMapMember(member), mapTarget));
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> CtorParameter(string parameterName, Func<CtorParamTargetBuilder<TSource>, ConfiguredCtorParamTargetBuilder> map)
        {
            var ctorParameter = new CtorParameterMap(parameterName, map(new CtorParamTargetBuilder<TSource>()).MappedMember);
            ctorParameters.Add(ctorParameter);
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> Member<TMember>(Expression<Func<TDestination, TMember>> memberLambdaExpression, Func<MemberTargetBuilder<TSource>, ConfiguredMemberTargetBuilder> map)
        {
            var mapTarget = map(new MemberTargetBuilder<TSource>()).MappedMember;
            mappedMembers.Add(new MemberMap(new LambdaExpressionMapMember(memberLambdaExpression), mapTarget));
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> Auto(string member)
        {
            mappedMembers.Add(new MemberMap(new StringMapMember(member), null));
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> Auto<TMember>(Expression<Func<TDestination, TMember>> memberLambdaExpression)
        {
            mappedMembers.Add(new MemberMap(new LambdaExpressionMapMember(memberLambdaExpression), null));
            return this;
        }
    }

    #region MapTargets

    public abstract class MapMember
    {
        public abstract MemberInfo GetMember(Type sourceType);
    }
    public class StringMapMember : MapMember
    {
        public string PropertyName { get; }
        public StringMapMember(string propertyName)
        {
            PropertyName = propertyName;
        }
        public override MemberInfo GetMember(Type sourceType)
        {
            return sourceType.GetMember(PropertyName).First();
        }
    }

    public class LambdaExpressionMapMember : MapMember
    {
        public LambdaExpression PropertyExpression { get; }
        public LambdaExpressionMapMember(LambdaExpression propertyExpression)
        {
            PropertyExpression = propertyExpression;
        }
        public override MemberInfo GetMember(Type sourceType)
        {
            if (PropertyExpression.Body is MemberExpression memberExpression)
                return memberExpression.Member;
            else
                throw new InvalidOperationException("The expession give is not a valid member expression");
        }
    }

    public abstract class MapTarget
    {
        public abstract Expression GetExpression(ParameterExpression itParameter);
    }

    public class StringMapTarget : MapTarget
    {
        internal string SourceExpression { get; }
        internal StringMapTarget(string sourceExpression)
        {
            SourceExpression = sourceExpression;
        }

        public override Expression GetExpression(ParameterExpression itParameter)
        {
            return System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, null, SourceExpression);
            //return MemberExpressionBuilder.GetExpressionSelector(itParameter, SourceExpression).Body;
        }
    }

    public class ExpressionMapTarget : MapTarget
    {
        internal Expression SourceExpression { get; }
        internal ExpressionMapTarget(Expression sourceExpression)
        {
            SourceExpression = sourceExpression;
        }

        public override Expression GetExpression(ParameterExpression itParameter)
        {
            return LambdaParameterReplacer.ReplaceOfType(SourceExpression, itParameter.Type, itParameter);
        }
    }

    public class LambdaExpressionMapTarget : MapTarget
    {
        internal LambdaExpression SourceExpression { get; }
        internal LambdaExpressionMapTarget(LambdaExpression sourceExpression)
        {
            SourceExpression = sourceExpression;
        }

        public override Expression GetExpression(ParameterExpression itParameter)
        {
            return LambdaParameterReplacer.Replace(SourceExpression, SourceExpression.Parameters.First(), itParameter);
        }
    }

    public class ConstantMapTarget : MapTarget
    {
        public object Value { get; }
        internal ConstantMapTarget(object value)
        {
            Value = value;
        }

        public override Expression GetExpression(ParameterExpression itParameter)
        {
            return Expression.Constant(Value);
        }
    }

    #endregion



    #region Member Target Builder

    public class ValueMap
    {
        public Dictionary<object, object> Values { get; }
        public UnmappedValueType UnmappedValueType { get; }
        public object UnmappedValue { get; }
        internal ValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType, object unmappedValue)
        {
            Values = values;
            UnmappedValueType = unmappedValueType;
            UnmappedValue = unmappedValue;
        }
    }

    public enum UnmappedValueType
    {
        TypeDefault,
        SetValue,
        OriginalValue
    }

    public abstract class MemberTargetBuilderBase
    {
        internal string ParameterName { get; set; }
        internal DefaultMemberTarget DefaultMemberTarget { get; set; } = DefaultMemberTarget.Member;
    }


    public class MemberTargetBuilder : MemberTargetBuilderBase
    {
        public MemberTargetBuilder()
        {
        }

        public MemberTargetBuilder UsingCtorParameter(string parameterName)
        {
            ParameterName = parameterName;
            DefaultMemberTarget = DefaultMemberTarget.CtorParameter;
            return this;
        }

        public MemberTargetBuilder UsingCtorParameter()
        {
            ParameterName = null;
            DefaultMemberTarget = DefaultMemberTarget.CtorParameter;
            return this;
        }

        public ExpressionMemberTargetBuilder FromExpression(string expression)
        {
            return new ExpressionMemberTargetBuilder(this, new StringMapTarget(expression));
        }

        public ExpressionMemberTargetBuilder FromExpression(Expression expression)
        {
            return new ExpressionMemberTargetBuilder(this, new ExpressionMapTarget(expression));
        }

        public ConstantMemberTargetBuilder FromValue(object value)
        {
            return new ConstantMemberTargetBuilder(this, new ConstantMapTarget(value));
        }
    }

    public abstract class ConfiguredMemberTargetBuilder
    {
        internal MemberTargetBuilderBase TargetBuilder { get; }
        internal MapTarget MappedMember { get; }
        internal MapTarget MappedKey { get; private set; }

        protected ConfiguredMemberTargetBuilder(MemberTargetBuilderBase memberTargetBuilder, MapTarget mappedMember)
        {
            TargetBuilder = memberTargetBuilder;
            MappedMember = mappedMember;
        }

        protected void SetMappedKey(MapTarget mappedKey)
        {
            MappedKey = mappedKey;
        }
    }

    public class ExpressionMemberTargetBuilder : ConfiguredMemberTargetBuilder
    {
        internal ValueMap ValueMap { get; private set; }

        public ExpressionMemberTargetBuilder(MemberTargetBuilderBase memberTargetBuilder, MapTarget mappedMember)
            : base(memberTargetBuilder, mappedMember)
        {

        }

        public ExpressionMemberTargetBuilder MapsToKeyExpression(string expression)
        {
            SetMappedKey(new StringMapTarget(expression));
            return this;
        }

        public ExpressionMemberTargetBuilder MapsToKeyExpression(Expression expression)
        {
            SetMappedKey(new ExpressionMapTarget(expression));
            return this;
        }

        public ExpressionMemberTargetBuilder HasValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType = UnmappedValueType.TypeDefault, object unmappedValue = null)
        {
            ValueMap = new ValueMap(values, unmappedValueType, unmappedValue);
            return this;
        }
    }

    public class ConstantMemberTargetBuilder : ConfiguredMemberTargetBuilder
    {
        public ConstantMemberTargetBuilder(MemberTargetBuilderBase memberTargetBuilder, MapTarget mappedMember)
            : base(memberTargetBuilder, mappedMember)
        {
        }
    }

    #endregion

    #region Member Target Builder <TSource>

    public class MemberTargetBuilder<TSource> : MemberTargetBuilderBase
    {
        public MemberTargetBuilder()
        {
        }

        public MemberTargetBuilder<TSource> UsingCtorParameter(string parameterName)
        {
            ParameterName = parameterName;
            DefaultMemberTarget = DefaultMemberTarget.CtorParameter;
            return this;
        }

        public MemberTargetBuilder<TSource> UsingCtorParameter()
        {
            ParameterName = null;
            DefaultMemberTarget = DefaultMemberTarget.CtorParameter;
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> FromExpression(string expression)
        {
            return new ExpressionMemberTargetBuilder<TSource>(this, new StringMapTarget(expression));
        }

        public ExpressionMemberTargetBuilder<TSource> FromExpression(Expression expression)
        {
            return new ExpressionMemberTargetBuilder<TSource>(this, new ExpressionMapTarget(expression));
        }

        public ExpressionMemberTargetBuilder<TSource> FromExpression<T>(Expression<Func<TSource, T>> expression)
        {
            return new ExpressionMemberTargetBuilder<TSource>(this, new LambdaExpressionMapTarget(expression));
        }

        public ConstantMemberTargetBuilder<TSource> FromValue(object value)
        {
            return new ConstantMemberTargetBuilder<TSource>(this, new ConstantMapTarget(value));
        }

    }




    public class ExpressionMemberTargetBuilder<TSource> : ConfiguredMemberTargetBuilder
    {
        internal ValueMap ValueMap { get; set; }

        public ExpressionMemberTargetBuilder(MemberTargetBuilderBase memberTargetBuilder, MapTarget mappedMember)
            : base(memberTargetBuilder, mappedMember)
        {
        }

        public ExpressionMemberTargetBuilder<TSource> MapsToKeyExpression(string expression)
        {
            SetMappedKey(new StringMapTarget(expression));
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> MapsToKeyExpression(Expression expression)
        {
            SetMappedKey(new ExpressionMapTarget(expression));
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> MapsToKeyExpression<T>(Expression<Func<TSource, T>> expression)
        {
            SetMappedKey(new LambdaExpressionMapTarget(expression));
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> HasValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType = UnmappedValueType.TypeDefault, object unmappedValue = null)
        {
            ValueMap = new ValueMap(values, unmappedValueType, unmappedValue);
            return this;
        }
    }

    public class ConstantMemberTargetBuilder<TSource> : ConfiguredMemberTargetBuilder
    {
        public ConstantMemberTargetBuilder(MemberTargetBuilderBase memberTargetBuilder, MapTarget mappedMember)
            : base(memberTargetBuilder, mappedMember)
        {
        }
    }

    #endregion


    #region Ctor Target Builder

    public class CtorParamTargetBuilderBase
    {

    }

    public class CtorParamTargetBuilder : CtorParamTargetBuilderBase
    {
        public CtorParamTargetBuilder()
        {
        }

        public ExpressionCtorParamTargetBuilder FromExpression(string expression)
        {
            return new ExpressionCtorParamTargetBuilder(this, new StringMapTarget(expression));
        }

        public ExpressionCtorParamTargetBuilder FromExpression(Expression expression)
        {
            return new ExpressionCtorParamTargetBuilder(this, new ExpressionMapTarget(expression));
        }

        public ConstantCtorParamTargetBuilder FromValue(object value)
        {
            return new ConstantCtorParamTargetBuilder(this, new ConstantMapTarget(value));
        }

    }

    public abstract class ConfiguredCtorParamTargetBuilder
    {
        internal CtorParamTargetBuilderBase TargetBuilder { get; }
        internal MapTarget MappedMember { get; }
        internal MapTarget MappedKey { get; private set; }

        protected ConfiguredCtorParamTargetBuilder(CtorParamTargetBuilderBase targetBuilder, MapTarget mappedMember)
        {
            TargetBuilder = targetBuilder;
            MappedMember = mappedMember;
        }

        protected void SetMappedKey(MapTarget mappedKey)
        {
            MappedKey = mappedKey;
        }
    }

    public class ExpressionCtorParamTargetBuilder : ConfiguredCtorParamTargetBuilder
    {
        internal ValueMap ValueMap { get; private set; }

        public ExpressionCtorParamTargetBuilder(CtorParamTargetBuilderBase targetBuilder, MapTarget mappedMember)
            : base(targetBuilder, mappedMember)
        {

        }

        public ExpressionCtorParamTargetBuilder MapsToKeyExpression(string expression)
        {
            SetMappedKey(new StringMapTarget(expression));
            return this;
        }

        public ExpressionCtorParamTargetBuilder MapsToKeyExpression(Expression expression)
        {
            SetMappedKey(new ExpressionMapTarget(expression));
            return this;
        }

        public ExpressionCtorParamTargetBuilder HasValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType = UnmappedValueType.TypeDefault, object unmappedValue = null)
        {
            ValueMap = new ValueMap(values, unmappedValueType, unmappedValue);
            return this;
        }
    }

    public class ConstantCtorParamTargetBuilder : ConfiguredCtorParamTargetBuilder
    {
        public ConstantCtorParamTargetBuilder(CtorParamTargetBuilderBase targetBuilder, MapTarget mappedMember)
            : base(targetBuilder, mappedMember)
        {
        }
    }

    #endregion

    #region Ctor Target Builder <TSource>

    public class CtorParamTargetBuilder<TSource> : CtorParamTargetBuilderBase
    {
        public CtorParamTargetBuilder()
        {
        }

        public ExpressionCtorParamTargetBuilder<TSource> FromExpression(string expression)
        {
            return new ExpressionCtorParamTargetBuilder<TSource>(this, new StringMapTarget(expression));
        }

        public ExpressionCtorParamTargetBuilder<TSource> FromExpression(Expression expression)
        {
            return new ExpressionCtorParamTargetBuilder<TSource>(this, new ExpressionMapTarget(expression));
        }

        public ExpressionCtorParamTargetBuilder<TSource> FromExpression<T>(Expression<Func<TSource, T>> expression)
        {
            return new ExpressionCtorParamTargetBuilder<TSource>(this, new LambdaExpressionMapTarget(expression));
        }

        public ConstantCtorParamTargetBuilder<TSource> FromValue(object value)
        {
            return new ConstantCtorParamTargetBuilder<TSource>(this, new ConstantMapTarget(value));
        }
    }




    public class ExpressionCtorParamTargetBuilder<TSource> : ConfiguredCtorParamTargetBuilder
    {
        internal ValueMap ValueMap { get; set; }

        public ExpressionCtorParamTargetBuilder(CtorParamTargetBuilderBase targetBuilder, MapTarget mappedMember)
            : base(targetBuilder, mappedMember)
        {
        }

        public ExpressionCtorParamTargetBuilder<TSource> MapsToKeyExpression(string expression)
        {
            SetMappedKey(new StringMapTarget(expression));
            return this;
        }

        public ExpressionCtorParamTargetBuilder<TSource> MapsToKeyExpression(Expression expression)
        {
            SetMappedKey(new ExpressionMapTarget(expression));
            return this;
        }

        public ExpressionCtorParamTargetBuilder<TSource> MapsToKeyExpression<T>(Expression<Func<TSource, T>> expression)
        {
            SetMappedKey(new LambdaExpressionMapTarget(expression));
            return this;
        }

        public ExpressionCtorParamTargetBuilder<TSource> HasValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType = UnmappedValueType.TypeDefault, object unmappedValue = null)
        {
            ValueMap = new ValueMap(values, unmappedValueType, unmappedValue);
            return this;
        }
    }

    public class ConstantCtorParamTargetBuilder<TSource> : ConfiguredCtorParamTargetBuilder
    {
        public ConstantCtorParamTargetBuilder(CtorParamTargetBuilderBase memberTargetBuilder, MapTarget mappedMember)
            : base(memberTargetBuilder, mappedMember)
        {
        }
    }

    #endregion





}
