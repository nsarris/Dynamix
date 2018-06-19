using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    internal static class InvocationHelper
    {
        public static object[] GetInvocationParameters(IEnumerable<ParameterInfo> parameterInfos, IEnumerable<(string parameterName, object value)> namedParameters, bool defaultValueForMissing = false)
        {
            List<object> invocationParameters = new List<object>();
            namedParameters = namedParameters ?? Enumerable.Empty<(string, object)>();

            foreach (var p in parameterInfos)
            {
                var (parameterName, value) = namedParameters.FirstOrDefault(x => x.parameterName == p.Name);

                if (parameterName.IsNullOrEmpty() && !(p.IsOptional || p.HasDefaultValue))
                {
                    if (defaultValueForMissing)
                        invocationParameters.Add(p.ParameterType.DefaultOf());
                    else
                        throw new InvalidOperationException($"Non optional parameter '{p.Name}' missing");
                }
                else
                    invocationParameters.Add(value);
            }

            return invocationParameters.ToArray();
        }

        public static (string,object)[] GetInvocationParameters(object anonymousTypeArguments)
        {
            return new AnonymousTypeWrapper(anonymousTypeArguments).Fields.Select(x => (x.Value.AutoPropertyName, x.Value.Getter(anonymousTypeArguments))).ToArray();
        }
    }
}
