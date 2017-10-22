using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.QueryableExtensions
{
    public static class QyerableExtensions
    {
        public static SingleQueryable<T> ToSingleQueryable<T>(this IQueryable<T> queryable)
        {
            return new SingleQueryable<T>(queryable);
        }

        public static SingleQueryable<T> ToSingleQueryable<T>(this IQueryable<T> queryable, Expression<Func<T,bool>> predicate)
        {
            return new SingleQueryable<T>(queryable.Where(predicate));
        }
    }
}
