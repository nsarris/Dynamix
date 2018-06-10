using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.DynamicProjection
{
    class ProjectionBuilder
    {
        public ProjectionBuilder From(string fromExpression, Func<MapTarget, MapTarget> map)
        {
            return this;
        }
    }

    class ProjectionBuilder<TDestination> : ProjectionBuilder
    {
        public ProjectionBuilder From(string fromExpression, Func<MapTarget<TDestination>, MapTarget<TDestination>> map)
        {
            return this;
        }
    }

    class ProjectionBuilder<TSource, TDestination> : ProjectionBuilder<TDestination>
    {
        public ProjectionBuilder<TSource, TDestination> From<TMember>(Expression<Func<TSource, TMember>> from, Func<MapTarget<TDestination>, MapTarget<TDestination>> map)
        {
            return this;
        }
    }

    class MapTarget
    {
        public void Auto()
        {
            //return this;
        }

        public void ToExpression(string toExpression)
        {
            //return this;
        }

        public void ToConstructorParameter(string paramaterName)
        {
            //return this;
        }

        public void ToValue(object value)
        {
            //return this;
        }

        public void ToValueMap(Dictionary<object, object> values, object defaultValue)
        {
           // return this;
        }
    }

    class MapTarget<TDestination> : MapTarget
    {
        public void ToExpression<TMember>(Expression<Func<TDestination, TMember>> to)
        {
            //return this;
        }
    }



    //class MappedMember<TSource, TDestination>
    //{

    //}

    //class MappedMember<TSource, TDestination>
    //{

    //}

}
