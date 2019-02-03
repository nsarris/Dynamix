using System;
using System.Threading.Tasks;
using Dynamix.Helpers;

namespace Dynamix.Reflection
{
	public interface IActionWrapper
	{
		Delegate Delegate { get; }
		void Invoke();
		Task InvokeAsync();
	}

	public class ActionWrapper : IActionWrapper
    {
        private readonly Action operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke()
        {
            operation();
        }

        public Task InvokeAsync()
        {
            try
            {
                operation();
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper : IActionWrapper
    {
        private readonly Func<Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke()
        {
            AsyncTaskHelper.RunSync(() => operation());
        }

        public Task InvokeAsync()
        {
            return operation();
        }

		public static implicit operator Delegate(AsyncActionWrapper actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1);
		Task InvokeAsync(TArg1 arg1);
	}

	public class ActionWrapper<TArg1> : IActionWrapper<TArg1>
    {
        private readonly Action<TArg1> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1)
        {
            operation(arg1);
        }

        public Task InvokeAsync(TArg1 arg1)
        {
            try
            {
                operation(arg1);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1> : IActionWrapper<TArg1>
    {
        private readonly Func<TArg1, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1));
        }

        public Task InvokeAsync(TArg1 arg1)
        {
            return operation(arg1);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2);
	}

	public class ActionWrapper<TArg1, TArg2> : IActionWrapper<TArg1, TArg2>
    {
        private readonly Action<TArg1, TArg2> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2)
        {
            operation(arg1, arg2);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2)
        {
            try
            {
                operation(arg1, arg2);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2> : IActionWrapper<TArg1, TArg2>
    {
        private readonly Func<TArg1, TArg2, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2)
        {
            return operation(arg1, arg2);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3> : IActionWrapper<TArg1, TArg2, TArg3>
    {
        private readonly Action<TArg1, TArg2, TArg3> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            operation(arg1, arg2, arg3);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            try
            {
                operation(arg1, arg2, arg3);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3> : IActionWrapper<TArg1, TArg2, TArg3>
    {
        private readonly Func<TArg1, TArg2, TArg3, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return operation(arg1, arg2, arg3);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4> : IActionWrapper<TArg1, TArg2, TArg3, TArg4>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            operation(arg1, arg2, arg3, arg4);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4> : IActionWrapper<TArg1, TArg2, TArg3, TArg4>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return operation(arg1, arg2, arg3, arg4);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            operation(arg1, arg2, arg3, arg4, arg5);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            return operation(arg1, arg2, arg3, arg4, arg5);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12, in TArg13>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12, in TArg13, in TArg14>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12, in TArg13, in TArg14, in TArg15>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> actionWrapper) => actionWrapper.Delegate;
    }

	public interface IActionWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12, in TArg13, in TArg14, in TArg15, in TArg16>
	{
		Delegate Delegate { get; }
		void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16);
		Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16);
	}

	public class ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16>
    {
        private readonly Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16> operation;

		public Delegate Delegate => operation;

        public ActionWrapper(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16)
        {
            operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16)
        {
            try
            {
                operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
                return TaskCompatibility.CompletedTask;
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException(ex);
            }
		}

		public static implicit operator Delegate(ActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16> actionWrapper) => actionWrapper.Delegate;
    }

	public class AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16> : IActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, Task> operation;
        
		public Delegate Delegate => operation;

        public AsyncActionWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, Task> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public void Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16)
        {
            AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
        }

        public Task InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

		public static implicit operator Delegate(AsyncActionWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16> actionWrapper) => actionWrapper.Delegate;
    }

}