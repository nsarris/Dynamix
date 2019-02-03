using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    internal static class TaskCompatibility
    {
#if NET45
        public static Task<TResult> FromException<TResult>(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");

            var tcs = new TaskCompletionSource<TResult>();
            tcs.TrySetException(exception);
            return tcs.Task;
        }

        public static Task FromException(Exception exception)
        {
            return FromException<object>(exception);
        }

        public static Task CompletedTask => Task.FromResult<object>(null);
#else
        public static Task<TResult> FromException<TResult>(Exception exception)
            => Task.FromException<TResult>(exception);
        
        public static Task FromException(Exception exception)
            => Task.FromException(exception);
        
        public static Task CompletedTask => Task.CompletedTask; 
#endif
    }
}
