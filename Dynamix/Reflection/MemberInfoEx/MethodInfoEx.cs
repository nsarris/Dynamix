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
        readonly Dictionary<string, ParameterInfo> parameters;

        public MethodInfoEx(MethodInfo method, bool enableDelegateCaching = true)
        {
            this.MethodInfo = method ?? throw new ArgumentNullException(nameof(method));

            if (method.ContainsGenericParameters)
                throw new ArgumentException("Dynamix MethodInfoEx wrappers do not support generic parameters");

            this.IsExtension = method.IsExtension();
            parameters = method.GetParameters().Select((parameter, index) => (parameter, index)).ToDictionary(x => x.parameter.Name ?? $"<unnamed_param{x.index}>", x => x.parameter);
            this.Signature = new ReadOnlyCollection<Type>(parameters.Values.Select(x => x.ParameterType).ToList());

            if (enableDelegateCaching)
            {
                instanceInvoker = MemberAccessorDelegateBuilder.CachedMethodBuilder.BuildGenericInstance(method);
                if (method.IsStatic)
                    staticInvoker = MemberAccessorDelegateBuilder.CachedMethodBuilder.BuildGenericStatic(method);
            }
            else
            {
                var builder = new MethodInvokerLambdaBuilder(false);
                instanceInvoker = builder.BuildGenericInstance(method).Compile();
                if (method.IsStatic)
                    staticInvoker = builder.BuildGenericStatic(method).Compile();
            }
        }

        #region Static Invokers

        private void AssertStatic()
        {
            if (!MethodInfo.IsStatic)
                throw new InvalidOperationException("Cannot use InvokeStatic on non static method");
        }

        public object InvokeStatic()
        {
            return InvokeStatic(Constants.EmptyObjectArray);
        }

        public object InvokeStatic(params object[] arguments)
        {
            AssertStatic();

            if (arguments != null && arguments.Count() == 1
                && arguments[0] != null &&
                arguments[0].GetType().Namespace == null)
                return InvokeAnonymousStatic(arguments[0]);
            else
                return staticInvoker(arguments);
        }

        private object InvokeAnonymousStatic(object anonymousTypeArguments, bool defaultValueForMissing = false)
        {
            return InvokeStatic(InvocationHelper.GetInvocationParameters(anonymousTypeArguments), defaultValueForMissing);
        }

        private object InvokeStatic(IEnumerable<(string parameterName, object value)> namedParameters, bool defaultValueForMissing = false)
        {
            AssertStatic();
            var invocationParameters = InvocationHelper.GetInvocationParameters(parameters.Values, namedParameters, defaultValueForMissing);
            return instanceInvoker(invocationParameters);
        }

        public object InvokeStatic(params (string parameterName, object value)[] namedParameters)
        {
            return InvokeStatic(namedParameters.AsEnumerable());
        }

        public object InvokeStatic(IEnumerable<(string parameterName, object value)> namedParameters)
        {
            return InvokeStatic(namedParameters, false);
        }

        public object InvokeStaticWithDefaults(IEnumerable<(string parameterName, object value)> namedParameters)
        {
            return InvokeStatic(namedParameters, true);
        }

        public object InvokeStaticWithDefaults()
        {
            return InvokeStatic(null, true);
        }

        public object InvokeStaticWithDefaults(object anonymousTypeArguments)
        {
            return InvokeAnonymousStatic(anonymousTypeArguments, true);
        }

        #endregion

        #region Instance Invokers

        public object Invoke(object instance)
        {
            return Invoke(instance, Constants.EmptyObjectArray);
        }

        public object Invoke(object instance, params object[] arguments)
        {
            if (instance == null && staticInvoker != null)
                return staticInvoker(arguments);

            if (arguments != null && arguments.Count() == 1
                && arguments[0] != null &&
                arguments[0].GetType().Namespace == null)
                return InvokeAnonymous(instance, arguments[0]);
            else
                return instanceInvoker(instance, arguments);

        }

        private object InvokeAnonymous(object instance, object anonymousTypeArguments, bool defaultValueForMissing = false)
        {
            return Invoke(instance, InvocationHelper.GetInvocationParameters(anonymousTypeArguments), defaultValueForMissing);
        }

        private object Invoke(object instance, IEnumerable<(string parameterName, object value)> namedParameters, bool defaultValueForMissing = false)
        {
            var invocationParameters = InvocationHelper.GetInvocationParameters(parameters.Values.Skip(1), namedParameters, defaultValueForMissing);
            return instanceInvoker(instance, invocationParameters);
        }

        public object Invoke(object instance, IEnumerable<(string parameterName, object value)> namedParameters)
        {
            return Invoke(instance, namedParameters, false);
        }

        public object Invoke(object instance, params (string parameterName, object value)[] namedParameters)
        {
            return Invoke(instance, namedParameters.AsEnumerable());
        }

        public object InvokeWithDefaults(object instance, IEnumerable<(string parameterName, object value)> namedParameters)
        {
            return Invoke(instance, namedParameters, true);
        }

        public object InvokeWithDefaults(object instance)
        {
            return Invoke(instance, null, true);
        }

        public object InvokeWithDefaults(object instance, object anonymousTypeArguments)
        {
            return InvokeAnonymous(instance, anonymousTypeArguments, true);
        }

        #endregion

        public static implicit operator MethodInfo(MethodInfoEx methodInfoEx)
        {
            return methodInfoEx.MethodInfo;
        }

    }
}
