using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Helpers
{
    public static class With
    {
        public static WithObjects<T> Objects<T>(T item)
        {
            return new WithObjects<T>(item);
        }

        public static WithObjects<T1, T2> Objects<T1, T2>(T1 item1, T2 item2)
        {
            return new WithObjects<T1, T2>(item1, item2);
        }

        public static WithObjects<T1, T2, T3> Objects<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new WithObjects<T1, T2, T3>(item1, item2, item3);
        }

        public static WithObjects<T1, T2, T3, T4> Objects<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new WithObjects<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }





    public class WithObjects<T>
    {
        readonly T obj;
        public WithObjects(T @object)
        {
            this.obj = @object;
        }

        public WithObjects<T> Do(Action<T> action)
        {
            action(obj);
            return this;
        }

        public WithObjects<T> Do<TResult>(Func<T, TResult> function, out TResult result)
        {
            result = function(obj);
            return this;
        }

        public TResult Get<TResult>(Func<T, TResult> function)
        {
            return function(obj);
        }
    }

    public class WithObjects<T1, T2>
    {
        readonly T1 obj1;
        readonly T2 obj2;

        public WithObjects(T1 obj1, T2 obj2)
        {
            this.obj1 = obj1;
            this.obj2 = obj2;
        }

        public WithObjects<T1, T2> Do(Action<T1, T2> action)
        {
            action(obj1, obj2);
            return this;
        }

        public WithObjects<T1, T2> Do<TResult>(Func<T1, T2, TResult> function, out TResult result)
        {
            result = function(obj1, obj2);
            return this;
        }

        public TResult Get<TResult>(Func<T1, T2, TResult> function)
        {
            return function(obj1, obj2);
        }
    }

    public class WithObjects<T1, T2, T3>
    {
        readonly T1 obj1;
        readonly T2 obj2;
        readonly T3 obj3;

        public WithObjects(T1 obj1, T2 obj2, T3 obj3)
        {
            this.obj1 = obj1;
            this.obj2 = obj2;
            this.obj3 = obj3;
        }

        public WithObjects<T1, T2, T3> Do(Action<T1, T2, T3> action)
        {
            action(obj1, obj2, obj3);
            return this;
        }

        public WithObjects<T1, T2, T3> Do<TResult>(Func<T1, T2, T3, TResult> function, out TResult result)
        {
            result = function(obj1, obj2, obj3);
            return this;
        }

        public TResult Get<TResult>(Func<T1, T2, T3, TResult> function)
        {
            return function(obj1, obj2, obj3);
        }
    }

    public class WithObjects<T1, T2, T3, T4>
    {
        readonly T1 obj1;
        readonly T2 obj2;
        readonly T3 obj3;
        readonly T4 obj4;

        public WithObjects(T1 obj1, T2 obj2, T3 obj3, T4 obj4)
        {
            this.obj1 = obj1;
            this.obj2 = obj2;
            this.obj3 = obj3;
            this.obj4 = obj4;
        }

        public WithObjects<T1, T2, T3, T4> Do(Action<T1, T2, T3, T4> action)
        {
            action(obj1, obj2, obj3, obj4);
            return this;
        }

        public WithObjects<T1, T2, T3, T4> Do<TResult>(Func<T1, T2, T3, T4, TResult> function, out TResult result)
        {
            result = function(obj1, obj2, obj3, obj4);
            return this;
        }

        public TResult Get<TResult>(Func<T1, T2, T3, T4, TResult> function)
        {
            return function(obj1, obj2, obj3, obj4);
        }
    }
}
