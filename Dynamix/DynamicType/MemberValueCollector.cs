using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix
{
    public class MemberValueCollector<T>
        where T : class
    {
        private readonly Dictionary<string, object> memberValues = new Dictionary<string, object>();

        public MemberValueCollector<T> Member(string name, object value)
        {
            memberValues[name] = value;
            return this;
        }

        public MemberValueCollector<T> Member<TMember>(Expression<Func<T, TMember>> memberSelector, TMember value)
        {
            memberValues[ReflectionHelper.GetMemberName(memberSelector)] = value;
            return this;
        }

        public IEnumerable<(string, object)> GetTuples()
            => memberValues.Select(x => (x.Key, x.Value));

        public IReadOnlyDictionary<string, object> GetDictionary()
            => memberValues;
    }
}
