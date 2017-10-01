using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public class DynamicQueryable : IQueryable, IEnumerable<object>
    {
        IQueryable source;

        public DynamicQueryable(IQueryable query)
        {
            this.source = query;
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
            if (predicate == null) throw new ArgumentNullException("predicate");
            return new DynamicQueryable((IQueryable)Execute(source, Functions.Where, predicate));
        }

        public DynamicQueryable Select(LambdaExpression selector)
        {
            if (selector == null) throw new ArgumentNullException("selector");
            return new DynamicQueryable((IQueryable)Execute(source, Functions.Select, selector));
        }

        public DynamicOrderedQueryable OrderBy(LambdaExpression ordering)
        {
            if (ordering == null) throw new ArgumentNullException("ordering");
            return new DynamicOrderedQueryable((IOrderedQueryable)Execute(source, Functions.OrderBy, ordering));
        }

        public DynamicOrderedQueryable OrderByDescending(LambdaExpression ordering)
        {
            if (ordering == null) throw new ArgumentNullException("ordering");
            return new DynamicOrderedQueryable((IOrderedQueryable)Execute(source, Functions.OrderByDescending, ordering));
        }

        public DynamicOrderedQueryable OrderBy(string ordering)
        {
            if (string.IsNullOrWhiteSpace(ordering)) throw new ArgumentNullException("ordering");
            var first = true;
            DynamicOrderedQueryable res = null;
            foreach (var order in ParseOrdering(source.ElementType, ordering))
            {
                if (first)
                    res = (order.Item2 ? OrderByDescending(order.Item1) : OrderBy(order.Item1));
                else
                    res = (order.Item2 ? res.ThenByDescending(order.Item1) : res.ThenBy(order.Item1));
                first = false;
            }
            return res;
        }

        public DynamicQueryable Take(int count)
        {
            return new DynamicQueryable((IQueryable)Execute(source, Functions.Take, Expression.Constant(count)));
        }

        public DynamicQueryable Skip(int count)
        {
            return new DynamicQueryable((IQueryable)Execute(source, Functions.Skip, Expression.Constant(count)));
        }

        public DynamicQueryable GroupBy(LambdaExpression keySelector)
        {
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            return new DynamicQueryable((IQueryable)Execute(source, Functions.GroupBy, keySelector));
        }

        public DynamicQueryable GroupBy(LambdaExpression keySelector, LambdaExpression elementSelector)
        {
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");
            return new DynamicQueryable((IQueryable)Execute(source, Functions.GroupBy, keySelector, elementSelector));
        }

        public bool Any(LambdaExpression predicate = null)
        {
            return (bool)Execute(source, Functions.Any, predicate);
        }

        public int Count(LambdaExpression predicate = null)
        {
            return (int)Execute(source, Functions.Count, predicate);
        }

        public DynamicQueryable Distinct(LambdaExpression predicate = null)
        {
            return new DynamicQueryable((IQueryable)Execute(source, Functions.Distinct, predicate));
        }

        public object First(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.First, predicate);
        }

        public object FirstOrDefault(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.FirstOrDefault, predicate);
        }

        public object Single(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.Single, predicate);
        }

        public object SingleOrDefault(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.SingleOrDefault, predicate);
        }

        public object Last(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.Single, predicate);
        }

        public object LastOrDefault(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.SingleOrDefault, predicate);
        }

        public object Min(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.Min, predicate);
        }

        public object Max(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.Max, predicate);
        }

        public object Average(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.Average, predicate);
        }

        public object Sum(LambdaExpression predicate = null)
        {
            return Execute(source, Functions.Sum, predicate);
        }

        public override string ToString()
        {
            //return (string)Execute(source, Functions.ToString);
            return source.ToString();
        }

        public object ToCastList(Type ListType = null)
        {
            if (ListType == null)
                ListType = typeof(object);
            
            var method = typeof(Queryable).GetMethod("Cast")
                    .MakeGenericMethod(ListType);
            var method2 = typeof(Enumerable).GetMethod("ToList")
                .MakeGenericMethod(ListType);

            var castData = method.Invoke(null, new object[] { source });
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
            return (IQueryable<T>)source.Provider.CreateQuery(source.Expression);
        }

        public IQueryable<T> AsQueryable<T>()
        {
            return (IQueryable<T>)source;
        }

        public IQueryable AsQueryable()
        {
            return (IQueryable)source;
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
            get { return source.ElementType; }
        }

        public Expression Expression
        {
            get { return source.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return source.Provider; }
        }

        public IEnumerator GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            var en = source.GetEnumerator();
            while (en.MoveNext())
                yield return en.Current;
        }

        private static LambdaExpression CreateMemberLambda(Type EntityType, string propertyName)
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
                            efmethods = new EFMethods();
                            efmethods.SetOfT = setMethods.Where(x => x.Args.Length == 1).Single().Method.MakeGenericMethod(entityType);
                            efmethods.Set = setMethods.Where(x => x.Args.Length == 0).Single().Method;
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
                    return new DynamicQueryable((IQueryable)efmethods.SetOfT.Invoke(dbContext, null));
                }
                else
                    return new DynamicQueryable((IQueryable)efmethods.Set.Invoke(dbContext, new[] { entityType }));
            }
        }
    }

    public class DynamicOrderedQueryable : DynamicQueryable, IOrderedQueryable
    {
        IOrderedQueryable source;
        public DynamicOrderedQueryable(IOrderedQueryable query)
            : base(query)
        {
            this.source = query;
        }
        public DynamicOrderedQueryable ThenBy(LambdaExpression ordering)
        {
            if (ordering == null) throw new ArgumentNullException("ordering");
            return new DynamicOrderedQueryable((IOrderedQueryable)Execute(source, Functions.ThenBy, ordering));
        }

        public DynamicOrderedQueryable ThenByDescending(LambdaExpression ordering)
        {
            if (ordering == null) throw new ArgumentNullException("ordering");
            return new DynamicOrderedQueryable((IOrderedQueryable)Execute(source, Functions.ThenByDescending, ordering));
        }
    }
}
