using Dynamix.Expressions.LambdaBuilders;
using Dynamix.Reflection.DelegateBuilders;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public class MethodInfoExIgnoreAttribute : Attribute
    {

    }

    [DebuggerDisplay("Name = {MethodInfo.Name}, ReturnType = {MethodInfo.ReturnType}")]
    public class MethodInfoEx : IMemberInfoEx
    {
        public string Name => MethodInfo.Name;
        public MethodInfo MethodInfo { get; private set; }
        public bool IsExtension { get; private set; }
        public IEnumerable<Type> Signature { get; private set; }
        MemberInfo IMemberInfoEx.MemberInfo => MethodInfo;

        MemberInfoExKind IMemberInfoEx.Kind => MemberInfoExKind.Method;

        readonly GenericInstanceInvoker instanceInvoker;
        readonly GenericStaticInvoker staticInvoker;

        public MethodInfoEx(MethodInfo method, bool enableDelegateCaching = true)
        {
            this.MethodInfo = method ?? throw new ArgumentNullException(nameof(method));
            this.IsExtension = method.IsExtension();
            this.Signature = new ReadOnlyCollection<Type>(method.GetParameters().Select(x => x.ParameterType).ToList());

            if (enableDelegateCaching)
            {
                instanceInvoker = MemberAccessorDelegateBuilder.MethodBuilder.BuildGenericInstance(method);
                if (method.IsStatic)
                    staticInvoker = MemberAccessorDelegateBuilder.MethodBuilder.BuildGenericStatic(method);
            }
            else
            {
                var builder = new MethodInvokerLambdaBuilder(false);
                instanceInvoker = builder.BuildGenericInstance(method).Compile();
                if (method.IsStatic)
                    staticInvoker = builder.BuildGenericStatic(method).Compile();
                    
            }
        }

        public object InvokeStatic(params object[] arguments)
        {
            if (!MethodInfo.IsStatic)
                throw new InvalidOperationException("Cannot use InvokeStatic on non static method");

            return staticInvoker(arguments);
        }

        public object Invoke(object instance, params object[] arguments)
        {
            return instanceInvoker(instance, arguments);
        }

        public static implicit operator MethodInfo(MethodInfoEx methodInfoEx)
        {
            return methodInfoEx.MethodInfo;
        }

    }
}
