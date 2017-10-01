using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public static class LINQExtensions
    {
        
        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, int, TResult> resultSelector)
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");

            using (IEnumerator<TFirst> firstEnumerator = first.GetEnumerator())
            using (IEnumerator<TSecond> secondEnumerator = second.GetEnumerator())
            {
                var i = 0;
                while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext())
                    yield return resultSelector(firstEnumerator.Current, secondEnumerator.Current, i);

            }
        }

        public static IEnumerable<TResult> ZipOutter<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector, TFirst emptyFirst = default(TFirst), TSecond emptySecond = default(TSecond))
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");

            using (IEnumerator<TFirst> firstEnumerator = first.GetEnumerator())
            using (IEnumerator<TSecond> secondEnumerator = second.GetEnumerator())
            {
                var firstHasValue = firstEnumerator.MoveNext();
                var secondHasValue = secondEnumerator.MoveNext();
                while (firstHasValue | secondHasValue)
                {
                    yield return resultSelector(
                        firstHasValue ? firstEnumerator.Current : emptyFirst,
                        secondHasValue ? secondEnumerator.Current : emptySecond);

                    firstHasValue = firstEnumerator.MoveNext();
                    secondHasValue = secondEnumerator.MoveNext();
                }
            }
        }
    }
}
