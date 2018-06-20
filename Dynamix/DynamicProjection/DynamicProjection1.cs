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
            public ProjectionSource Source { get; set; }
            public ProjectionSource SourceKey { get; set; }
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

        //public MemberInfo MemberInfo { get; set; }
        //public Type MemberType { get; set; }
        //public string CtorParameterName { get; set; }
        //public Type CtorParameterType { get; set; }
        //public ProjectionTarget ProjectionTarget { get; set; }
        //public ProjectedMember ProjectedMember { get; set; }
        //public ProjectionSource Source { get; set; }
        //public ProjectionSource SourceKey { get; set; }
        //public ValueMap ValueMap { get; set; }
        ////public ValueMap ReverseValueMap { get; set; }
        //public Expression SourceExpression { get; set; }
    }

    internal class CompiledCtorParamTargetConfiguration
    {
        public string ParameterName { get; set; }
        public Type ParameterType { get; set; }
        public ProjectionSource Source { get; set; }
        public ValueMap ValueMap { get; set; }
        //public ValueMap ReverseValueMap { get; set; }
        public Expression SourceExpression { get; set; }
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
        CompiledDynamicProjectionConfiguration compiledConfiguration;
        
        internal DynamicProjection(DynamicProjectionConfiguration configuration)
        {
            this.configuration = configuration;
            compiler = new DynamicProjectionCompiler(configuration);

            Compile();
        }

        internal void Compile()
        {
            compiledConfiguration = compiler.GetCompiledConfiguration();
        }


        public DynamicQueryable BuildQuery(
            IQueryable queryable,
            IEnumerable<string> selectedColumns = null
            )
        {
            var d = new DynamicQueryable(queryable)
                .Select(compiler.BuildSelector(selectedColumns));

            return d;
        }
    }
}
