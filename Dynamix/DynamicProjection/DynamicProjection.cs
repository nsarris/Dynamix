using Dynamix.Expressions.PredicateBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.DynamicProjection
{

    internal class CompiledMemberTargetConfiguration
    {
        internal class CompiledMemberConfiguration
        {
            public MemberInfo MemberInfo { get; set; }
            public Type MemberType { get; set; }
            public string CtorParameterName { get; set; }
            public Type CtorParameterType { get; set; }
            public ProjectionTarget ProjectionTarget { get; set; }
            //public ProjectedMember ProjectedMember { get; set; }
        }

        internal class CompiledSourceConfiguration
        {
            public IProjectionSource Source { get; set; }
            public IProjectionSource SourceKey { get; set; }
            public ValueMap ValueMap { get; set; }
            //public ValueMap ReverseValueMap { get; set; }
            public Expression SourceExpression { get; set; }
        }

        internal CompiledMemberConfiguration Member { get; }
        internal CompiledSourceConfiguration Source { get; }

        public CompiledMemberTargetConfiguration(CompiledMemberConfiguration member, CompiledSourceConfiguration source)
        {
            Member = member;
            Source = source;
        }
    }

    internal class CompiledCtorParamTargetConfiguration
    {
        internal class CompiledCtorParamConfiguration
        {
            public ParameterInfo ParameterInfo { get; set; }
            //public string ParameterName { get; set; }
            //public Type ParameterType { get; set; }
        }

        internal class CompiledSourceConfiguration
        {
            public IProjectionSource Source { get; set; }
            public ValueMap ValueMap { get; set; }
            //public ValueMap ReverseValueMap { get; set; }
            public Expression SourceExpression { get; set; }
        }

        internal CompiledCtorParamConfiguration CtorParameter { get; }
        internal CompiledSourceConfiguration Source { get; }

        public CompiledCtorParamTargetConfiguration(CompiledCtorParamConfiguration ctorParam, CompiledSourceConfiguration source)
        {
            CtorParameter = ctorParam;
            Source = source;
        }
    }

    internal class CompiledValueMap
    {
        public ValueMap ValueMap { get; set; }
        public Dictionary<object, object> ReverseValues { get; set; }
    }


    internal class CtorParameterAssignment
    {
        public string MemberName { get; }
        public Expression SourceExpression { get; }
        public CtorParameterAssignment(string memberName, Expression sourceExpression)
        {
            MemberName = memberName;
            SourceExpression = sourceExpression;
        }
    }

    internal class MemberInitAssignment
    {
        public string MemberName { get; }
        public MemberAssignment MemberAssignement { get; }
        public MemberInitAssignment(string memberName, MemberAssignment memberAssignement)
        {
            MemberName = memberName;
            MemberAssignement = memberAssignement;
        }
    }

    public sealed class DynamicProjection
    {
        readonly DynamicProjectionConfiguration configuration;
        readonly DynamicProjectionCompiler compiler;
        internal CompiledDynamicProjectionConfiguration CompiledConfiguration { get; }

        internal DynamicProjection(DynamicProjectionConfiguration configuration)
        {
            this.configuration = configuration;
            compiler = new DynamicProjectionCompiler(configuration);

            //Compile();
            CompiledConfiguration = compiler.GetCompiledConfiguration();
        }


        public DynamicQueryable BuildQuery(
            IQueryable queryable,
            IEnumerable<string> columns = null,
            NodeBase filter = null,
            IEnumerable<OrderItem> sort = null
            )
        {
            var query = new DynamicQueryable(queryable);

            var predicates = filter != null ? new DynamicProjectionPredicateVisitor(this).VisitLambda(filter, null, null) : null;
            var orderItems = 
                (sort ?? Enumerable.Empty<OrderItem>())
                .Select(x => new
                {
                    SourceExpression = CompiledConfiguration.CompiledMembers.TryGetValue(x.Expression, out var memberMap) 
                                ? Expression.Lambda(memberMap.Source.SourceExpression, CompiledConfiguration.It)
                                : null,
                    OrderItem = x
                })
                .ToList();

            if (predicates?.SourcePredicate != null)
                query = query.Where(predicates.SourcePredicate);

            foreach(var item in orderItems.Where(x => x.SourceExpression != null))
                query = item.OrderItem.IsDescending ? query.OrderByDescending(item.SourceExpression) : query.OrderBy(item.SourceExpression);
                    
            query = query.Select(compiler.BuildSelector(columns));

            if (predicates?.ProjectionPredicate != null)
                query = query.Where(predicates.ProjectionPredicate);

            query = query.OrderBy(orderItems.Where(x => x.SourceExpression == null).Select(x => x.OrderItem));
            
            return query;
        }

        public DynamicQueryable BuildQuery(
            IQueryable queryable,
            IEnumerable<string> columns = null,
            NodeBase filter = null,
            string sort = null
            )
        {
            return BuildQuery(queryable, columns, filter, OrderByExpressionParser.Parse(sort));
        }
    }
}
