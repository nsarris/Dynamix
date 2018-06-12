using Dynamix.Expressions;
using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public MemberExpression Member { get; }
        public MapTarget MapTarget { get; }
        public MemberMap(MemberExpression member, MapTarget mapTarget)
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
        public ParameterExpression ItParameter { get; }
        public DefaultMemberTarget DefaultMemberTarget { get; internal set; }
        public Func<string, string, bool> DefaultMemberNameComparer { get; internal set; }
            = (string x, string y) => StringComparer.Ordinal.Compare(x, y) == 0;

        protected readonly List<MemberMap> mappedMembers = new List<MemberMap>();
        protected readonly List<CtorParameterMap> ctorParameters = new List<CtorParameterMap>();
        protected readonly ConstructorInfoEx ctor;

        protected ProjectionBuilderBase(Type sourceType, Type projectedType)
        {
            SourceType = sourceType;
            ProjectedType = projectedType;
            ItParameter = Expression.Parameter(SourceType);

            ctor = projectedType.GetConstructorsEx()
                .OrderByDescending(x => x.Signature.Count())
                .FirstOrDefault();
        }

        protected bool TryGetCtorParamter(string name, out string parameterName, out Type parameterType)
        {
            var item = ctor.Signature.FirstOrDefault(x => DefaultMemberNameComparer(x.Key, name));
            parameterName = item.Key;
            parameterType = item.Value;
            return parameterName != null;
        }

        protected bool TryGetMember(string name, out string memberName, out Type memberType)
        {
            memberName = null;
            memberType = null;

            var prop = ProjectedType.GetPropertiesEx().FirstOrDefault(x => DefaultMemberNameComparer(x.Name, name));
            if (prop != null)
            {
                memberName = prop.Name;
                memberType = prop.Type;
            }
            else
            {
                var field = ProjectedType.GetFieldsEx().FirstOrDefault(x => DefaultMemberNameComparer(x.Name, name));
                if (field != null)
                {
                    memberName = field.Name;
                    memberType = field.Type;
                }
            }
            return memberName != null;
        }

        protected MemberExpression GetMemberExpression(string member)
        {
            return MemberExpressionBuilder.MakeDeepMemberAccess(ItParameter, member);
        }

        protected MemberExpression GetMemberExpression(LambdaExpression memberLambdaExpression)
        {
            return LambdaParameterReplacer.Replace(memberLambdaExpression, memberLambdaExpression.Parameters.First(), ItParameter).Body as MemberExpression;
        }

        protected ProjectionBuilderBase Auto1(string member)
        {
            //if (DefaultMemberTarget == DefaultMemberTarget.CtorParameter &&
            //    TryGetCtorParamter(member, out var parameterName, out var parameterType))
            //{

            //}
            //else if (TryGetMember(member, out var memberName, out var memberType))
            //{

            //}
            //else
            //{

            //}
            //var e = ExpressionEx.ConvertIfNeeded(MemberExpressionBuilder.GetPropertySelector(itParameter, memberName), memberType);
            return this;
        }
    }

    

    public class ProjectionBuilder : ProjectionBuilderBase
    {

        public ProjectionBuilder(Type sourceType, Type projectedType)
            : base(sourceType, projectedType)
        {

        }



        public ProjectionBuilder Member(string member, Func<MemberTargetBuilder, MemberTargetBuilder> map)
        {
            var mapTarget = map(new MemberTargetBuilder()).MappedMember;
            mappedMembers.Add(new MemberMap(GetMemberExpression(member), mapTarget));
            return this;
        }

        public ProjectionBuilder CtorParameter(string parameterName, Func<CtorParamTargetBuilder, CtorParamTargetBuilder> map)
        {
            var ctorParameter = new CtorParameterMap(parameterName, map(new CtorParamTargetBuilder()).MappedMember);
            ctorParameters.Add(ctorParameter);
            return this;
        }

        public ProjectionBuilder Auto(string member)
        {
            mappedMembers.Add(new MemberMap(GetMemberExpression(member), null));
            return this;
        }
    }

    public class ProjectionBuilder<TSource> : ProjectionBuilderBase
    {
        public ProjectionBuilder(Type projectedType)
            : base(typeof(TSource), projectedType)
        {
        }

        public ProjectionBuilder<TSource> Member(string member, Func<MemberTargetBuilder<TSource>, MemberTargetBuilder<TSource>> map)
        {
            var mapTarget = map(new MemberTargetBuilder<TSource>()).MappedMember;
            mappedMembers.Add(new MemberMap(GetMemberExpression(member), mapTarget));
            return this;
        }

        public ProjectionBuilder<TSource> CtorParameter(string parameterName, Func<CtorParamTargetBuilder<TSource>, CtorParamTargetBuilder<TSource>> map)
        {
            var ctorParameter = new CtorParameterMap(parameterName, map(new CtorParamTargetBuilder<TSource>()).MappedMember);
            ctorParameters.Add(ctorParameter);
            return this;
        }

        public ProjectionBuilder<TSource> Auto(string member)
        {
            mappedMembers.Add(new MemberMap(GetMemberExpression(member), null));
            return this;
        }
    }

    public class ProjectionBuilder<TSource, TDestination> : ProjectionBuilderBase
    {
        public ProjectionBuilder()
            : base(typeof(TSource), typeof(TDestination))
        {

        }

        public ProjectionBuilder<TSource, TDestination> Member(string member, Func<MemberTargetBuilder<TSource>, MemberTargetBuilder<TSource>> map)
        {
            var mapTarget = map(new MemberTargetBuilder<TSource>()).MappedMember;
            mappedMembers.Add(new MemberMap(GetMemberExpression(member), mapTarget));
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> CtorParameter(string parameterName, Func<CtorParamTargetBuilder<TSource>, CtorParamTargetBuilder<TSource>> map)
        {
            var ctorParameter = new CtorParameterMap(parameterName, map(new CtorParamTargetBuilder<TSource>()).MappedMember);
            ctorParameters.Add(ctorParameter);
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> Member<TMember>(Expression<Func<TDestination, TMember>> memberLambdaExpression, Func<MemberTargetBuilder<TSource>, MemberTargetBuilder<TSource>> map)
        {
            var mapTarget = map(new MemberTargetBuilder<TSource>()).MappedMember;
            mappedMembers.Add(new MemberMap(GetMemberExpression(memberLambdaExpression), mapTarget));
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> Auto(string member)
        {
            mappedMembers.Add(new MemberMap(GetMemberExpression(member), null));
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> Auto<TMember>(Expression<Func<TDestination, TMember>> memberLambdaExpression)
        {
            mappedMembers.Add(new MemberMap(GetMemberExpression(memberLambdaExpression), null));
            return this;
        }
    }

    public class MapTarget
    {

    }

    public class StringMapTarget : MapTarget
    {
        internal string SourceExpression { get; }
        internal StringMapTarget(string sourceExpression)
        {
            SourceExpression = sourceExpression;
        }
    }

    public class ExpressionMapTarget : MapTarget
    {
        internal Expression SourceExpression { get; }
        internal ExpressionMapTarget(Expression sourceExpression)
        {
            SourceExpression = sourceExpression;
        }
    }

    public class LambdaExpressionMapTarget : MapTarget
    {
        internal LambdaExpression SourceExpression { get; }
        internal LambdaExpressionMapTarget(LambdaExpression sourceExpression)
        {
            SourceExpression = sourceExpression;
        }
    }

    public class ConstantMapTarget : MapTarget
    {
        public object Value { get; }
        internal ConstantMapTarget(object value)
        {
            Value = value;
        }
    }

    public class ConstantDictionaryMapTarget : MapTarget
    {
        public Dictionary<object, object> Values { get; }
        public bool HasDefaultValue { get; }
        public object DefaultValue { get; }
        internal ConstantDictionaryMapTarget(Dictionary<object, object> values, bool hasDefaultValue, object defaultValue)
        {
            Values = values;
            DefaultValue = defaultValue;
            HasDefaultValue = hasDefaultValue;
        }
    }


    public static class MapTargetBuilderExtensions
    {
        public static T FromExpression<T>(this T mapTarget, string expression)
            where T : MapTargetBuilder
        {
            mapTarget.MappedMember = new StringMapTarget(expression);
            //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
            return mapTarget;
        }

        public static T FromExpression<T>(this T mapTarget, Expression expression)
            where T : MapTargetBuilder
        {
            mapTarget.MappedMember = new ExpressionMapTarget(expression);
            //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
            return mapTarget;
        }

        public static T FromValue<T>(this T mapTarget, object value)
            where T : MapTargetBuilder
        {
            mapTarget.MappedMember = new ConstantMapTarget(value);
            //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
            return mapTarget;
        }

        public static T FromValueMap<T>(this T mapTarget, Dictionary<object, object> values, object defaultValue)
                where T : MapTargetBuilder
        {
            mapTarget.MappedMember = new ConstantDictionaryMapTarget(values, true, defaultValue);
            //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
            return mapTarget;
        }

        public static T FromValueMap<T>(this T mapTarget, Dictionary<object, object> values)
                where T : MapTargetBuilder
        {
            mapTarget.MappedMember = new ConstantDictionaryMapTarget(values, false, null);
            //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
            return mapTarget;
        }

        //public static T FromExpression<T, TSource>(this T mapTarget, Expression<Func<TSource, object>> expression)
        //    where T : MemberTargetBuilder<TSource>
        //{
        //    mapTarget.MappedMember = new ExpressionMapTarget(expression);
        //    //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
        //    return mapTarget;
        //}

        public static T UsingCtorParameter<T>(this T memberTargetBuilder, string parameterName)
            where T: MemberTargetBuilder
        {
            memberTargetBuilder.ParameterName = parameterName;
            memberTargetBuilder.DefaultMemberTarget = DefaultMemberTarget.CtorParameter;
            return memberTargetBuilder;
        }

        public static T UsingCtorParameter<T>(this T memberTargetBuilder)
            where T : MemberTargetBuilder
        {
            memberTargetBuilder.ParameterName = null;
            memberTargetBuilder.DefaultMemberTarget = DefaultMemberTarget.CtorParameter;
            return memberTargetBuilder;
        }

    }

    public abstract class MapTargetBuilder
    {
        internal MapTarget MappedMember { get; set; }
        internal string ParameterName { get; set; }
        internal DefaultMemberTarget DefaultMemberTarget { get; set; }

        protected MapTargetBuilder(DefaultMemberTarget defaultMemberTarget)
        {
            this.DefaultMemberTarget = defaultMemberTarget;
        }
    }


    public class MemberTargetBuilder : MapTargetBuilder
    {
        public MemberTargetBuilder()
            : base(DefaultMemberTarget.Member)
        {
        }
    }

    public class MemberTargetBuilder<TSource> : MemberTargetBuilder
    {
        public MemberTargetBuilder()
            : base()
        {
        }

        public MemberTargetBuilder<TSource> FromExpression<TExpression>(Expression<Func<TSource, TExpression>> expression)
        {
            MappedMember = new LambdaExpressionMapTarget(expression);
            return this;
        }
    }



    public class CtorParamTargetBuilder : MapTargetBuilder
    {
        public CtorParamTargetBuilder()
            : base(DefaultMemberTarget.CtorParameter)
        {
        }
    }

    public class CtorParamTargetBuilder<TSource> : CtorParamTargetBuilder
    {
        public CtorParamTargetBuilder()
            : base()
        {
        }

        public CtorParamTargetBuilder<TSource> FromExpression<TExpression>(Expression<Func<TSource, TExpression>> expression)
        {
            MappedMember = new LambdaExpressionMapTarget(expression);
            return this;
        }
    }



    class Test
    {
        public void Test1()
        {
            var pb1 = new ProjectionBuilder<Decoder>(typeof(Encoder))
                .CtorParameter("dsda",
                    map => map
                    .FromExpression(x => x.Fallback)
                    )
                .CtorParameter("dsda",
                    map => map.FromExpression(x => x.Fallback))
                //.CtorParameter("dsda", map => map.FromExpression()
                .Member("dsda",
                    map => map
                    .UsingCtorParameter("ds")
                    .FromValue(5)
                    );


            var pb = new ProjectionBuilder<Decoder, Encoder>()
                .Auto("dsda")
                .Auto(x => x.FallbackBuffer)
                .CtorParameter("dsda",
                    map => map.FromExpression(x => x.Fallback))
                .CtorParameter("dsda",
                    map => map.FromExpression(x => x.Fallback))
                //.CtorParameter("dsda", map => map.FromExpression()
                .Member("dsda",
                    map => map.FromValue(5))
               ;
        }
    }


}
