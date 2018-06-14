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
            mappedMembers.Add(new MemberMap(GetMemberExpression(member), mapTarget));
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

        public ProjectionBuilder<TSource> Member(string member, Func<MemberTargetBuilder<TSource>, ConfiguredMemberTargetBuilder> map)
        {
            var mapTarget = map(new MemberTargetBuilder<TSource>()).MappedMember;
            mappedMembers.Add(new MemberMap(GetMemberExpression(member), mapTarget));
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

        public ProjectionBuilder<TSource, TDestination> Member(string member, Func<MemberTargetBuilder<TSource>, ConfiguredMemberTargetBuilder> map)
        {
            var mapTarget = map(new MemberTargetBuilder<TSource>()).MappedMember;
            mappedMembers.Add(new MemberMap(GetMemberExpression(member), mapTarget));
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

    #region MapTargets

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

    #endregion

    //public static class MapTargetBuilderExtensions
    //{
    //    public static T FromExpression<T>(this T mapTarget, string expression)
    //        where T : MapTargetBuilder
    //    {
    //        mapTarget.MappedMember = new StringMapTarget(expression);
    //        //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
    //        return mapTarget;
    //    }

    //    public static T FromExpression<T>(this T mapTarget, Expression expression)
    //        where T : MapTargetBuilder
    //    {
    //        mapTarget.MappedMember = new ExpressionMapTarget(expression);
    //        //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
    //        return mapTarget;
    //    }

    //    public static T MapsToKeyExpression<T>(this T mapTarget, string expression)
    //        where T : MapTargetBuilder
    //    {
    //        mapTarget.MappedKey = new StringMapTarget(expression);
    //        //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
    //        return mapTarget;
    //    }

    //    public static T MapsToKeyExpression<T>(this T mapTarget, Expression expression)
    //        where T : MapTargetBuilder
    //    {
    //        mapTarget.MappedKey = new ExpressionMapTarget(expression);
    //        //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
    //        return mapTarget;
    //    }

    //    public static T FromValue<T>(this T mapTarget, object value)
    //        where T : MapTargetBuilder
    //    {
    //        mapTarget.MappedMember = new ConstantMapTarget(value);
    //        //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
    //        return mapTarget;
    //    }

    //    public static T FromValueMap<T>(this T mapTarget, Dictionary<object, object> values, object defaultValue)
    //            where T : MapTargetBuilder
    //    {
    //        mapTarget.MappedMember = new ConstantDictionaryMapTarget(values, true, defaultValue);
    //        //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
    //        return mapTarget;
    //    }

    //    public static T FromValueMap<T>(this T mapTarget, Dictionary<object, object> values)
    //            where T : MapTargetBuilder
    //    {
    //        mapTarget.MappedMember = new ConstantDictionaryMapTarget(values, false, null);
    //        //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
    //        return mapTarget;
    //    }

    //    //public static T FromExpression<T, TSource>(this T mapTarget, Expression<Func<TSource, object>> expression)
    //    //    where T : MemberTargetBuilder<TSource>
    //    //{
    //    //    mapTarget.MappedMember = new ExpressionMapTarget(expression);
    //    //    //var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
    //    //    return mapTarget;
    //    //}

    //    public static T UsingCtorParameter<T>(this T memberTargetBuilder, string parameterName)
    //        where T : MemberTargetBuilder
    //    {
    //        memberTargetBuilder.ParameterName = parameterName;
    //        memberTargetBuilder.DefaultMemberTarget = DefaultMemberTarget.CtorParameter;
    //        return memberTargetBuilder;
    //    }

    //    public static T UsingCtorParameter<T>(this T memberTargetBuilder)
    //        where T : MemberTargetBuilder
    //    {
    //        memberTargetBuilder.ParameterName = null;
    //        memberTargetBuilder.DefaultMemberTarget = DefaultMemberTarget.CtorParameter;
    //        return memberTargetBuilder;
    //    }

    //}

    #region Member Target Builder

    public class MemberTargetBuilder
    {
        internal string ParameterName { get; set; }
        internal MapTarget MappedMember { get; set; }
        internal DefaultMemberTarget DefaultMemberTarget { get; set; } = DefaultMemberTarget.Member;

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
            MappedMember = new StringMapTarget(expression);
            return new ExpressionMemberTargetBuilder();
        }

        public ExpressionMemberTargetBuilder FromExpression(Expression expression)
        {
            MappedMember = new ExpressionMapTarget(expression);
            return new ExpressionMemberTargetBuilder();
        }

        public ConstantMemberTargetBuilder FromValue(object value)
        {
            MappedMember = new ConstantMapTarget(value);
            return new ConstantMemberTargetBuilder();
        }

        public ConstantMemberTargetBuilder FromValueMap(Dictionary<object, object> values, object defaultValue)
        {
            MappedMember = new ConstantDictionaryMapTarget(values, true, defaultValue);
            return new ConstantMemberTargetBuilder();
        }

        public ConstantMemberTargetBuilder FromValueMap(Dictionary<object, object> values)
        {
            MappedMember = new ConstantDictionaryMapTarget(values, false, null);
            return new ConstantMemberTargetBuilder();
        }


    }

    public class ConfiguredMemberTargetBuilder
    {
        internal MapTarget MappedMember { get; set; }
        internal MapTarget MappedKey { get; set; }
    }

    public class ExpressionMemberTargetBuilder : ConfiguredMemberTargetBuilder
    {
        public ExpressionMemberTargetBuilder()
        {
        }

        public ExpressionMemberTargetBuilder MapsToKeyExpression(string expression)
        {
            MappedKey = new StringMapTarget(expression);
            return this;
        }

        public ExpressionMemberTargetBuilder MapsToKeyExpression(Expression expression)
        {
            MappedKey = new ExpressionMapTarget(expression);
            return this;
        }
    }

    public class ConstantMemberTargetBuilder : ConfiguredMemberTargetBuilder
    {
        public ConstantMemberTargetBuilder()
        {
        }
    }

    #endregion

    #region Member Target Builder <TSource>

    public class MemberTargetBuilder<TSource>
    {
        internal string ParameterName { get; set; }
        internal DefaultMemberTarget DefaultMemberTarget { get; set; } = DefaultMemberTarget.Member;

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
            //MappedMember = new StringMapTarget(expression);
            return new ExpressionMemberTargetBuilder<TSource>();
        }

        public ExpressionMemberTargetBuilder<TSource> FromExpression(Expression expression)
        {
            //MappedMember = new ExpressionMapTarget(expression);
            return new ExpressionMemberTargetBuilder<TSource>();
        }

        public ExpressionMemberTargetBuilder<TSource> FromExpression<T>(Expression<Func<TSource, T>> expression)
        {
            //MappedMember = new LambdaExpressionMapTarget(expression);
            return new ExpressionMemberTargetBuilder<TSource>();
        }

        public ConstantMemberTargetBuilder<TSource> FromValue(object value)
        {
            //MappedMember = new ConstantMapTarget(value);
            return new ConstantMemberTargetBuilder<TSource>();
        }

        public ConstantMemberTargetBuilder<TSource> FromValueMap(Dictionary<object, object> values, object defaultValue)
        {
            //MappedMember = new ConstantDictionaryMapTarget(values, true, defaultValue);
            return new ConstantMemberTargetBuilder<TSource>();
        }

        public ConstantMemberTargetBuilder<TSource> FromValueMap(Dictionary<object, object> values)
        {
            //MappedMember = new ConstantDictionaryMapTarget(values, false, null);
            return new ConstantMemberTargetBuilder<TSource>();
        }


    }




    public class ExpressionMemberTargetBuilder<TSource> : ConfiguredMemberTargetBuilder
    {
        
        public ExpressionMemberTargetBuilder()
        {
        }

        public ExpressionMemberTargetBuilder<TSource> MapsToKeyExpression(string expression)
        {
            MappedKey = new StringMapTarget(expression);
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> MapsToKeyExpression(Expression expression)
        {
            MappedKey = new ExpressionMapTarget(expression);
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> MapsToKeyExpression<T>(Expression<Func<TSource, T>> expression)
        {
            MappedKey = new LambdaExpressionMapTarget(expression);
            return this;
        }
    }

    public class ConstantMemberTargetBuilder<TSource> : ConfiguredMemberTargetBuilder
    {
        public ConstantMemberTargetBuilder()
        {
        }
    }

    #endregion


    #region Ctor Target Builder

    public class CtorParamTargetBuilder
    {
        //internal MapTarget MappedMember { get; set; }
        
        public CtorParamTargetBuilder()
        {
        }

        public ExpressionCtorParamTargetBuilder FromExpression(string expression)
        {
            //MappedMember = new StringMapTarget(expression);
            return new ExpressionCtorParamTargetBuilder();
        }

        public ConstantCtorParamTargetBuilder FromExpression(Expression expression)
        {
            //MappedMember = new ExpressionMapTarget(expression);
            return new ConstantCtorParamTargetBuilder();
        }

        public ConstantCtorParamTargetBuilder FromValue(object value)
        {
            //MappedMember = new ConstantMapTarget(value);
            return new ConstantCtorParamTargetBuilder();
        }

        public ConstantCtorParamTargetBuilder FromValueMap(Dictionary<object, object> values, object defaultValue)
        {
            //MappedMember = new ConstantDictionaryMapTarget(values, true, defaultValue);
            return new ConstantCtorParamTargetBuilder();
        }

        public ConstantCtorParamTargetBuilder FromValueMap(Dictionary<object, object> values)
        {
           // MappedMember = new ConstantDictionaryMapTarget(values, false, null);
            return new ConstantCtorParamTargetBuilder();
        }


    }

    public class  ConfiguredCtorParamTargetBuilder
    {
        internal MapTarget MappedMember { get; set; }
        internal MapTarget MappedKey { get; set; }
    }

    public class ExpressionCtorParamTargetBuilder : ConfiguredCtorParamTargetBuilder
    {
        

        public ExpressionCtorParamTargetBuilder()
        {
        }

        public ExpressionCtorParamTargetBuilder MapsToKeyExpression(string expression)
        {
            MappedKey = new StringMapTarget(expression);
            return this;
        }

        public ExpressionCtorParamTargetBuilder MapsToKeyExpression(Expression expression)
        {
            MappedKey = new ExpressionMapTarget(expression);
            return this;
        }
    }

    public class ConstantCtorParamTargetBuilder : ConfiguredCtorParamTargetBuilder
    {
        public ConstantCtorParamTargetBuilder()
        {
        }
    }

    #endregion

    #region Ctor Target Builder <TSource>

    public class CtorParamTargetBuilder<TSource>
    {
        //internal MapTarget MappedMember { get; set; }
        
        public CtorParamTargetBuilder()
        {
        }

        public ExpressionCtorParamTargetBuilder<TSource> FromExpression(string expression)
        {
            //MappedMember = new StringMapTarget(expression);
            return new ExpressionCtorParamTargetBuilder<TSource>();
        }

        public ExpressionCtorParamTargetBuilder<TSource> FromExpression(Expression expression)
        {
            //MappedMember = new ExpressionMapTarget(expression);
            return new ExpressionCtorParamTargetBuilder<TSource>();
        }

        public ExpressionCtorParamTargetBuilder<TSource> FromExpression<T>(Expression<Func<TSource, T>> expression)
        {
            //MappedMember = new LambdaExpressionMapTarget(expression);
            return new ExpressionCtorParamTargetBuilder<TSource>();
        }

        public ConstantCtorParamTargetBuilder<TSource> FromValue(object value)
        {
            //MappedMember = new ConstantMapTarget(value);
            return new ConstantCtorParamTargetBuilder<TSource>();
        }

        public ConstantCtorParamTargetBuilder<TSource> FromValueMap(Dictionary<object, object> values, object defaultValue)
        {
            //MappedMember = new ConstantDictionaryMapTarget(values, true, defaultValue);
            return new ConstantCtorParamTargetBuilder<TSource>();
        }

        public ConstantCtorParamTargetBuilder<TSource> FromValueMap(Dictionary<object, object> values)
        {
            //MappedMember = new ConstantDictionaryMapTarget(values, false, null);
            return new ConstantCtorParamTargetBuilder<TSource>();
        }


    }

    public class ExpressionCtorParamTargetBuilder<TSource> : ConfiguredCtorParamTargetBuilder
    {
        public ExpressionCtorParamTargetBuilder()
        {
        }

        public ExpressionCtorParamTargetBuilder<TSource> MapsToKeyExpression(string expression)
        {
            MappedKey = new StringMapTarget(expression);
            return this;
        }

        public ExpressionCtorParamTargetBuilder<TSource> MapsToKeyExpression(Expression expression)
        {
            MappedKey = new ExpressionMapTarget(expression);
            return this;
        }

        public ExpressionCtorParamTargetBuilder<TSource> MapsToKeyExpression<T>(Expression<Func<TSource, T>> expression)
        {
            MappedKey = new LambdaExpressionMapTarget(expression);
            return this;
        }
    }

    public class ConstantCtorParamTargetBuilder<TSource> : ConfiguredCtorParamTargetBuilder
    {
        public ConstantCtorParamTargetBuilder()
        {
        }
    }

    #endregion



    class Test
    {
        public void Test1()
        {
            var pb1 = new ProjectionBuilder<Decoder>(typeof(Encoder))
                .Auto("dsda")
                .CtorParameter("dsda",
                    map => map
                    .FromExpression(x => x.Fallback)
                    )
                .CtorParameter("dsda",
                    map => map
                    .FromExpression(x => x.Fallback))
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
                    map => map
                    .FromExpression(x => x.Fallback)
                    .MapsToKeyExpression(x => x.Fallback))
                .Member(x => x.Fallback,
                    map => map
                    .UsingCtorParameter("Dsa")
                    .FromExpression(x => x.FallbackBuffer)
                    .MapsToKeyExpression(x => x.Fallback))
                //.CtorParameter("dsda", map => map.FromExpression()
                .Member("dsda",
                    map => map.FromValue(5))
               ;
        }
    }


}
