using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    internal class HashKey : IEquatable<HashKey>
    {
        readonly int hashCode;
        public HashKey(IEnumerable values)
        {
            hashCode = GetHashCode(values);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(HashKey))
                return false;

            return ((HashKey)obj).GetHashCode() == this.GetHashCode();
        }

        public bool Equals(HashKey other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        protected static int GetHashCode(IEnumerable values, int prime = 397)
        {
            if (values == null)
                return 0;

            var tmpvalues = values.Cast<object>();

            if (!tmpvalues.Any())
                return 0;

            unchecked
            {
                int hashCode = tmpvalues.First().GetHashCode();
                foreach (var v in tmpvalues.Skip(1))
                    hashCode = (hashCode * prime) ^ (v == null ? 0 : v.GetHashCode());

                return hashCode;
            }
        }
    }
}
