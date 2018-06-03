using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public static class MemberInfoExtensions
    {
        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            if (Attribute.IsDefined(member, typeof(T)))
                return ((T)(Attribute.GetCustomAttribute(member, typeof(T))));
            else
                return null;
        }

        public static bool HasAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return (Attribute.IsDefined(member, typeof(T)));
        }

        public static bool HasAttribute(this MemberInfo member, Attribute attr)
        {
            return (Attribute.IsDefined(member, attr.GetType()));
        }

        public static bool HasSignature(this MethodInfo methodInfo, IEnumerable<Type> signature)
        {
            return methodInfo.GetParameters().Select(x => x.ParameterType).SequenceEqual(signature);
        }

        public static bool IsInvokableWith(this MethodInfo methodInfo, IEnumerable<object> parameters)
        {
            IEnumerable<Type> signature;

            if (parameters == null)
                signature = Type.EmptyTypes;
            else
                signature = parameters.Select(x => x == null ? typeof(object) : x.GetType());

            if (parameters.Count() == signature.Count())
            {
                var noMatch = false;
                foreach (var p in parameters)
                {
                    foreach (var t in signature)
                    {
                        if ((!(p is null) && !t.IsAssignableFrom(p.GetType()))
                            || (p is null && !t.IsByRef))
                        {
                            noMatch = true;
                            break;
                        }
                    }
                    if (noMatch)
                        break;
                }
                if (!noMatch)
                    return true;
            }

            return false;
        }
    }
}
