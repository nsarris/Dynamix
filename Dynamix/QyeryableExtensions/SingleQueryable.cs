using System;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.QueryableExtensions
{
    public class SingleQueryable<T>
    {
        public SingleQueryable(IQueryable<T> queryable)
        {
            Queryable = queryable;
        }

        public SingleQueryable(T item)
        {
            Queryable = new[] { item }.AsQueryable();
        }

        public IQueryable<T> Queryable { get; private set; }
        public T Single()
        {
            return Queryable.Single();
        }

        public T SingleOrDefault()
        {
            return Queryable.SingleOrDefault();
        }

        public TResult Single<TResult>(Expression<Func<T, TResult>> selector)
        {
            return Queryable.Select(selector).Single();
        }

        public TResult SingleOrDefault<TResult>(Expression<Func<T, TResult>> selector)
        {
            return Queryable.Select(selector).SingleOrDefault();
        }

        public bool Satisfies(Expression<Func<T, bool>> condition)
        {
            return Queryable.All(condition);
        }

        public SingleQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            return new SingleQueryable<TResult>(Queryable.Select(selector));
        }

        public SingleQueryable<TResult> Cast<TResult>()
        {
            return new SingleQueryable<TResult>(Queryable.Cast<TResult>());
        }

        public override string ToString()
        {
            return Queryable.ToString();
        }
    }
}
