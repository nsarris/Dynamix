using System.Linq.Expressions;

namespace Dynamix.PredicateBuilder
{
    public class ExpressionNodeVisitorInput
    {
        public PredicateBuilderConfiguration Configuration { get; }
        public ParameterExpression ItParameterExpression { get; }
        public ExpressionNodeVisitorInput(ParameterExpression itParameterExpression)
        {
            ItParameterExpression = itParameterExpression;
        }

        public ExpressionNodeVisitorInput(ParameterExpression itParameterExpression, PredicateBuilderConfiguration configuration)
        {
            ItParameterExpression = itParameterExpression;
            Configuration = configuration;
        }
    }
}
