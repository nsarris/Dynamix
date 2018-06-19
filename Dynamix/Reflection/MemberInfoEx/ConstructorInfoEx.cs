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

        readonly Dictionary<string, ParameterInfo> parameters;
        readonly GenericStaticInvoker invoker;

        public ConstructorInfoEx(ConstructorInfo ctor, bool enableDelegateCaching = true)
        {
            ConstructorInfo = ctor;
            parameters = ctor.GetParameters().ToDictionary(x => x.Name);
            Signature = new ReadOnlyDictionary<string,Type>(parameters.ToDictionary(x => x.Key, x => x.Value.ParameterType));

            if (enableDelegateCaching)
                invoker = MemberAccessorDelegateBuilder.CachedConstructorBuilder.BuildGeneric(ctor);
            else
            {
                var builder = new ConstructorInvokerLambdaBuilder(false);
                invoker = builder.BuildGeneric(ctor).Compile();
            }
        }
        
        public object Invoke(params object[] arguments)
        {
            return invoker(arguments);
        }

        public object Invoke(params (string parameterName, object value)[] namedParameters)
        {
            return Invoke(namedParameters.AsEnumerable());
        }

        private object Invoke(IEnumerable<(string parameterName, object value)> namedParameters, bool defaultValueForMissing = false)
        {
            var invocationParameters = InvocationHelper.GetInvocationParameters(parameters.Values, namedParameters, defaultValueForMissing);
            return invoker(invocationParameters);
        }

        public object InvokeWithDefaults(IEnumerable<(string parameterName, object value)> namedParameters)
        {
            return Invoke(namedParameters, true);
        }
        public object InvokeWithDefaults()
        {
            return Invoke(null, true);
        }

        public static implicit operator ConstructorInfo(ConstructorInfoEx constructorInfoEx)
        {
            return constructorInfoEx.ConstructorInfo;
        }
    }
}
