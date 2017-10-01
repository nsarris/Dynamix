using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Dynamix.PredicateBuilder
{
    public class ComplexNode : NodeBase
    {
        public List<NodeBase> Nodes { get; set; }

        public override string GetStringExpression()
        {
            string ret = string.Empty;
            foreach (var node in Nodes)
                ret += " (" + node.GetStringExpression() + " " + node.Operator.ToString() + ") ";
            return ret;
        }

        //public override Expression<Func<T, bool>> GetExpression<T>(ParameterExpression input = null)
        //{
        //    return (Expression<Func<T, bool>>)GetLambdaExpression(typeof(T), input);
        //}

        //public override LambdaExpression GetLambdaExpression<T>(ParameterExpression input = null)
        //{
        //    return GetLambdaExpression(typeof(T), input);
        //}

        public override Expression GetExpression(Type Type)
        {
            Expression e = null;
            LogicalOperator nextOp;

            foreach (var node in Nodes)
            {
                if (e == null)
                {
                    e = node.GetExpression(Type);
                }
                else
                {
                    switch (node.Operator)
                    {
                        case LogicalOperator.And:
                            e = Expression.AndAlso(e, node.GetExpression(Type));
                            break;
                        case LogicalOperator.Or:
                            e = Expression.OrElse(e, node.GetExpression(Type));
                            break;
                        case LogicalOperator.End:
                            e = Expression.AndAlso(e, node.GetExpression(Type));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                nextOp = node.Operator;
            }

            return e;
        }
    }
}