using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{
    public static class Define
    {
        public static Expression<Func<T>> Lambda<T>(Expression<Func<T>> expression)
        {
            return expression;
        }
        public static Expression<Func<T1, T3>> Lambda<T1, T3>(Expression<Func<T1, T3>> expression)
        {
            return expression;
        }

        public static Expression<Func<T1, T2, T3>> Lambda<T1, T2, T3>(Expression<Func<T1, T2, T3>> expression)
        {
            return expression;
        }

        public static Expression<Func<T1, T2, T3, T4>> Lambda<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4>> expression)
        {
            return expression;
        }


        public static Func<T> Delegate<T>(Func<T> expression)
        {
            return expression;
        }
        public static Func<T1, T2> Delegate<T1, T2>(Func<T1, T2> expression)
        {
            return expression;
        }

        public static Func<T1, T2, T3> Delegate<T1, T2, T3>(Func<T1, T2, T3> expression)
        {
            return expression;
        }

        public static Func<T1, T2, T3, T4> Delegate<T1, T2, T3, T4>(Func<T1, T2, T3, T4> expression)
        {
            return expression;
        }
    }
}
