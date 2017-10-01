using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions.Builders
{
    public delegate object GenericInstanceInvoker(object instance, params object[] parameters);
    public delegate object GenericStaticInvoker(params object[] parameters);

    public class MethodInvokerBuilder
    {
        //public MethodInvokerBuilder()
        //{
        //    if (typeof(T).IsPrimitive)
        //        throw new InvalidOperationException(nameof(MethodInvokerBuilder<T>) + " cannot be ussed with primitive types");
        //}
        public LambdaExpression BuildGenericStatic(MethodInfo MethodInfo)
        {
            if (!MethodInfo.IsStatic)
                throw new ArgumentException("Method is not static", nameof(MethodInfo));

            return BuildGenericInvoker(MethodInfo);
        }

        public LambdaExpression BuildGenericInstance(MethodInfo MethodInfo)
        {
            if (MethodInfo.IsStatic)
                throw new ArgumentException("Method is not instance", nameof(MethodInfo));

            return BuildGenericInvoker(MethodInfo);
        }

        public LambdaExpression BuildGenericExtension(MethodInfo MethodInfo)
        {
            if (!MethodInfo.IsExtension())
                throw new ArgumentException("Method is not an extension", nameof(MethodInfo));

            return BuildGenericInvoker(MethodInfo, true);
        }


        private LambdaExpression BuildGenericInvoker(MethodInfo methodInfo, bool asExtension = false)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (asExtension && !methodInfo.IsExtension())
                throw new ArgumentException("Method is not an extension method", nameof(methodInfo));

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var inputParameters = Expression.Parameter(typeof(object[]), "parameters");

            var methodParameters = methodInfo.GetParameters();
            var methodCallParameters = methodParameters
                .Skip(asExtension && methodInfo.IsExtension() ? 1 : 0)
                .Select((x, i) => ExpressionEx.ConvertIfNeeded(
                            Expression.ArrayAccess(inputParameters, Expression.Constant(i)),
                            x.ParameterType));

            Expression invokerExpression;
            ParameterExpression[] expressionParameters;
            Type delegateType;

            //Instance
            if (!methodInfo.IsStatic)
            {
                invokerExpression = Expression.Call(
                ExpressionEx.ConvertIfNeeded(instanceParameter, methodInfo.DeclaringType),
                methodInfo,
                methodCallParameters);

                expressionParameters = new[] { instanceParameter, inputParameters };
                delegateType = typeof(GenericInstanceInvoker);
            }
            //Static as Extension
            else if (asExtension && methodInfo.IsExtension())
            {
                invokerExpression = Expression.Call(
                    methodInfo,
                    new Expression[] { ExpressionEx.ConvertIfNeeded(instanceParameter, methodParameters[0].ParameterType) }.Concat(methodCallParameters));

                expressionParameters = new[] { instanceParameter, inputParameters };
                delegateType = typeof(GenericInstanceInvoker);
            }
            //Static
            else
            {
                invokerExpression = Expression.Call(
                methodInfo,
                methodCallParameters);

                expressionParameters = new[] { inputParameters };
                delegateType = typeof(GenericStaticInvoker);
            }

            var body = (methodInfo.ReturnType == typeof(void)) ?
                    (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                    ExpressionEx.CastTypeSafe(invokerExpression, typeof(object));

            return Expression.Lambda(delegateType, body, expressionParameters);
        }

        private class MethodParameter
        {
            public Type MethodType { get; set; }
            public ParameterExpression InputParameter { get; set; }
        }
        
        private LambdaExpression BuildInvokerExpressionFromTypes(MethodInfo methodInfo, Type instanceType, IEnumerable<Type> parameterTypes, Type returnType, Type delegateType)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            returnType = returnType ?? methodInfo.ReturnType;
            instanceType = instanceType ?? methodInfo.DeclaringType;

            var methodParameters =
                    (methodInfo.IsStatic ?
                        Enumerable.Empty<MethodParameter>() :
                        new[] { new MethodParameter { InputParameter = Expression.Parameter(instanceType, "instance"), MethodType = methodInfo.DeclaringType } })
                    .Concat(methodInfo.GetParameters().ZipOutter(parameterTypes ?? Enumerable.Empty<Type>(),
                        (mp, pt) => new MethodParameter
                        {
                            InputParameter = Expression.Parameter(pt ?? mp.ParameterType, mp.Name),
                            MethodType = mp.ParameterType
                        }))
                        .Select(x => new
                        {
                            x.InputParameter,
                            MethodCallParameter = x.MethodType == x.InputParameter.Type ? (Expression)x.InputParameter : ExpressionEx.ConvertIfNeeded(x.InputParameter, x.MethodType)
                        })
                        .ToList();

            var isAction = methodInfo.ReturnType == typeof(void) && (returnType == null || returnType == typeof(void));

            delegateType = delegateType ?? (isAction ?
                    GenericTypeExtensions.GetActionGenericType(methodParameters.Select(x => x.MethodCallParameter.Type)) :
                    GenericTypeExtensions.GetFuncGenericType(methodParameters.Select(x => x.MethodCallParameter.Type).Concat(new[] { returnType }).ToArray()));

            var invokerExpression = Expression.Call(
                methodInfo.IsStatic ? null : methodParameters.Select(x => x.MethodCallParameter).First(),
                methodInfo,
                methodParameters.Select(x => x.MethodCallParameter).Skip(methodInfo.IsStatic ? 0 : 1));

            var body = methodInfo.ReturnType == typeof(void) ?
                        (!isAction ?
                        (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                        invokerExpression) :
                    Expression.Convert(invokerExpression, returnType);

            return Expression.Lambda(delegateType, body, methodParameters.Select(x => x.InputParameter));
        }

        public LambdaExpression BuildFromTypes(MethodInfo methodInfo, Type instanceType = null, IEnumerable<Type> parameterTypes = null, Type returnType = null)
        {
            return BuildInvokerExpressionFromTypes(methodInfo, instanceType, parameterTypes, returnType, null);
        }

        public LambdaExpression BuildFromDelegate(MethodInfo methodInfo, Type delegateType)
        {
            if (delegateType == null)
                return BuildFromTypes(methodInfo);

            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            var instanceType = methodInfo.DeclaringType;
            var parameterTypes = methodInfo.GetParameters().Select(x => x.ParameterType);
            Type returnType = methodInfo.ReturnType;

            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException(delegateType.Name + " is not a delegate type");

            var invokeMethod = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            if (invokeMethod == null)
                throw new ArgumentException(delegateType.Name + " delegate type has no invoke method.");

            var invokeMethodParameters = invokeMethod.GetParameters();

            instanceType = methodInfo.IsStatic ? null : invokeMethodParameters.Select(x => x.ParameterType).FirstOrDefault();
            parameterTypes = invokeMethodParameters.Skip(methodInfo.IsStatic ? 0 : 1).Select(x => x.ParameterType);

            return BuildInvokerExpressionFromTypes(methodInfo, instanceType, parameterTypes, returnType, delegateType);
        }

        public Expression<TDelegate> BuildFromDelegate<TDelegate>(MethodInfo methodInfo)
        {
            return (Expression<TDelegate>)BuildFromDelegate(methodInfo, typeof(TDelegate));
        }

        /// INSTANCE ///
        //No parameters

        public Expression<Action<T>> BuildActionInstance<T>(MethodInfo methodInfo)
        {
            return (Expression<Action<T>>)BuildFromTypes(methodInfo, typeof(T), null, typeof(void));
        }
        public Expression<Func<T, TResult>> BuildFuncInstance<T, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<T, TResult>>)BuildFromTypes(methodInfo, typeof(T), null, typeof(TResult));
        }

        //One parameter
        public Expression<Action<T, TParam1>> BuildActionInstance<T, TParam1>(MethodInfo methodInfo)
        {
            return (Expression<Action<T, TParam1>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1) }, typeof(void));
        }
        public Expression<Func<T, TParam1, TResult>> BuildFuncInstance<T, TParam1, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<T, TParam1, TResult>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1) }, typeof(TResult));
        }

        //Two parameters
        public Expression<Action<T, TParam1, TParam2>> BuildActionInstance<T, TParam1, TParam2>(MethodInfo methodInfo)
        {
            return (Expression<Action<T, TParam1, TParam2>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2) }, typeof(void));
        }
        public Expression<Func<T, TParam1, TParam2, TResult>> BuildFuncInstance<T, TParam1, TParam2, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<T, TParam1, TParam2, TResult>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2) }, typeof(TResult));
        }

        //Three parameters
        public Expression<Action<T, TParam1, TParam2, TParam3>> BuildActionInstance<T, TParam1, TParam2, TParam3>(MethodInfo methodInfo)
        {
            return (Expression<Action<T, TParam1, TParam2, TParam3>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(void));
        }
        public Expression<Func<T, TParam1, TParam2, TParam3, TResult>> BuildFuncInstance<T, TParam1, TParam2, TParam3, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<T, TParam1, TParam2, TParam3, TResult>>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(TResult));
        }


        // STATIC ///
        //No parameters

        public Expression<Action> BuildActionStatic(MethodInfo methodInfo)
        {
            return (Expression<Action>)BuildFromTypes(methodInfo, null, null, typeof(void));
        }
        public Expression<Func<TResult>> BuildFuncStatic<TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TResult>>)BuildFromTypes(methodInfo, null, null, typeof(TResult));
        }

        //One parameter
        public Expression<Action<TParam1>> BuildActionStatic<TParam1>(MethodInfo methodInfo)
        {
            return (Expression<Action<TParam1>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1) }, typeof(void));
        }
        public Expression<Func<TParam1, TResult>> BuildFuncStatic<TParam1, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TParam1, TResult>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1) }, typeof(TResult));
        }

        //Two parameters
        public Expression<Action<TParam1, TParam2>> BuildActionStatic<TParam1, TParam2>(MethodInfo methodInfo)
        {
            return (Expression<Action<TParam1, TParam2>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2) }, typeof(void));
        }
        public Expression<Func<TParam1, TParam2, TResult>> BuildFuncStatic<TParam1, TParam2, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TParam1, TParam2, TResult>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2) }, typeof(TResult));
        }

        //Three parameters
        public Expression<Action<TParam1, TParam2, TParam3>> BuildActionStatic<TParam1, TParam2, TParam3>(MethodInfo methodInfo)
        {
            return (Expression<Action<TParam1, TParam2, TParam3>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(void));
        }
        public Expression<Func<TParam1, TParam2, TParam3, TResult>> BuildFuncStatic<TParam1, TParam2, TParam3, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TParam1, TParam2, TParam3, TResult>>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(TResult));
        }

    }

    public class StaticMethodInvokerBuilder
    {
        //public MethodInvokerBuilder()
        //{
        //    if (typeof(T).IsPrimitive)
        //        throw new InvalidOperationException(nameof(MethodInvokerBuilder<T>) + " cannot be ussed with primitive types");
        //}
        
        
        private LambdaExpression BuildGenericInvoker(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            var inputParameters = Expression.Parameter(typeof(object[]), "parameters");

            var methodParameters = methodInfo.GetParameters();
            var methodCallParameters = methodParameters
                .Select((x, i) => ExpressionEx.ConvertIfNeeded(
                            Expression.ArrayAccess(inputParameters, Expression.Constant(i)),
                            x.ParameterType));

            var invokerExpression = Expression.Call(
            methodInfo,
            methodCallParameters);

            var expressionParameters = new[] { inputParameters };
            var delegateType = typeof(GenericStaticInvoker);
            

            var body = (methodInfo.ReturnType == typeof(void)) ?
                    (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                    ExpressionEx.CastTypeSafe(invokerExpression, typeof(object));

            return Expression.Lambda(delegateType, body, expressionParameters);
        }

        private class MethodParameter
        {
            public Type MethodType { get; set; }
            public ParameterExpression InputParameter { get; set; }
        }

        public LambdaExpression BuildInvokerExpressionFromTypes(MethodInfo methodInfo, IEnumerable<Type> parameterTypes = null, Type returnType = null)
        {
            return BuildInvokerExpressionFromTypes(methodInfo, parameterTypes, returnType, null);
        }

        private LambdaExpression BuildInvokerExpressionFromTypes(MethodInfo methodInfo, IEnumerable<Type> parameterTypes, Type returnType, Type delegateType)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            returnType = returnType ?? methodInfo.ReturnType;
            
            var methodParameters =
                    methodInfo.GetParameters().ZipOutter(parameterTypes ?? Enumerable.Empty<Type>(),
                        (mp, pt) => new MethodParameter
                        {
                            InputParameter = Expression.Parameter(pt ?? mp.ParameterType, mp.Name),
                            MethodType = mp.ParameterType
                        })
                        .Select(x => new
                        {
                            x.InputParameter,
                            MethodCallParameter = x.MethodType == x.InputParameter.Type ? (Expression)x.InputParameter : ExpressionEx.ConvertIfNeeded(x.InputParameter, x.MethodType)
                        })
                        .ToList();

            var isAction = methodInfo.ReturnType == typeof(void) && (returnType == null || returnType == typeof(void));

            delegateType = delegateType ?? (isAction ?
                    GenericTypeExtensions.GetActionGenericType(methodParameters.Select(x => x.MethodCallParameter.Type)) :
                    GenericTypeExtensions.GetFuncGenericType(methodParameters.Select(x => x.MethodCallParameter.Type).Concat(new[] { returnType }).ToArray()));

            var invokerExpression = Expression.Call(
                null,
                methodInfo,
                methodParameters.Select(x => x.MethodCallParameter).Skip(methodInfo.IsStatic ? 0 : 1));

            var body = methodInfo.ReturnType == typeof(void) ?
                        (!isAction ?
                        (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                        invokerExpression) :
                    Expression.Convert(invokerExpression, returnType);

            return Expression.Lambda(delegateType, body, methodParameters.Select(x => x.InputParameter));
        }

        public LambdaExpression BuildInvokerExpressionFromDelegate(MethodInfo methodInfo, Type delegateType)
        {
            if (delegateType == null)
                return BuildInvokerExpressionFromTypes(methodInfo);

            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            var parameterTypes = methodInfo.GetParameters().Select(x => x.ParameterType);
            Type returnType = methodInfo.ReturnType;

            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException(delegateType.Name + " is not a delegate type");

            var invokeMethod = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            if (invokeMethod == null)
                throw new ArgumentException(delegateType.Name + " delegate type has no invoke method.");

            var invokeMethodParameters = invokeMethod.GetParameters();
            
            parameterTypes = invokeMethodParameters.Select(x => x.ParameterType);

            return BuildInvokerExpressionFromTypes(methodInfo, parameterTypes, returnType, delegateType);
        }

        public Expression<TDelegate> BuildTypedInvokerExpression<TDelegate>(MethodInfo methodInfo)
        {
            return (Expression<TDelegate>)BuildInvokerExpressionFromDelegate(methodInfo, typeof(TDelegate));
        }


        //No parameters

        public Expression<Action> BuildActionStatic(MethodInfo methodInfo)
        {
            return (Expression<Action>)BuildInvokerExpressionFromTypes(methodInfo, null, typeof(void));
        }
        public Expression<Func<TResult>> BuildFuncStatic<TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TResult>>)BuildInvokerExpressionFromTypes(methodInfo, null, typeof(TResult));
        }

        //One parameter
        public Expression<Action<TParam1>> BuildActionStatic<TParam1>(MethodInfo methodInfo)
        {
            return (Expression<Action<TParam1>>)BuildInvokerExpressionFromTypes(methodInfo, new[] { typeof(TParam1) }, typeof(void));
        }
        public Expression<Func<TParam1, TResult>> BuildFuncStatic<TParam1, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TParam1, TResult>>)BuildInvokerExpressionFromTypes(methodInfo, new[] { typeof(TParam1) }, typeof(TResult));
        }

        //Two parameters
        public Expression<Action<TParam1, TParam2>> BuildActionStatic<TParam1, TParam2>(MethodInfo methodInfo)
        {
            return (Expression<Action<TParam1, TParam2>>)BuildInvokerExpressionFromTypes(methodInfo, new[] { typeof(TParam1), typeof(TParam2) }, typeof(void));
        }
        public Expression<Func<TParam1, TParam2, TResult>> BuildFuncStatic<TParam1, TParam2, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TParam1, TParam2, TResult>>)BuildInvokerExpressionFromTypes(methodInfo, new[] { typeof(TParam1), typeof(TParam2) }, typeof(TResult));
        }

        //Three parameters
        public Expression<Action<TParam1, TParam2, TParam3>> BuildActionStatic<TParam1, TParam2, TParam3>(MethodInfo methodInfo)
        {
            return (Expression<Action<TParam1, TParam2, TParam3>>)BuildInvokerExpressionFromTypes(methodInfo, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(void));
        }
        public Expression<Func<TParam1, TParam2, TParam3, TResult>> BuildFuncStatic<TParam1, TParam2, TParam3, TResult>(MethodInfo methodInfo)
        {
            return (Expression<Func<TParam1, TParam2, TParam3, TResult>>)BuildInvokerExpressionFromTypes(methodInfo, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(TResult));
        }





    }

}