using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dynamix.Reflection
{
	public class TypeConstructor<T, TArg1, TArg2> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, T>(GetConstructor(typeof(TArg1), typeof(TArg2)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2)
        {
            var r = constructor(arg1, arg2);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var r = constructor(arg1, arg2, arg3);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var r = constructor(arg1, arg2, arg3, arg4);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
            initializer?.Invoke(r);
            return r;
        }
    }
	public class TypeConstructor<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16> : TypeConstructor<T>
        where T : class
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, T> constructor;

        public TypeConstructor(Type concreteType = null, Action<T> initializer = null)
            : base(concreteType, initializer)
        {
            constructor = MemberAccessorDelegateBuilder.CachedConstructorBuilder.Build<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, T>(GetConstructor(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15), typeof(TArg16)));
        }

        public T Construct(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16)
        {
            var r = constructor(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
            initializer?.Invoke(r);
            return r;
        }
    }
}