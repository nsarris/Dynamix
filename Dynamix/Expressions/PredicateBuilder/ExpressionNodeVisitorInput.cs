﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.Expressions.PredicateBuilder
{
    public class ExpressionNodeVisitorInput
    {
        private static readonly string defaultItParameterName = string.Empty;

        public Type ItParameterType  => ItParameterExpression.Type;
        public PredicateBuilderConfiguration DefaultConfiguration { get; }
        public IReadOnlyDictionary<string, PredicateBuilderConfiguration> Configurations { get; }
        public ParameterExpression ItParameterExpression { get; }

        public ExpressionNodeVisitorInput(Type itParameterType)
            :this(itParameterType, defaultItParameterName, null)
        {
            
        }

        public ExpressionNodeVisitorInput(Type itParameterType, PredicateBuilderConfiguration defaultConfiguration)
            : this(itParameterType, defaultItParameterName, defaultConfiguration, null)
        {

        }

        public ExpressionNodeVisitorInput(Type itParameterType, string itParameterName)
            : this(itParameterType, itParameterName, null, null)
        {

        }

        public ExpressionNodeVisitorInput(Type itParameterType, PredicateBuilderConfiguration defaultConfiguration, IDictionary<string, PredicateBuilderConfiguration> configurations)
            :this(itParameterType, defaultItParameterName, defaultConfiguration, configurations)
        {

        }

        public ExpressionNodeVisitorInput(Type itParameterType, string itParameterName, IDictionary<string, PredicateBuilderConfiguration> configurations)
            :this(itParameterType, itParameterName, null, configurations)
        {
         
        }


        public ExpressionNodeVisitorInput(Type itParameterType, string itParameterName, PredicateBuilderConfiguration defaultConfiguration, IDictionary<string, PredicateBuilderConfiguration> configurations = null)
        {
            ItParameterExpression = Expression.Parameter(itParameterType, itParameterName);
            DefaultConfiguration = defaultConfiguration ?? PredicateBuilderConfiguration.Default;

            if (configurations != null)
                Configurations = new ReadOnlyDictionary<string, PredicateBuilderConfiguration>(configurations);
            else
                Configurations = new Dictionary<string, PredicateBuilderConfiguration>();
        }
    }
}
