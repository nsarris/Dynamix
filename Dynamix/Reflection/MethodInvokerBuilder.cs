using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Expressions;

namespace Dynamix
{
    public delegate object InstanceInvoker(object instance, params object[] parameters);
    public delegate object StaticInvoker(params object[] parameters);
    public static class MethodInvokerBuilder
    {
        public static StaticInvoker BuildStaticInvoker(MethodInfo MethodInfo)
        {
            if (!MethodInfo.IsStatic)
                throw new ArgumentException("Method is not static", nameof(MethodInfo));

            return (StaticInvoker)BuildInvoker(MethodInfo);
        }

        public static InstanceInvoker BuildInstanceInvoker(MethodInfo MethodInfo)
        {
            if (MethodInfo.IsStatic)
                throw new ArgumentException("Method is not instance", nameof(MethodInfo));

            return (InstanceInvoker)BuildInvoker(MethodInfo);
        }

        public static InstanceInvoker BuildExtensionAsInstanceInvoker(MethodInfo MethodInfo)
        {
            if (!MethodInfo.IsExtension())
                throw new ArgumentException("Method is not an extension", nameof(MethodInfo));

            return (InstanceInvoker)BuildInvoker(MethodInfo, true);
        }


        public static Delegate BuildInvoker(MethodInfo MethodInfo, bool AsExtension = false)
        {
            if (MethodInfo == null)
                throw new ArgumentNullException(nameof(MethodInfo));

            if (AsExtension && !MethodInfo.IsExtension())
                throw new ArgumentException("Method is not an extension method", nameof(MethodInfo));

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var methodCallParameters = Expression.Parameter(typeof(object[]), "parameters");

            var parameterExpressions = MethodInfo.GetParameters()
                .Skip(AsExtension && MethodInfo.IsExtension() ? 1 : 0)
                .Select((x, i) => Expression.Convert(
                            Expression.ArrayAccess(methodCallParameters, Expression.Constant(i)),
                            x.ParameterType));

            Expression invokerExpression;

            //Instance
            if (!MethodInfo.IsStatic)
            {
                invokerExpression = Expression.Call(
                Expression.Convert(instanceParameter, MethodInfo.DeclaringType),
                MethodInfo,
                parameterExpressions);

                var body = (MethodInfo.ReturnType == typeof(void)) ?
                    (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                    ExpressionEx.CastTypeSafe(invokerExpression, typeof(object));

                var invokerLambda = Expression.Lambda<InstanceInvoker>(body, instanceParameter, methodCallParameters);
                return invokerLambda.Compile();
            }
            //Static as Extension
            else if (AsExtension && MethodInfo.IsExtension())
            {
                invokerExpression = Expression.Call(
                    MethodInfo,
                    new Expression[] { Expression.Convert(instanceParameter, MethodInfo.GetParameters()[0].ParameterType) }.Concat(parameterExpressions));
                
                var body = (MethodInfo.ReturnType == typeof(void)) ?
                    (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                    ExpressionEx.CastTypeSafe(invokerExpression, typeof(object));

                var invokerLambda = Expression.Lambda<InstanceInvoker>(body, instanceParameter, methodCallParameters);
                return invokerLambda.Compile();
            }
            //Static
            else
            {
                invokerExpression = Expression.Call(
                MethodInfo,
                parameterExpressions);

                var body = (MethodInfo.ReturnType == typeof(void)) ?
                    (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                    ExpressionEx.CastTypeSafe(invokerExpression, typeof(object));

                var invokerLambda = Expression.Lambda<StaticInvoker>(body, methodCallParameters);
                return invokerLambda.Compile();
            }

        }

        

        private static LambdaExpression BuildTypedInvokerLambdaExpression(MethodInfo MethodInfo)//, Type DelegateType = null)
        {
            if (MethodInfo == null)
                throw new ArgumentNullException(nameof(MethodInfo));

            var methodTypeArguments = (!MethodInfo.IsStatic ? new Type[] { MethodInfo.DeclaringType } : new Type[] { })
                    .Concat(MethodInfo.GetParameters().Select(x => x.ParameterType));

            //if (DelegateType != null)
            //{
            //    var isFunc = DelegateType.IsFunc() && DelegateType.HasGenericArguments(methodTypeArguments.Concat(new[] { MethodInfo.ReturnType }).ToArray());
            //    var isAction = DelegateType.IsAction() && DelegateType.HasGenericArguments(methodTypeArguments.ToArray());

            //    if (!isAction && !isFunc)
            //    {
            //        if (MethodInfo.ReturnType != typeof(void))
            //            throw new ArgumentException("Invoker type must be of Func<Instance,Paremeters1...n,ReturnType>");
            //        else if (MethodInfo.ReturnType == typeof(void))
            //            throw new ArgumentException("Invoker type must be of Action<Instance,Paremeters1...n>");
            //    }
            //}

            Type DelegateType = null;
            if (MethodInfo.ReturnType == typeof(void))
                DelegateType = GenericTypeExtensions.GetActionGenericType(methodTypeArguments);
            else
                DelegateType = GenericTypeExtensions.GetFuncGenericType(methodTypeArguments.Concat(new[] { MethodInfo.ReturnType }).ToArray());

            var methodParameters = MethodInfo.GetParameters().ToList();

            var parameterExpressions = methodParameters
                .Select((x, i) => Expression.Parameter(x.ParameterType))
                .ToList();

            if (!MethodInfo.IsStatic)
            {
                var instanceParameter = Expression.Parameter(MethodInfo.DeclaringType, "instance");

                var invokerExpression = Expression.Call(
                    Expression.Convert(instanceParameter, MethodInfo.DeclaringType),
                    MethodInfo,
                    parameterExpressions);

                if (DelegateType != null)
                    return Expression.Lambda(DelegateType, invokerExpression, new[] { instanceParameter }.Concat(parameterExpressions));
                else
                    return Expression.Lambda(invokerExpression, new[] { instanceParameter }.Concat(parameterExpressions));
            }
            else
            {
                var invokerExpression = Expression.Call(
                    MethodInfo,
                    parameterExpressions);

                if (DelegateType != null)
                    return Expression.Lambda(DelegateType, invokerExpression, parameterExpressions);
                else
                    return Expression.Lambda(invokerExpression, parameterExpressions);
            }
        }

        public static T BuildTypedInvoker<T>(MethodInfo MethodInfo)
        {
            var exp = BuildTypedInvokerLambdaExpression(MethodInfo);
            var castedExp = exp as Expression<T>;
            
            if (castedExp == null)
                throw new ArgumentException("Invalid Delegate type, must be of " + exp.Name);

            return castedExp.Compile();
        }

        public static Delegate BuildTypedInvoker(MethodInfo MethodInfo)
        {
            return BuildTypedInvokerLambdaExpression(MethodInfo).Compile();
        }
    }

}