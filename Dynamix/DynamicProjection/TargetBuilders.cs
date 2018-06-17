using Dynamix.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.DynamicProjection
{

    #region Member Target Builder

    public sealed class MemberTargetBuilder 
    {
        readonly MemberTargetConfiguration configuration;

        internal MemberTargetBuilder(MemberTargetConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public MemberTargetBuilder UsingCtorParameter(string parameterName)
        {
            configuration.CtorParameterName = parameterName;
            configuration.ProjectionTarget = ProjectionTarget.CtorParameter;
            return this;
        }

        public MemberTargetBuilder UsingCtorParameter()
        {
            configuration.CtorParameterName = null;
            configuration.ProjectionTarget = ProjectionTarget.CtorParameter;
            return this;
        }

        public ExpressionMemberTargetBuilder FromExpression(string expression)
        {
            configuration.Source = new StringProjectionSource(expression);
            return new ExpressionMemberTargetBuilder(configuration);
        }

        public ExpressionMemberTargetBuilder FromExpression(Expression expression)
        {
            configuration.Source = new ExpressionProjectionSource(expression);
            return new ExpressionMemberTargetBuilder(configuration);
        }

        public ConstantMemberTargetBuilder FromValue(object value)
        {
            configuration.Source = new ConstantProjectionSource(value);
            return new ConstantMemberTargetBuilder(configuration);
        }
    }

    public class ConfiguredMemberTargetBuilder
    {
        internal MemberTargetConfiguration Configuration { get; }

        internal ConfiguredMemberTargetBuilder(MemberTargetConfiguration configuration)
        {
            Configuration = configuration;
        }
    }

    public class ExpressionMemberTargetBuilder : ConfiguredMemberTargetBuilder
    {
        internal ExpressionMemberTargetBuilder(MemberTargetConfiguration configuration)
            : base(configuration)
        {

        }

        public ExpressionMemberTargetBuilder MapsToKeyExpression(string expression)
        {
            Configuration.SourceKey = new StringProjectionSource(expression);
            return this;
        }

        public ExpressionMemberTargetBuilder MapsToKeyExpression(Expression expression)
        {
            Configuration.SourceKey = new ExpressionProjectionSource(expression);
            return this;
        }

        public ExpressionMemberTargetBuilder HasValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType = UnmappedValueType.TypeDefault, object unmappedValue = null)
        {
            Configuration.ValueMap = new ValueMap(values, unmappedValueType, unmappedValue); 
            return this;
        }
    }

    public class ConstantMemberTargetBuilder : ConfiguredMemberTargetBuilder
    {
        internal ConstantMemberTargetBuilder(MemberTargetConfiguration configuration)
            : base(configuration)
        {
        }
    }

    #endregion

    #region Member Target Builder <TSource>

    public sealed class MemberTargetBuilder<TSource> 
    {
        readonly MemberTargetConfiguration configuration;

        internal MemberTargetBuilder(MemberTargetConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public MemberTargetBuilder<TSource> UsingCtorParameter(string parameterName)
        {
            configuration.CtorParameterName = parameterName;
            configuration.ProjectionTarget = ProjectionTarget.CtorParameter;
            return this;
        }

        public MemberTargetBuilder<TSource> UsingCtorParameter()
        {
            configuration.CtorParameterName = null;
            configuration.ProjectionTarget = ProjectionTarget.CtorParameter;
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> FromExpression(string expression)
        {
            configuration.Source = new StringProjectionSource(expression);
            return new ExpressionMemberTargetBuilder<TSource>(configuration);
        }

        public ExpressionMemberTargetBuilder<TSource> FromExpression(Expression expression)
        {
            configuration.Source = new ExpressionProjectionSource(expression);
            return new ExpressionMemberTargetBuilder<TSource>(configuration);
        }

        public ExpressionMemberTargetBuilder<TSource> FromExpression<T>(Expression<Func<TSource, T>> expression)
        {
            configuration.Source = new LambdaExpressionProjectionSource(expression);
            return new ExpressionMemberTargetBuilder<TSource>(configuration);
        }

        public ConstantMemberTargetBuilder<TSource> FromValue(object value)
        {
            configuration.Source = new ConstantProjectionSource(value);
            return new ConstantMemberTargetBuilder<TSource>(configuration);
        }

    }

    public sealed class ExpressionMemberTargetBuilder<TSource> : ConfiguredMemberTargetBuilder
    {
        internal ExpressionMemberTargetBuilder(MemberTargetConfiguration configuration)
            : base(configuration)
        {

        }

        public ExpressionMemberTargetBuilder<TSource> MapsToKeyExpression(string expression)
        {
            Configuration.SourceKey = new StringProjectionSource(expression);
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> MapsToKeyExpression(Expression expression)
        {
            Configuration.SourceKey = new ExpressionProjectionSource(expression);
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> MapsToKeyExpression<T>(Expression<Func<TSource, T>> expression)
        {
            Configuration.SourceKey = new LambdaExpressionProjectionSource(expression);
            return this;
        }

        public ExpressionMemberTargetBuilder<TSource> HasValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType = UnmappedValueType.TypeDefault, object unmappedValue = null)
        {
            Configuration.ValueMap = new ValueMap(values, unmappedValueType, unmappedValue);
            return this;
        }
    }

    public sealed class ConstantMemberTargetBuilder<TSource> : ConfiguredMemberTargetBuilder
    {
        internal ConstantMemberTargetBuilder(MemberTargetConfiguration configuration)
            : base(configuration)
        {
        }

        internal ConstantMemberTargetBuilder<TSource> ForFutureUse()
        {
            return this;
        }
    }

    #endregion
    #region Ctor Target Builder

    public sealed class CtorParamTargetBuilder 
    {
        readonly CtorParamTargetConfiguration configuration;

        internal CtorParamTargetBuilder(CtorParamTargetConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ExpressionCtorParamTargetBuilder FromExpression(string expression)
        {
            configuration.Source = new StringProjectionSource(expression);
            return new ExpressionCtorParamTargetBuilder(configuration);
        }

        public ExpressionCtorParamTargetBuilder FromExpression(Expression expression)
        {
            configuration.Source = new ExpressionProjectionSource(expression);
            return new ExpressionCtorParamTargetBuilder(configuration);
        }

        public ConstantCtorParamTargetBuilder FromValue(object value)
        {
            configuration.Source = new ConstantProjectionSource(value);
            return new ConstantCtorParamTargetBuilder(configuration);
        }

    }

    public class ConfiguredCtorParamTargetBuilder
    {
        internal CtorParamTargetConfiguration Configuration { get; }
        internal ConfiguredCtorParamTargetBuilder(CtorParamTargetConfiguration configuration)
        {
            Configuration = configuration;
        }
    }

    public sealed class ExpressionCtorParamTargetBuilder : ConfiguredCtorParamTargetBuilder
    {

        internal ExpressionCtorParamTargetBuilder(CtorParamTargetConfiguration configuration)
            : base(configuration)
        {

        }

        public ExpressionCtorParamTargetBuilder HasValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType = UnmappedValueType.TypeDefault, object unmappedValue = null)
        {
            Configuration.ValueMap = new ValueMap(values, unmappedValueType, unmappedValue);
            return this;
        }
    }

    public sealed class ConstantCtorParamTargetBuilder : ConfiguredCtorParamTargetBuilder
    {
        internal ConstantCtorParamTargetBuilder(CtorParamTargetConfiguration configuration)
            : base(configuration)
        {
        }
    }

    #endregion

    #region Ctor Target Builder <TSource>

    public sealed class CtorParamTargetBuilder<TSource> 
    {
        readonly CtorParamTargetConfiguration configuration;

        internal CtorParamTargetBuilder(CtorParamTargetConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ExpressionCtorParamTargetBuilder<TSource> FromExpression(string expression)
        {
            configuration.Source = new StringProjectionSource(expression);
            return new ExpressionCtorParamTargetBuilder<TSource>(configuration);
        }

        public ExpressionCtorParamTargetBuilder<TSource> FromExpression(Expression expression)
        {
            configuration.Source = new ExpressionProjectionSource(expression);
            return new ExpressionCtorParamTargetBuilder<TSource>(configuration);
        }

        public ExpressionCtorParamTargetBuilder<TSource> FromExpression<T>(Expression<Func<TSource, T>> expression)
        {
            configuration.Source = new LambdaExpressionProjectionSource(expression);
            return new ExpressionCtorParamTargetBuilder<TSource>(configuration);
        }

        public ConstantCtorParamTargetBuilder<TSource> FromValue(object value)
        {
            configuration.Source = new ConstantProjectionSource(value);
            return new ConstantCtorParamTargetBuilder<TSource>(configuration);
        }
    }

    public sealed class ExpressionCtorParamTargetBuilder<TSource> : ConfiguredCtorParamTargetBuilder
    {
        
        internal ExpressionCtorParamTargetBuilder(CtorParamTargetConfiguration configuration)
            : base(configuration)
        {
        }

        
        public ExpressionCtorParamTargetBuilder<TSource> HasValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType = UnmappedValueType.TypeDefault, object unmappedValue = null)
        {
            Configuration.ValueMap = new ValueMap(values, unmappedValueType, unmappedValue);
            return this;
        }
    }

    public sealed class ConstantCtorParamTargetBuilder<TSource> : ConfiguredCtorParamTargetBuilder
    {
        internal ConstantCtorParamTargetBuilder(CtorParamTargetConfiguration configuration)
            : base(configuration)
        {
        }

        internal ConstantCtorParamTargetBuilder<TSource> ForFutureUse()
        {
            return this;
        }
    }

    #endregion





}
