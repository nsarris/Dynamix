using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dynamix.PredicateBuilder
{
    public enum ExpressionOperator
    {
        Contains,
        DoesNotContain,
        StartsWith,
        EndsWith,
        Equals,
        DoesNotEqual,
        
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,

        IsContainedIn,
        IsNotContainedIn,

        IsNull,
        IsEmpty,
        IsNullOrEmpty,

        IsNotNull,
        IsNotEmpty,
        IsNotNullOrEmpty
    }
}