using Dynamix.Expressions.LambdaBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Reflection.DelegateBuilders;

namespace Dynamix.Reflection
{
    public static class MemberAccessorDelegateBuilder
    {
        public static FieldAccessorDelegateBuilder FieldBuilder { get; private set; } = new FieldAccessorDelegateBuilder();
        public static PropertyAccessorDelegateBuilder PropertyBuilder { get; private set; } = new PropertyAccessorDelegateBuilder();
        public static ConstructorInvokerDelegateBuilder ConstructorBuilder { get; private set; } = new ConstructorInvokerDelegateBuilder();
        public static MethodInvokerDelegateBuilder MethodBuilder { get; private set; } = new MethodInvokerDelegateBuilder();
    }
}
