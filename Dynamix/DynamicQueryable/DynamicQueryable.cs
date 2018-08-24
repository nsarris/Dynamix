using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Reflection;

namespace Dynamix
{
    public class DynamicQueryable : IQueryable, IEnumerable<object>
    {
        protected IQueryable Source { get; private set; }

        public DynamicQueryable(IQueryable query)
        {
            this.Source = query;
        }

        protected enum Functions
        {
            Where,
            Select,
            OrderBy,
            OrderByDescending,
            ThenBy,
            ThenByDescending,
            Take,
            Skip,
            Any,
            Count,
            Distinct,
            GroupBy,
            First,
            FirstOrDefault,
            Single,
            SingleOrDefault,
            Last,
            LastOrDefault,
            Max,
            Min,
            Sum,
            Average,
            //ToList,
            //ToString,
            //Cast
        }

        protected static object Execute(IQueryable source, Functions function, params Expression[] parameters)
        {
            Type[] types = new Type[] { source.ElementType };
            Expression[] parameterExpressions = new Expression[] { source.Expression };
            bool Execute = false;

            switch (function)
            {
                case Functions.Where:case Functions.Take:
                case Functions.Skip:
                    parameterExpressions = parameterExpressions.Concat(parameters).ToArray();
                    break;
                case Functions.Any:
                case Functions.Count:
                case Functions.First:
                case Functions.FirstOrDefault:
                case Functions.Single:
                case Functions.SingleOrDefault:
                case Functions.Last:
                case Functions.LastOrDefault:
                case Functions.Max:
                case Functions.Min:
                case Functions.Sum:
                case Functions.Average:
                //case Functions.ToList:
                //case Functions.Cast:
                //case Functions.ToString:
                    Execute = true;
                    if (parameters != null && parameters.Any() && parameters[0] != null)
                        parameterExpressions = parameterExpressions.Concat(parameters).ToArray();
                    break;
                case Functions.Select:
                case Functions.Distinct:
                case Functions.OrderBy:
                case Functions.OrderByDescending:
                case Functions.ThenBy:
                case Functions.ThenByDescending:
                case Functions.GroupBy:
                    if (parameters != null && parameters.Any())
                    {
                        var p = parameters.Where(x => x != null);
                        types = types.Concat(p.Cast<LambdaExpression>().Select(x => x.Body.Type)).ToArray();
                        parameterExpressions = parameterExpressions.Concat(p).ToArray();
                    }
                    break;
                default:
                    throw new ArgumentException($"Unhandled function: {function})");
            }

            if (Execute)
                return source.Provider.Execute(
                    Expression.Call(
                        typeof(Queryable), function.ToString(),
                        types, parameterExpressions));
            else
                return source.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable), function.ToString(),
                        types, parameterExpressions));

        }

        public DynamicQueryable Where(LambdaExpression predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return new DynamicQueryable((IQueryable)Execute(Source, Functions.Where, predicate));
        }

        public DynamicQueryable Select(LambdaExpression selector)
        {
            if (selector == null) throw new ArgumentNullException("selector");
            return new DynamicQueryable((IQueryable)Execute(Source, Functions.Select, selector));
        }

        public DynamicOrderedQueryable OrderBy(LambdaExpression ordering)
        {
            if (ordering == null) throw new ArgumentNullException("ordering");
            return new DynamicOrderedQueryable((IOrderedQueryable)Execute(Source, Functions.OrderBy, ordering));
        }

        public DynamicOrderedQueryable OrderByDescending(LambdaExpression ordering)
        {
            if (ordering == null) throw new ArgumentNullException("ordering");
            return new DynamicOrderedQueryable((IOrderedQueryable)Execute(Source, Functions.OrderByDescending, ordering));
        }

        public DynamicOrderedQueryable OrderBy(string orderByExpression)
        {
            return OrderBy(OrderByExpressionParser.Parse(orderByExpression));
        }

        public DynamicOrderedQueryable OrderBy(IEnumerable<OrderItem> orderItems)
        {
            if (orderItems == null) throw new ArgumentNullException(nameof(orderItems), "Order items cannot be null");
            DynamicOrderedQueryable res = null;
            foreach (var item in orderItems)
            {
                var l =
                    item.ParseExpressionAs == ParseExpressionAs.Property ?
                        Expressions.MemberExpressionBuilder.GetPropertySelector(Source.ElementType, item.Expression) :
                    item.ParseExpressionAs == ParseExpressionAs.NestedProperty ?
                        Expressions.MemberExpressionBuilder.GetDeepPropertySelector(Source.ElementType, item.Expression)
                        :
                        Expressions.MemberExpressionBuilder.GetExpressionSelector(Source.ElementType, item.Expression);

                if (res == null)
                    res = (item.IsDescending ? OrderByDescending(l) : OrderBy(l));
                else
                    res = (item.IsDescending ? res.ThenByDescending(l) : res.ThenBy(l));
            }
            return res ?? new DynamicOrderedQueryable(Source);
        }

        public DynamicQueryable Take(int count)
        {
            return new DynamicQueryable((IQueryable)Execute(Source, Functions.Take, Expression.Constant(count)));
        }

        public DynamicQueryable Skip(int count)
        {
            return new DynamicQueryable((IQueryable)Execute(Source, Functions.Skip, Expression.Constant(count)));
        }

        public DynamicQueryable GroupBy(LambdaExpression keySelector)
        {
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            return new DynamicQueryable((IQueryable)Execute(Source, Functions.GroupBy, keySelector));
        }

        public DynamicQueryable GroupBy(LambdaExpression keySelector, LambdaExpression elementSelector)
        {
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");
            return new DynamicQueryable((IQueryable)Execute(Source, Functions.GroupBy, keySelector, elementSelector));
        }

        public bool Any(LambdaExpression predicate = null)
        {
            return (bool)Execute(Source, Functions.Any, predicate);
        }

        public int Count(LambdaExpression predicate = null)
        {
            return (int)Execute(Source, Functions.Count, predicate);
        }

        public DynamicQueryable Distinct(LambdaExpression predicate = null)
        {
            return new DynamicQueryable((IQueryable)Execute(Source, Functions.Distinct, predicate));
        }

        public object First(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.First, predicate);
        }

        public object FirstOrDefault(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.FirstOrDefault, predicate);
        }

        public object Single(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.Single, predicate);
        }

        public object SingleOrDefault(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.SingleOrDefault, predicate);
        }

        public object Last(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.Single, predicate);
        }

        public object LastOrDefault(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.SingleOrDefault, predicate);
        }

        public object Min(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.Min, predicate);
        }

        public object Max(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.Max, predicate);
        }

        public object Average(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.Average, predicate);
        }

        public object Sum(LambdaExpression predicate = null)
        {
            return Execute(Source, Functions.Sum, predicate);
        }

        public override string ToString()
        {
            return Source.ToString();
        }

        public object ToCastList(Type ListType = null)
        {
            if (ListType == null)
                ListType = typeof(object);
            
            var method = typeof(Queryable).GetMethod("Cast")
                    .MakeGenericMethodCached(ListType);
            var method2 = typeof(Enumerable).GetMethod("ToList")
                .MakeGenericMethodCached(ListType);

            var castData = method.Invoke(null, new object[] { Source });
            var retData = method2.Invoke(null, new object[] { castData });

            return retData;
        }

        public DynamicQueryable Cast(Type CastType = null)
        {
            if (CastType == null)
                CastType = typeof(object);

            return new DynamicQueryable(
                (IQueryable)Source.Provider.Execute(
                    Expression.Call(
                        typeof(Queryable), "Cast",
                        new Type[] { CastType }, Source.Expression)));
        }

        //public List<T> ToList<T>()
        //{
        //    return ((List<object>)Execute(source, Functions.ToList)).Cast<T>().ToList();
        //}

        public IQueryable<T> ToQueryable<T>()
        {
            return (IQueryable<T>)Source.Provider.CreateQuery(Source.Expression);
        }

        public IQueryable<T> AsQueryable<T>()
        {
            return (IQueryable<T>)Source;
        }

        public IQueryable AsQueryable()
        {
            return Source;
        }

        public Type ElementType
        {
            get { return Source.ElementType; }
        }

        public Expression Expression
        {
            get { return Source.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return Source.Provider; }
        }

        public IEnumerator GetEnumerator()
        {
            return Source.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            var en = Source.GetEnumerator();
            while (en.MoveNext())
                yield return en.Current;
        }

        public static DynamicQueryable FromEntityFrameworkDbContextSet(object dbContext, Type entityType, bool stateTracking = false)
        {
            return EntityFrameworkHelper.FromDbContextSet(dbContext, entityType, stateTracking);
        }

        public static DynamicQueryable FromLinqToDBDataConnection(object dataConnection, Type modelType)
        {
            return LinqToDBHeper.FromLinqToDBDataConnection(dataConnection, modelType);
        }
    }
}
