using Dynamix.Expressions;
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

    public class ProjectionBuilder
    {
        public Type SourceType { get; }
        public Type ProjectedType { get; }
        public ParameterExpression ItParameter { get; }

        readonly List<MappedMember> mappedMembers = new List<MappedMember>();
        readonly List<CtorParameter> ctorParameters = new List<CtorParameter>();
        

        public ProjectionBuilder(Type sourceType, Type projectedType)
        {
            SourceType = sourceType;
            ProjectedType = projectedType;
            ItParameter = Expression.Parameter(SourceType);
        }
        public ProjectionBuilder Member(string memberExpression, Func<MapTarget, MappedMember> map)
        {
            mappedMembers.Add(map(new MapTarget(ItParameter, Expression.PropertyOrField(ItParameter, memberExpression))));
            return this;
        }

        public ProjectionBuilder CtorParameter(string parameterName, Func<MapTarget, MappedMember> map)
        {
            ctorParameters.Add(new CtorParameter(parameterName, map(new MapTarget(null,null)).SourceExpression));
            return this;
        }
    }

    public class ProjectionBuilder<TSource> //: ProjectionBuilder
    {
        public Type SourceType { get; } = typeof(TSource);
        public Type ProjectedType { get; }

        public ProjectionBuilder(Type projectedType)
        {
            ProjectedType = projectedType;
        }

        public ProjectionBuilder<TSource> Member(string memberExpression, Func<MapTarget<TSource>, MappedMember> map)
        {
            return this;
        }

        public ProjectionBuilder<TSource> CtorParameter(string parameterName, Func<MapTarget<TSource>, MappedMember> map)
        {
            return this;
        }
    }

    public class ProjectionBuilder<TSource, TDestination> //: ProjectionBuilder<TSource>
    {
        public Type SourceType { get; } = typeof(TSource);
        public Type ProjectedType { get; } = typeof(TDestination);

        public ProjectionBuilder()
        {

        }

        public ProjectionBuilder<TSource, TDestination> Member(string memberExpression, Func<MapTarget<TSource>, MappedMember> map)
        {
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> CtorParameter(string parameterName, Func<MapTarget<TSource>, MappedMember> map)
        {
            return this;
        }

        public ProjectionBuilder<TSource, TDestination> Member<TMember>(Expression<Func<TDestination, TMember>> memberExpression, Func<MapTarget<TSource>, MappedMember> map)
        {
            return this;
        }
    }

    public class MappedMember
    {
        public MemberExpression MemberExpression { get; }
        public Expression SourceExpression { get; }
        public MappedMember(MemberExpression memberExpression, Expression sourceExpression)
        {
            MemberExpression = memberExpression;
            SourceExpression = sourceExpression;
        }
    }

    public class MapTarget
    {
        protected readonly ParameterExpression itParameter;
        protected readonly MemberExpression memberExpression;

        public MapTarget(ParameterExpression itParameter, MemberExpression memberExpression)
        {
            this.itParameter = itParameter;
            this.memberExpression = memberExpression;
        }

        public MappedMember Auto()
        {
            var e = ExpressionEx.ConvertIfNeeded(MemberExpressionBuilder.GetPropertySelector(itParameter, memberExpression.Member.Name), memberExpression.Type);
            return new MappedMember(memberExpression, e);
        }

        public MappedMember FromExpression(string expression)
        {
            var e = System.Linq.Dynamic.DynamicExpression.Parse(new[] { itParameter }, memberExpression.Type, expression);
            return new MappedMember(memberExpression, e);
        }

        public MappedMember FromExpression(Expression expression)
        {
            return new MappedMember(memberExpression, expression);
        }

        public MappedMember FromValue(object value)
        {
            var e = ExpressionEx.ConvertIfNeeded(Expression.Constant(value), memberExpression.Type);
            return new MappedMember(memberExpression, e);
        }

        public void FromValueMap(Dictionary<object, object> values, object defaultValue)
        {
            // return this;
        }
    }

    public class MapTarget<TSource> : MapTarget
    {
        public MapTarget(ParameterExpression itParameter, MemberExpression memberExpression)
            : base(itParameter, memberExpression)
        {
        }

        public MappedMember FromExpression<TExpression>(Expression<Func<TSource, TExpression>> expression)
        {
            return new MappedMember(memberExpression, expression);
        }
    }

    class Test
    {
        public void Test1()
        {
            var pb1 = new ProjectionBuilder<Decoder>(typeof(Encoder))
                .CtorParameter("dsda",
                    map => map.FromExpression(x => x.Fallback))
                .CtorParameter("dsda",
                    map => map.FromExpression(x => x.Fallback))
                //.CtorParameter("dsda", map => map.FromExpression()
                .Member("dsda",
                    map => map.FromValue(5));
                

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
