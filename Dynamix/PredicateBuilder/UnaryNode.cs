using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Dynamix.PredicateBuilder
{
    public class UnaryNode : NodeBase
    {
        public string Property { get; set; }
        new public ExpressionOperator Operator { get; set; }
        public override bool HasChildren { get { return false; } }
        public object Value { get; set; }

        public override string GetStringExpression()
        {
            return this.ToString();
        }

        //public override Expression<Func<T,bool>> GetExpression<T>(ParameterExpression input = null)
        //{
        //    //return PredicateBuilder.GetStronglyTypedPredicate<T>(Property, Operator, Value, input); 
        //    return null;
        //}

        //public override LambdaExpression GetLambdaExpression<T>(ParameterExpression input = null)
        //{
        //    //return PredicateBuilder.GetPredicate(typeof(T), Property, Operator, Value, input); 
        //    return null;
        //}

        //public override LambdaExpression GetLambdaExpression(Type Type, ParameterExpression input = null)
        //{
        //    //return PredicateBuilder.GetPredicate(Type, Property, Operator, Value, input);
        //    return null;
        //}



        public override string ToString()
        {
            return Property + " " + Operator.ToString() + " " + Value.ToString();
        }


        public override Expression GetExpression(Type Type)
        {
            return PredicateBuilder.GetPredicateExpression(Type, Property, Operator, Value);
        }
    }
}