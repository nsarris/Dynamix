using Dynamix.Expressions.Builders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions.Builders
{
    public static class MemberAccessorExpressionBuilder
    {
        public static InstanceFieldAccessorBuilder InstanceFieldBuilder { get; private set; } = new InstanceFieldAccessorBuilder();
        public static InstancePropertyAccessorBuilder InstancePropertyBuilder { get; private set; } = new InstancePropertyAccessorBuilder();
        public static StaticFieldAccessorBuilder StaticFieldBuilder { get; private set; } = new StaticFieldAccessorBuilder();
        public static StaticPropertyAccessorBuilder StaticPropertyBuilder { get; private set; } = new StaticPropertyAccessorBuilder();
        public static ConstructorInvokerBuilder ConstructorBuilder { get; private set; } = new ConstructorInvokerBuilder();
        public static MethodInvokerBuilder MethodBuilder { get; private set; } = new MethodInvokerBuilder();
    }
}
