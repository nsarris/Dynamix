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

            CompiledConfiguration = compiler.GetCompiledConfiguration();
        }

        public DynamicQueryable BuildQuery(
            IQueryable queryable,
            IEnumerable<string> columns = null,
            NodeBase filter = null)
        {
            return BuildQuery(queryable, columns, filter, (IEnumerable<OrderItem>)null);
        }

        public DynamicQueryable BuildQuery(
            IQueryable queryable,
            IEnumerable<string> columns,
            NodeBase filter,
            IEnumerable<OrderItem> sort
            )
        {
            var query = new DynamicQueryable(queryable);

            //Get all predicate expressions 
            var predicateMembers =
                    filter != null ?
                    new DynamicProjectionPredicateMemberVisitor().Visit(filter) :
                    new List<string>();

            //Determine target of predicate (source or projection)
            var predicateTarget =
                predicateMembers.All(x => this.CompiledConfiguration.CompiledMembers.ContainsKey(x)) ?
                DynamicProjectionOperationTarget.Source : DynamicProjectionOperationTarget.Projection;

            //Build predicate
            var predicate = 
                filter != null ? 
                    predicateTarget == DynamicProjectionOperationTarget.Source ?
                        new DynamicProjectionPredicateVisitor(this)
                            .VisitLambda(filter) :
                        new ExpressionNodeVisitor()
                            .VisitLambda(filter, configuration.ProjectedType) :
                null;

            //Determine target of sort (source or projection)
            var sortTarget =
                (sort ?? Enumerable.Empty<OrderItem>())
                .All(x => CompiledConfiguration.CompiledMembers.ContainsKey(x.Expression)) ?
                DynamicProjectionOperationTarget.Source : DynamicProjectionOperationTarget.Projection;

            //Apply predicate on source if applicable
            if (predicate != null && predicateTarget == DynamicProjectionOperationTarget.Source)
                query = query.Where(predicate);

            //Apply sort on source if applicable
            if (sort != null && sortTarget == DynamicProjectionOperationTarget.Source)
                query = sort.Aggregate(query, (acc, next) =>
                    {
                        var lambda = 
                            Expression.Lambda(
                                CompiledConfiguration.CompiledMembers[next.Expression]
                                    .Source.SourceExpression, 
                                CompiledConfiguration.It);

                        return next.IsDescending ?
                            query.OrderByDescending(lambda) :
                            query.OrderBy(lambda);
                    });

            //Append predicate and sort columns to projection if needed
            var selectorColumns = columns;
            if (predicateTarget == DynamicProjectionOperationTarget.Projection)
                selectorColumns = columns.Concat(predicateMembers);
            if (sortTarget == DynamicProjectionOperationTarget.Projection)
                selectorColumns = columns.Concat(sort.Select(x => x.Expression));
            selectorColumns = selectorColumns.Distinct().ToList();

            //Apply selector
            query = query.Select(compiler.BuildSelector(selectorColumns));

            //Apply predicate on projection if applicable
            if (predicate != null && predicateTarget == DynamicProjectionOperationTarget.Projection)
                query = query.Where(predicate);

            //Apply sort on projection if applicable
            if (sort != null && predicateTarget == DynamicProjectionOperationTarget.Projection)
                query.OrderBy(sort);

            return query;
        }

        public DynamicQueryable BuildQuery(
            IQueryable queryable,
            IEnumerable<string> columns,
            NodeBase filter,
            string sort
            )
        {
            return BuildQuery(queryable, columns, filter, OrderByExpressionParser.Parse(sort));
        }
    }
}
