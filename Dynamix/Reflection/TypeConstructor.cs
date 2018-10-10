using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public class TypeConstructor<T>
            where T : class
    {
        protected readonly Action<T> initializer;

        public Type ReferenceType { get; }
        public Type ConcreteType { get; }

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
        {
            this.initializer = initializer;

            ReferenceType = typeof(T);
            ConcreteType = concreteType ?? ReferenceType;

            if (ReferenceType != ConcreteType && !ReferenceType.IsAssignableFrom(ConcreteType))
                throw new InvalidOperationException($"{ReferenceType.Name} cannot be used as a reference type for concrete type {ConcreteType.Name}");
        }

        protected ConstructorInfo GetConstructor(params Type[] types)
        {
            var ctor = ConcreteType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, types, null);
            if (ctor == null)
                throw new InvalidOperationException($"Type {ConcreteType.Name} does not have a public constructor that with the specified argument");
            return ctor;
        }

        public T Construct(IEnumerable<object> arguments)
        {
            return Construct(arguments.ToArray());
        }

        public T Construct(params object[] arguments)
        {
            var ctor = ConcreteType.GetConstructorsEx().FirstOrDefault(c => c.Signature.Count == arguments.Count());
            if (ctor == null)
                throw new InvalidOperationException($"Type {ConcreteType.Name} does not have a public constructor that has the specified number of arguments");

            var r = (T)ctor.Invoke(arguments);
            initializer?.Invoke(r);
            return r;
        }

        public T ConstructWithDefaults(params object[] arguments)
        {
            return ConstructWithDefaults(arguments.AsEnumerable());
        }

        public T ConstructWithDefaults(IEnumerable<object> arguments)
        {
            var ctor = ConcreteType.GetConstructorsEx().FirstOrDefault(c => c.Signature.Count < arguments.Count());
            if (ctor == null)
                throw new InvalidOperationException($"Type {ConcreteType.Name} does not have a public constructor that has that many arguments");

            var r = (T)ctor.InvokeWithDefaults(arguments);
            initializer?.Invoke(r);
            return r;
        }

        public T Construct(params (string name, object value)[] arguments)
        {
            return Construct(arguments.AsEnumerable());
        }

        public T Construct(IEnumerable<(string name, object value)> arguments)
        {
            var ctor = ConcreteType.GetConstructorsEx().FirstOrDefault(c =>
                arguments.All(x => c.Signature.Keys.Contains(x.name)));

            if (ctor == null)
                throw new InvalidOperationException($"Type {ConcreteType.Name} does not have a public constructor that has a constructor with given parameter names");

            var r = (T)ctor.Invoke(arguments);
            initializer?.Invoke(r);
            return r;
        }

        public T ConstructWithDefaults(params (string name, object value)[] arguments)
        {
            return ConstructWithDefaults(arguments.AsEnumerable());
        }

        public T ConstructWithDefaults(IEnumerable<(string name, object value)> arguments)
        {
            var ctor = ConcreteType.GetConstructorsEx().FirstOrDefault(c =>
                arguments.All(x => c.Signature.Keys.Contains(x.name)));

            if (ctor == null)
                throw new InvalidOperationException($"Type {ConcreteType.Name} does not have a public constructor that has a constructor with given parameter names");

            var r = (T)ctor.InvokeWithDefaults(arguments);
            initializer?.Invoke(r);
            return r;
        }
    }

    public class TypeConstructor<T, TArgument> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArgument, T> constructor;



        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArgument, T>(GetConstructor(typeof(TArgument)));
        }

        public T Construct(TArgument argument)
        {
            var r = constructor(argument);
            initializer?.Invoke(r);
            return r;
        }
    }

    public class TypeConstructor<T, TArgument1, TArgument2> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArgument1, TArgument2, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArgument1, TArgument2, T>(GetConstructor(typeof(TArgument1), typeof(TArgument2)));
        }

        public T Construct(TArgument1 argument1, TArgument2 argument2)
        {
            var r = constructor(argument1, argument2);
            initializer?.Invoke(r);
            return r;
        }
    }

    public class TypeConstructor<T, TArgument1, TArgument2, TArgument3> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArgument1, TArgument2, TArgument3, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArgument1, TArgument2, TArgument3, T>(GetConstructor(typeof(TArgument1), typeof(TArgument2), typeof(TArgument3)));
        }

        public T Construct(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            var r = constructor(argument1, argument2, argument3);
            initializer?.Invoke(r);
            return r;
        }
    }

    public class TypeConstructor<T, TArgument1, TArgument2, TArgument3, TArgument4> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArgument1, TArgument2, TArgument3, TArgument4, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArgument1, TArgument2, TArgument3, TArgument4, T>(GetConstructor(typeof(TArgument1), typeof(TArgument2), typeof(TArgument3), typeof(TArgument4)));
        }

        public T Construct(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4)
        {
            var r = constructor(argument1, argument2, argument3, argument4);
            initializer?.Invoke(r);
            return r;
        }
    }
}
