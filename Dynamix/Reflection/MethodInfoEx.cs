using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public class MethodInfoExIgnoreAttribute : Attribute
    {

    }

    [DebuggerDisplay("Name = {MethodInfo.Name}, ReturnType = {MethodInfo.ReturnType}")]
    public class MethodInfoEx
    {
        public MethodInfo MethodInfo { get; private set; }
        public bool IsExtension { get; private set; }

        Func<object, object[], object> invoker;
        Func<object[], object> staticInvoker;

        public MethodInfoEx(MethodInfo method)
        {
            this.MethodInfo = method;
            
            InitializeInvoker();
        }

        private void InitializeInvoker()
        {
            var methodCallParameters = Expression.Parameter(typeof(object[]), "parameters");
            var parameterExpressions = MethodInfo.GetParameters()
                .Select((x, i) => Expression.Convert(
                            Expression.ArrayAccess(methodCallParameters, Expression.Constant(i)),
                            x.ParameterType));
                
            Expression invokerExpression;
            
            if (!MethodInfo.IsStatic)
            {
                var instanceParameter = Expression.Parameter(typeof(object), "instance");

                invokerExpression = Expression.Call(
                Expression.Convert(instanceParameter, MethodInfo.DeclaringType),
                MethodInfo,
                parameterExpressions);

                var body = (MethodInfo.ReturnType == typeof(void)) ?
                    (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                    (Expression)Expression.Convert(invokerExpression, typeof(object));

                var invokerLambda = Expression.Lambda<Func<object, object[], object>>(body, instanceParameter, methodCallParameters);
                invoker = invokerLambda.Compile();
            }
            else
            {
                invokerExpression = Expression.Call(
                MethodInfo,
                parameterExpressions);

                var body = (MethodInfo.ReturnType == typeof(void)) ?
                    (Expression)Expression.Block(typeof(object), invokerExpression, Expression.Constant(null)) :
                    (Expression)Expression.Convert(invokerExpression, typeof(object));

                var invokerLambda = Expression.Lambda<Func<object[], object>>(body, methodCallParameters);
                staticInvoker = invokerLambda.Compile();
            }
        }

        public T GetTypedInvoker<T>()
        {
            var methodTypeArguments = new Type[] { MethodInfo.DeclaringType }
                    .Concat(MethodInfo.GetParameters().Select(x => x.ParameterType));

            var funcTypeArguments = new Type[] { MethodInfo.DeclaringType }
                    .Concat(MethodInfo.GetParameters().Select(x => x.ParameterType));

            var isFunc = typeof(T).HasGenericSignature(typeof(Func<>), methodTypeArguments.Concat(new[] { MethodInfo.ReturnType }).ToArray());
            var isAction = typeof(T).HasGenericSignature(typeof(Action<>), methodTypeArguments.Concat(new[] { MethodInfo.ReturnType }).ToArray());

            if (!isAction && (isFunc && MethodInfo.ReturnType == typeof(void)))
                throw new Exception("Invoker type must be of Action<Instance,Paremeters1...n,ReturnType>");
            if (!isFunc && (isAction && MethodInfo.ReturnType != typeof(void)))
                throw new Exception("Invoker type must be of Action<Instance,Paremeters1...n>");

            
            var methodParameters = MethodInfo.GetParameters().ToList();

            var parameterExpressions = methodParameters
                .Select((x, i) => Expression.Parameter(x.ParameterType))
                .ToList();

            Expression invokerExpression;

            if (!MethodInfo.IsStatic)
            {
                var instanceParameter = Expression.Parameter(MethodInfo.DeclaringType, "instance");

                invokerExpression = Expression.Call(
                    Expression.Convert(instanceParameter, MethodInfo.DeclaringType),
                    MethodInfo,
                    parameterExpressions);

                var invokerLambda = Expression.Lambda<T>(invokerExpression, new[] { instanceParameter }.Concat(parameterExpressions));
                return (T)invokerLambda.Compile();
            }
            else
            {
                invokerExpression = Expression.Call(
                    MethodInfo,
                    parameterExpressions);

                var invokerLambda = Expression.Lambda<T>(invokerExpression, parameterExpressions);
                return (T)invokerLambda.Compile();
            }
            
            
        }

        public object InvokeStatic(params object[] arguments)
        {
            return staticInvoker(arguments);
        }

        public object Invoke(object instance, params object[] arguments)
        {
            return Invoke(instance, arguments, false);
        }

        public object Invoke(object instance, object[] arguments, bool allowPrivate)
        {
            if (!allowPrivate && !MethodInfo.IsPublic)
                throw new Exception("Method " + this.MethodInfo.Name + " of " + this.MethodInfo.DeclaringType.FullName + " is not public.");

            return invoker.Invoke(instance, arguments ?? (new object[] { }));
        }

        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }

        public static Expression For(ParameterExpression loopVar, Expression initValue, Expression condition, Expression increment, Expression loopContent)
        {
            var initAssign = Expression.Assign(loopVar, initValue);

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(new[] { loopVar },
                initAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        condition,
                        Expression.Block(
                            loopContent,
                            increment
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }
    }
}
