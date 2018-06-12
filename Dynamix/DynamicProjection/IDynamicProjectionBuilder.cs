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

    //public static class ProjectionBuilderExtensions
    //{
    //    public static T Member<T>(this T projectionBuilder, string memberExpression, Func<MapTarget, MappedMember> map)
    //        where T : ProjectionBuilder
    //    {
    //        return projectionBuilder;
    //    }

    //    public static T CtorParameter<T>(this T projectionBuilder, string parameterName, Func<MapTarget, MappedMember> map)
    //        where T : ProjectionBuilder
    //    {
    //        return projectionBuilder;
    //    }

    //    public static T Member<T, TSource>(this T projectionBuilder, string memberExpression, Func<MapTarget<TSource>, MappedMember> map)
    //        where T : ProjectionBuilder<TSource>
    //    {
    //        return projectionBuilder;
    //    }

    //    public static T CtorParameter<T, TSource>(this T projectionBuilder, string parameterName, Func<MapTarget<TSource>, MappedMember> map)
    //        where T : ProjectionBuilder<TSource>
    //    {
    //        return projectionBuilder;
    //    }

    //    //public static ProjectionBuilder<TSource, TDestination> Member<TSource, TDestination>(this ProjectionBuilder<TSource, TDestination> projectionBuilder, Expression<Func<TDestination, object>> memberExpression, Func<MapTarget<TSource>, MappedMember> map)
    //    //{
    //    //    return projectionBuilder;
    //    //}

    //    public static T Member1<T , TSource>
    //        (this T projectionBuilder, TSource x)
    //        where T : List<TSource>
    //    {
    //        return projectionBuilder;
    //    }
    //}

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

    public class CtorParameter
    {
        public string ParameterName { get; }
        public Expression SourceExpression { get; }
        public CtorParameter(string parameterName, Expression sourceExpression)
        {
            SourceExpression = sourceExpression;
            ParameterName = parameterName;
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

        protected readonly List<MappedMember> mappedMembers = new List<MappedMember>();
        protected readonly List<CtorParameter> ctorParameters = new List<CtorParameter>();
        protected readonly ConstructorInfoEx ctor;

        public ProjectionBuilderBase(Type sourceType, Type projectedType)
        {
            SourceType = sourceType;
            ProjectedType = projectedType;
            ItParameter = Expression.Parameter(SourceType);

            ctor = projectedType.GetConstructorsEx()
                .OrderByDescending(x => x.Signature.Count())
                .FirstOrDefault();
        }
    }

    public class ProjectionBuilder : ProjectionBuilderBase
    {
        
        public ProjectionBuilder(Type sourceType, Type projectedType)
            :base(sourceType, projectedType)
        {
            
        }
        public ProjectionBuilder Member(string member, Func<MapMemberTarget, MappedMember> map)
        {
            //Deep member access
            var memberExpression = MemberExpressionBuilder.MakeDeepMemberAccess(ItParameter, member);
            mappedMembers.Add(map(new MapMemberTarget(ItParameter, memberExpression.Member.Name, memberExpression.Type)));
            return this;
        }

        public ProjectionBuilder CtorParameter(string parameterName, Func<MapCtorTarget, MappedMember> map)
        {
            var parameter = ctor.ConstructorInfo.GetParameters().Where(x => x.Name == parameterName).Single();
            ctorParameters.Add(new CtorParameter(parameterName, map(new MapCtorTarget(ItParameter, parameterName, parameter.ParameterType)).SourceExpression));
            return this;
        }
    }

    public class ProjectionBuilder<TSource> : ProjectionBuilderBase
    {
        public ProjectionBuilder(Type projectedType)
            :base(typeof(TSource), projectedType)
        {
        }

        public ProjectionBuilder<TSource> Member(string member, Func<MapMemberTarget<TSource>, MappedMember> map)
        {
            //Deep member access
            var memberExpression = MemberExpressionBuilder.MakeDeepMemberAccess(ItParameter, member);
            mappedMembers.Add(map(new MapMemberTarget<TSource>(ItParameter, memberExpression.Member.Name, memberExpression.Type)));
            return this;
        }

        public ProjectionBuilder<TSource> CtorParameter(string parameterName, Func<MapCtorTarget<TSource>, MappedMember> map)
        {
            var parameter = ctor.ConstructorInfo.GetParameters().Where(x => x.Name == parameterName).Single();
            ctorParameters.Add(new CtorParameter(parameterName, map(new MapCtorTarget<TSource>(ItParameter, parameterName, parameter.ParameterType)).SourceExpression));
            return this;
        }
    }

    public class ProjectionBuilder<TSource, TDestination> : ProjectionBuilderBase
    {
        public ProjectionBuilder()
            :base(typeof(TSource), typeof(TDestination))
        {

        }

        public ProjectionBuilder<TSource, TDestination> Member(string member, Func<MapMemberTarget<TSource>, MappedMember> map)
        {
            //Deep member access
            var memberExpression = MemberExpressionBuilder.MakeDeepMemberAccess(ItParameter, member);
            mappedMembers.Add(map(new MapMemberTarget<TSource>(ItParameter, memberExpression.Member.Name, memberExpression.Type)));
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> CtorParameter(string parameterName, Func<MapCtorTarget<TSource>, MappedMember> map)
        {
            var parameter = ctor.ConstructorInfo.GetParameters().Where(x => x.Name == parameterName).Single();
            ctorParameters.Add(new CtorParameter(parameterName, map(new MapCtorTarget<TSource>(ItParameter, parameterName, parameter.ParameterType)).SourceExpression));
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> Member<TMember>(Expression<Func<TDestination, TMember>> memberLambdaExpression, Func<MapMemberTarget<TSource>, MappedMember> map)
        {
            var memberExpression = LambdaParameterReplacer.Replace(memberLambdaExpression, memberLambdaExpression.Parameters.First(), ItParameter).Body as MemberExpression;
            mappedMembers.Add(map(new MapMemberTarget<TSource>(ItParameter, memberExpression.Member.Name, memberExpression.Type)));
            return this;
        }
    }

    public class MappedMember
    {
        //public MemberExpression MemberExpression { get; }
        public Expression SourceExpression { get; }
        public MappedMember(
            //MemberExpression memberExpression, 
            Expression sourceExpression)
        {
            //MemberExpression = memberExpression;
            SourceExpression = sourceExpression;
        }
    }

    //public class MappedCtorParameter
    //{
    //    public Expression SourceExpression { get; }
    //    public MappedCtorParameter(Expression sourceExpression)
    //    {
    //        SourceExpression = sourceExpression;
    //    }
    //}

    public abstract class MapTarget
    {
        protected readonly ParameterExpression itParameter;
        protected readonly string memberName;
        protected readonly Type memberType;
        protected DefaultMemberTarget defaultMemberTarget;

        public MapTarget(ParameterExpression itParameter, string memberName, Type memberType, DefaultMemberTarget defaultMemberTarget)
        {
            this.itParameter = itParameter;
            this.memberName = memberName;
            this.memberType = memberType;
            this.defaultMemberTarget = defaultMemberTarget;
        }

        public MappedMember Auto()
        {
            var e = ExpressionEx.ConvertIfNeeded(MemberExpressionBuilder.GetPropertySelector(itParameter, memberName), memberType);
            return new MappedMember(e);
        }

        public MappedMember FromExpression(string expression)
        {
            var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberType, expression);
            return new MappedMember(e);
        }

        public MappedMember FromExpression(Expression expression)
        {
            return new MappedMember(expression);
        }

        public MappedMember FromValue(object value)
        {
            var e = ExpressionEx.ConvertIfNeeded(Expression.Constant(value), memberType);
            return new MappedMember(e);
        }

        public void FromValueMap(Dictionary<object, object> values, object defaultValue)
        {
            // return this;
        }
    }

    public abstract class MapTarget<TSource> : MapTarget
    {
        
        public MapTarget(ParameterExpression itParameter, string memberName, Type memberType, DefaultMemberTarget defaultMemberTarget)
            :base(itParameter, memberName, memberType, defaultMemberTarget)
        {
            
        }

        public MappedMember FromExpression<TExpression>(Expression<Func<TSource, TExpression>> expression)
        {
            return new MappedMember(expression);
        }
    }

    public class MapMemberTarget : MapTarget
    {
        internal string ParameterName { get; set; }

        public MapMemberTarget(ParameterExpression itParameter, string memberName, Type memberType) 
            : base(itParameter, memberName, memberType, DefaultMemberTarget.Member)
        {
        }

        public MapMemberTarget UsingCtorParameter(string parameterName)
        {
            ParameterName = parameterName;
            defaultMemberTarget = DefaultMemberTarget.CtorParameter;
            return this;
        }

        public MapMemberTarget UsingCtorParameter()
        {
            ParameterName = memberName;
            return this;
        }
    }

    public class MapMemberTarget<TSource> : MapMemberTarget
    {
        public MapMemberTarget(ParameterExpression itParameter, string memberName, Type memberType) 
            : base(itParameter, memberName, memberType)
        {
        }

        public MappedMember FromExpression<TExpression>(Expression<Func<TSource, TExpression>> expression)
        {
            return new MappedMember(expression);
        }
    }

    public class MapCtorTarget : MapTarget
    {
        public MapCtorTarget(ParameterExpression itParameter, string memberName, Type memberType) 
            : base(itParameter, memberName, memberType, DefaultMemberTarget.CtorParameter)
        {
        }
    }

    public class MapCtorTarget<TSource> : MapCtorTarget
    {
        public MapCtorTarget(ParameterExpression itParameter, string memberName, Type memberType) 
            : base(itParameter, memberName, memberType)
        {
        }

        public MappedMember FromExpression<TExpression>(Expression<Func<TSource, TExpression>> expression)
        {
            return new MappedMember(expression);
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
                .CtorParameter("dsda",
                    map => map.FromExpression(x => x.Fallback))
                .CtorParameter("dsda",
                    map => map.FromExpression(x => x.Fallback))
                //.CtorParameter("dsda", map => map.FromExpression()
                .Member("dsda",
                    map => map.FromValue(5))
                .Member(x => x.Fallback,
                    map => map.Auto());
        }
    }


}
