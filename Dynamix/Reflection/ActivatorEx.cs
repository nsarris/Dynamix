using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public static class ActivatorEx
    {
        public static object CreateInstance(Type t)
        {
            var m = typeof(ActivatorEx<>).MakeGenericTypeCached(new Type[] { t }).GetMethod("CreateInstance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            return m.Invoke(null, null);
        }

        public static T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        public static T CreateInstance<T>(T instance)
        {
            return (T)CreateInstance(typeof(T));
        }
    }
    public static class ActivatorEx<T>
    {
        private static readonly Func<T> CreateInstanceFunc = Creator();

        public static T CreateInstance()
        {
            return CreateInstanceFunc();
        }

        static Func<T> Creator()
        {
            Type t = typeof(T);
            if (t == typeof(string))
                return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();

            if (HasDefaultConstructor(t))
                return Expression.Lambda<Func<T>>(Expression.New(t)).Compile();

            return () => (T)FormatterServices.GetUninitializedObject(t);
        }

        static bool HasDefaultConstructor(Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}
