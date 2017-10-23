using Dynamix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.PredicateBuilder
{
    public abstract class NodeBase
    {
        public LogicalOperator Operator { get; set; }
        public abstract bool HasChildren { get; }
        public abstract string GetStringExpression();

        public LambdaExpression GetLambdaExpression(Type Type, ParameterExpression input = null)
        {
            input = input == null ? Expression.Parameter(Type) : input;
            var r = Expression.Lambda(GetExpression(Type), input);
            r = EpxressionParameterReplacer.Replace(r, "x", input);
            return r;
        }

        public Expression<Func<T, bool>> GetLambdaExpression<T>(ParameterExpression input = null)
        {
            input = input == null ? Expression.Parameter(typeof(T)) : input;
            var expr = GetExpression(typeof(T));
            var lamda = Expression.Lambda<Func<T, bool>>(expr, input);

            return EpxressionParameterReplacer.Replace(lamda, "x", input);
        }

        public Expression GetExpression<T>()
        {
            return GetExpression(typeof(T));
        }
        public abstract Expression GetExpression(Type Type);

    }
}
