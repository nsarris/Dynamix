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
    public class ConstructorInfoExIgnoreAttribute : Attribute
    {

    }

    [DebuggerDisplay("Name = {MethodInfo.Name}, ReturnType = {MethodInfo.ReturnType}")]
    public class ConstructorInfoEx : IMemberInfoEx
    {
        public string Name => ConstructorInfo.Name;
        public ConstructorInfo ConstructorInfo { get; private set; }
        public IReadOnlyDictionary<string, Type> Signature { get; private set; }
        MemberInfo IMemberInfoEx.MemberInfo => ConstructorInfo;

        MemberInfoExKind IMemberInfoEx.Kind => MemberInfoExKind.Constructor;

        readonly GenericStaticInvoker invoker;

        public ConstructorInfoEx(ConstructorInfo ctor, bool enableDelegateCaching = true)
        {
            ConstructorInfo = ctor;
            Signature = new ReadOnlyDictionary<string,Type>(ctor.GetParameters().ToDictionary(x => x.Name, x => x.ParameterType));

            if (enableDelegateCaching)
                invoker = MemberAccessorDelegateBuilder.ConstructorBuilder.BuildGeneric(ctor);
            else
            {
                var builder = new ConstructorInvokerLambdaBuilder(false);
                invoker = builder.BuildGeneric(ctor).Compile();
            }
        }
        
        public object Invoke(object instance, params object[] arguments)
        {
            return invoker(instance, arguments);
        }

        public static implicit operator ConstructorInfo(ConstructorInfoEx constructorInfoEx)
        {
            return constructorInfoEx.ConstructorInfo;
        }
    }
}
