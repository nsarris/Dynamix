using Dynamix.Expressions.LambdaBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{
    public static class MemberAccessorLambdaBuilder
    {
        public static FieldAccessorLambdaBuilder FieldBuilder { get; private set; } = new FieldAccessorLambdaBuilder();
        public static PropertyAccessorLambdaBuilder PropertyBuilder { get; private set; } = new PropertyAccessorLambdaBuilder();
        public static ConstructorInvokerLambdaBuilder ConstructorBuilder { get; private set; } = new ConstructorInvokerLambdaBuilder();
        public static MethodInvokerLambdaBuilder MethodBuilder { get; private set; } = new MethodInvokerLambdaBuilder();
    }
}
