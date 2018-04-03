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
                case Functions.Where:
                case Functions.Take:
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
                    if (parameters != null && parameters.Count() > 0 && parameters[0] != null)
                        parameterExpressions = parameterExpressions.Concat(parameters).ToArray();
                    break;
                case Functions.Select:
                case Functions.Distinct:
                case Functions.OrderBy:
                case Functions.OrderByDescending:
                case Functions.ThenBy:
                case Functions.ThenByDescending:
                case Functions.GroupBy:
                    if (parameters != null && parameters.Count() > 0)
                    {
                        var p = parameters.Where(x => x != null);
                        types = types.Concat(p.Cast<LambdaExpression>().Select(x => x.Body.Type)).ToArray();
                        parameterExpressions = parameterExpressions.Concat(p).ToArray();
                    }
                    break;
                default:
                    throw new Exception("Unhandled function: " + function);
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
            if (predicate == null) //throw new ArgumentNullException("predicate");
                return this;
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

        public DynamicOrderedQueryable OrderBy(string ordering)
        {
            if (string.IsNullOrWhiteSpace(ordering)) throw new ArgumentNullException("ordering");
            DynamicOrderedQueryable res = null;
            foreach (var order in ParseOrdering(Source.ElementType, ordering))
            {
                if (res == null)
                    res = (order.Item2 ? OrderByDescending(order.Item1) : OrderBy(order.Item1));
                else
                    res = (order.Item2 ? res.ThenByDescending(order.Item1) : res.ThenBy(order.Item1));
            }
            return res;
        }

        public DynamicOrderedQueryable OrderBy(IEnumerable<OrderItem> orderItems)
        {
            if (orderItems == null) throw new ArgumentNullException("Order items cannot be null");
            DynamicOrderedQueryable res = null;
            foreach (var item in orderItems)
            {
                var l = CreateMemberLambda(Source.ElementType, item.PropertyName);
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
            //return (string)Execute(source, Functions.ToString);
            return Source.ToString();
        }

        public object ToCastList(Type ListType = null)
        {
            if (ListType == null)
                ListType = typeof(object);
            
            var method = typeof(Queryable).GetMethod("Cast")
                    .MakeGenericMethod(ListType);
            var method2 = typeof(Enumerable).GetMethod("ToList")
                .MakeGenericMethod(ListType);

            var castData = method.Invoke(null, new object[] { Source });
            var retData = method2.Invoke(null, new object[] { castData });

            return retData;
        }

        //public DynamicQueryable Cast(Type CastType = null)
        //{
        //    if (CastType == null)
        //        CastType = typeof(object);
            
        //    return new DynamicQueryable(
        //        (IQueryable) source.Provider.Execute(
        //            Expression.Call(
        //                typeof(Queryable), "Cast",
        //                new Type [] { CastType },source.Expression)));
        //}

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
            return (IQueryable)Source;
        }

        private List<Tuple<LambdaExpression, bool>> ParseOrdering(Type entityType, string ordering)
        {
            var res = new List<Tuple<LambdaExpression, bool>>();
            
            foreach (var item in ordering.Split(','))
            {
                var o = item.Trim().Split(' ');
                if (o.Count() > 2 || o.Count() == 0)
                    throw new ArgumentException("Invalid OrderBy Expression: [" + ordering +"]");

                var propName = o[0].Trim();
                if (!entityType.GetProperties().Any(x => x.Name == propName))
                    throw new ArgumentException("Invalid Property [" + propName +"] in OrderBy Expression: [" + ordering +"]");

                var l = CreateMemberLambda(entityType, o[0].Trim());

                var order = false; //asc

                if (o.Count() == 2)
                {
                    var orderStr = o[1].Trim().ToLower();

                    if (orderStr == "desc")
                        order = true;
                    else if (orderStr != "asc")
                        throw new ArgumentException("Invalid OrderBy Expression");
                }

                res.Add(new Tuple<LambdaExpression, bool>(l, order));
            }
            return res;
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

        internal static LambdaExpression CreateMemberLambda(Type EntityType, string propertyName)
        {
            var param = Expression.Parameter(EntityType, "x");
            Expression body = param;
            foreach (var member in propertyName.Split('.'))
                body = Expression.PropertyOrField(body, member);
            return Expression.Lambda(body, param);
        }

        private class EFMethods
        {
            public MethodInfo Set;
            public MethodInfo SetOfT;
            //public MethodInfo SetAsNoTracking;
        }

        static EFMethods efmethods;
        static object eflock = new object();
        public static DynamicQueryable FromDbContextSet(object dbContext, Type entityType, bool StateTracking = false)
        {
            //TODO DbSet<T> not working properly, disabling StateTracking by default
            if (efmethods == null)
            {
                lock (eflock)
                {
                    if (efmethods == null)
                    {
                        var setMethods = dbContext.GetType().GetMethods().Where(m => m.Name == "Set")
                            .Select(m => new
                            {
                                Method = m,
                                Params = m.GetParameters(),
                                Args = m.GetGenericArguments()
                            });
                            //.Where(x => x.Params.Length == 1
                            ////&& x.Args.Length == 1
                            ////&& x.Params[0].ParameterType == x.Args[0])
                            //    )
                            //.FirstOrDefault();

                        if (setMethods != null)
                        {
                            efmethods = new EFMethods
                            {
                                SetOfT = setMethods.Where(x => x.Args.Length == 1).Single().Method,
                                Set = setMethods.Where(x => x.Args.Length == 0).Single().Method
                            };
                        }
                    }
                }
            }

            if (efmethods.Set == null)
                throw new Exception("DbSet method not found on DbContext");
            else
            {
                if (StateTracking)
                {
                    return new DynamicQueryable((IQueryable)efmethods.SetOfT.MakeGenericMethod(entityType).Invoke(dbContext, null));
                }
                else
                    return new DynamicQueryable((IQueryable)efmethods.Set.Invoke(dbContext, new[] { entityType }));
            }
        }

        private class LinqToDBMethods
        {
            public MethodInfo GetTableT;
        }

        static LinqToDBMethods linqToDBMethods;
        static object l2dblock = new object();
        public static DynamicQueryable FromLinqToDBDataConnection(object dataConnection, Type modelType)
        {
            if (linqToDBMethods == null)
            {
                lock (l2dblock)
                {
                    if (linqToDBMethods == null)
                    {
                        linqToDBMethods = new LinqToDBMethods()
                        {
                            GetTableT = dataConnection.GetType().GetMethod("GetTable", new Type[] { })
                        };
                    }
                }
            }

            if (linqToDBMethods.GetTableT == null)
                throw new Exception("GetTable method not found on DataConnection");
            else
            {
                return new DynamicQueryable((IQueryable)linqToDBMethods.GetTableT.MakeGenericMethodCached(modelType).Invoke(dataConnection, new object [0]));
            }
        }
    }

    public class DynamicOrderedQueryable : DynamicQueryable, IOrderedQueryable
    {
        internal DynamicOrderedQueryable(IQueryable query)
            : base(query.AsQueryable())
        {
            
        }
        public DynamicOrderedQueryable(IOrderedQueryable query)
            : base(query)
        {
            
        }
        public DynamicOrderedQueryable ThenBy(LambdaExpression ordering)
        {
            if (ordering == null) throw new ArgumentNullException("ordering");
            if (Source is IOrderedQueryable)
                return new DynamicOrderedQueryable((IOrderedQueryable)Execute(Source, Functions.ThenBy, ordering));
            else
                return OrderBy(ordering);
        }

        public DynamicOrderedQueryable ThenByDescending(LambdaExpression ordering)
        {
            if (ordering == null) throw new ArgumentNullException("ordering");
            if (Source is IOrderedQueryable)
                return new DynamicOrderedQueryable((IOrderedQueryable)Execute(Source, Functions.ThenByDescending, ordering));
            else
                return OrderByDescending(ordering);
        }

        public DynamicOrderedQueryable ThenBy(IEnumerable<OrderItem> orderItems)
        {
            if (orderItems == null || !orderItems.Any()) throw new ArgumentNullException("Order items cannot be null or empty");
            DynamicOrderedQueryable res = null;
            foreach (var item in orderItems)
            {
                var l = CreateMemberLambda(Source.ElementType, item.PropertyName);
                if (Source is IOrderedQueryable)
                    res = (item.IsDescending ? res.ThenByDescending(l) : res.ThenBy(l));
                else
                    res = (item.IsDescending ? OrderByDescending(l) : OrderBy(l));
            }
            return res;
        }
    }

    public class OrderItem
    {
        public OrderItem(string propertyName, bool isDescending)
        {
            PropertyName = propertyName;
            IsDescending = isDescending;
        }

        public OrderItem(string propertyName, string direction)
        {
            PropertyName = propertyName;
            IsDescending = (StringComparer.OrdinalIgnoreCase.Compare(direction, "DESC") == 0);
        }

        public string PropertyName { get; set; }
        public bool IsDescending { get; set; }
    }

}
