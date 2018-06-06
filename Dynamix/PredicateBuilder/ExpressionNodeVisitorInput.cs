using System;
using System.Linq.Expressions;

namespace Dynamix.PredicateBuilder
{
    public class ExpressionNodeVisitorInput
    {
        private static readonly string defaultItParameterName = string.Empty;

        public Type ItParameterType  => ItParameterExpression.Type;
        public PredicateBuilderConfiguration Configuration { get; }
        public ParameterExpression ItParameterExpression { get; }

        public ExpressionNodeVisitorInput(Type itParameterType)
            :this(itParameterType, defaultItParameterName, null)
        {
            
        }

        public ExpressionNodeVisitorInput(Type itParameterType, PredicateBuilderConfiguration configuration)
            :this(itParameterType, defaultItParameterName, configuration)
        {

        }

        public ExpressionNodeVisitorInput(Type itParameterType, string itParameterName)
            :this(itParameterType, itParameterName, null)
        {
         
        }

        public ExpressionNodeVisitorInput(Type itParameterType, string itParameterName, PredicateBuilderConfiguration configuration)
        {
            ItParameterExpression = Expression.Parameter(itParameterType, itParameterName);
            Configuration = configuration ?? PredicateBuilderConfiguration.Default;
        }
    }
}
