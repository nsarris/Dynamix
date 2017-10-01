using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions.Builders
{
    public class ConstructorInvokerBuilder
    {
        public LambdaExpression BuildTypedConstructorExpression(ConstructorInfo ctor, Type instanceType = null, IEnumerable<Type> parameterTypes = null)
        {
            instanceType = instanceType ?? ctor.DeclaringType;

            var constructorParameters = ctor.GetParameters()
                .ZipOutter(parameterTypes ?? Enumerable.Empty<Type>(),
                (cp, p) => new
                {
                    MethodType = cp.ParameterType,
                    Name = cp.Name,
                    InputParameter = Expression.Parameter(p ?? cp.ParameterType, cp.Name),
                })
                .Select(x => new
                {
                    x.InputParameter,
                    MethodCallParameter = x.MethodType == x.InputParameter.Type ? (Expression)x.InputParameter : ExpressionEx.ConvertIfNeeded(x.InputParameter, x.MethodType)
                })
                .ToList();

            var delegateType = GenericTypeExtensions.GetFuncGenericType(constructorParameters.Select(x => x.InputParameter.Type).Concat(new[] { instanceType }));
            var invokerExpression = (Expression)Expression.New(ctor, constructorParameters.Select(x => x.MethodCallParameter));
            if (instanceType != ctor.DeclaringType)
                invokerExpression = Expression.Convert(invokerExpression, instanceType);

            return Expression.Lambda(delegateType, invokerExpression, constructorParameters.Select(x => x.InputParameter).ToArray());
        }
    }
}
