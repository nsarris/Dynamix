using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{


    public class MethodInfoExWrapper
    {
        public object WrappedObject { get; private set; }
        private IReadOnlyDictionary<string, List<MethodInfoEx>> MethodsByName { get; set; }
        public IEnumerable<MethodInfoEx> Methods { get; private set; }

        public MethodInfoExWrapper(object wrappedObject, BindingFlagsEx bindingFlags = BindingFlagsEx.All, bool enableFieldCaching = true)
            : this(wrappedObject, (BindingFlags)bindingFlags, enableFieldCaching)
        {
        }

        public MethodInfoExWrapper(object wrappedObject, BindingFlags bindingFlags, bool enableFieldCaching = true)
        {
            this.WrappedObject = wrappedObject ?? throw new ArgumentNullException(nameof(wrappedObject));
            this.Methods = new ReadOnlyCollection<MethodInfoEx>(wrappedObject.GetType().GetMethodsEx(bindingFlags, enableFieldCaching).ToList());
            this.MethodsByName = Methods.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.ToList());
        }


        public object Invoke(string methodName, IEnumerable<Type> signature, IEnumerable<object> arguments)
        {
            if (MethodsByName.TryGetValue(methodName, out var methods))
            {
                var method = methods.Where(x => x.MethodInfo.HasSignature(signature)).FirstOrDefault();
                if (method == null)
                    throw new MissingMethodException("Method " + methodName + "with given signature does not exist in Type " + WrappedObject.GetType());
                else
                    return method.Invoke(WrappedObject, arguments);
            }
            else
                throw new MissingMethodException("Method " + methodName + " does not exist in Type " + WrappedObject.GetType());
        }

        public object Invoke(string methodName, IEnumerable<object> arguments = null)
        {
            MethodInfoEx method = null;
            if (MethodsByName.TryGetValue(methodName, out var methods))
            {
                if (methods.Count > 1)
                    method = methods.First();
                else
                {
                    if (!arguments.Any(x => x == null))
                        method = methods.Where(x => x.MethodInfo.HasSignature(arguments.Select(y => y.GetType()))).FirstOrDefault();
                    
                    if (method == null)
                    {
                        //Best match
                        var matches = methods.Where(x => x.MethodInfo.IsInvokableWith(arguments)).ToList();

                        if (matches.Count == 1)
                            method = matches.First();
                        else if (matches.Count > 1)
                            throw new AmbiguousMatchException("More than one possible matches for " + methodName + " found in Type" + WrappedObject.GetType());
                    }
                }
            }

            if (method == null)
                throw new MissingMethodException("Method " + methodName + " does not exist in Type " + WrappedObject.GetType());
            else
                return method.Invoke(WrappedObject, arguments);
        }
    }

    public class MethodInfoExWrapper<T> : MethodInfoExWrapper
    {
        public MethodInfoExWrapper(object wrappedObject, BindingFlagsEx bindingFlags = BindingFlagsEx.All, bool enableFieldCaching = true)
            : base(wrappedObject, bindingFlags, enableFieldCaching)
        {
        }

        public MethodInfoExWrapper(object wrappedObject, BindingFlags bindingFlags, bool enableFieldCaching = true)
            : base(wrappedObject, bindingFlags, enableFieldCaching)
        {

        }

        public new T WrappedObject { get { return (T)base.WrappedObject; } }
    }
}
