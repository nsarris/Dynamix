using System;
using System.Threading.Tasks;
using Dynamix.Helpers;

namespace Dynamix.Reflection
{
	public interface IFuncWrapper<TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke();
		Task<TResult> InvokeAsync();
	}

	public class FuncWrapper<TResult> : IFuncWrapper<TResult>
    {
        private readonly Func<TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke()
        {
            return operation();
        }

        public Task<TResult> InvokeAsync()
        {
			try
            {
                return Task.FromResult(operation());
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TResult> : IFuncWrapper<TResult>
    {
        private readonly Func<Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke()
        {
            return AsyncTaskHelper.RunSync(() => operation());
        }

        public Task<TResult> InvokeAsync()
        {
            return operation();
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1);
		Task<TResult> InvokeAsync(TArg1 arg1);
	}

	public class FuncWrapper<TArg1, TResult> : IFuncWrapper<TArg1, TResult>
    {
        private readonly Func<TArg1, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1)
        {
            return operation(arg1);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1)
        {
			try
            {
                return Task.FromResult(operation(arg1));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TResult> : IFuncWrapper<TArg1, TResult>
    {
        private readonly Func<TArg1, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1)
        {
            return operation(arg1);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2);
	}

	public class FuncWrapper<TArg1, TArg2, TResult> : IFuncWrapper<TArg1, TArg2, TResult>
    {
        private readonly Func<TArg1, TArg2, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2)
        {
            return operation(arg1, arg2);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TResult> : IFuncWrapper<TArg1, TArg2, TResult>
    {
        private readonly Func<TArg1, TArg2, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2)
        {
            return operation(arg1, arg2);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return operation(arg1, arg2, arg3);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return operation(arg1, arg2, arg3);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return operation(arg1, arg2, arg3, arg4);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return operation(arg1, arg2, arg3, arg4);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            return operation(arg1, arg2, arg3, arg4, arg5);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            return operation(arg1, arg2, arg3, arg4, arg5);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12, in TArg13, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12, in TArg13, in TArg14, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12, in TArg13, in TArg14, in TArg15, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public interface IFuncWrapper<in TArg1, in TArg2, in TArg3, in TArg4, in TArg5, in TArg6, in TArg7, in TArg8, in TArg9, in TArg10, in TArg11, in TArg12, in TArg13, in TArg14, in TArg15, in TArg16, TResult>
	{
		Delegate Delegate { get; }
		TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16);
		Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16);
	}

	public class FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult> operation;

		public Delegate Delegate => operation;

        public FuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16)
        {
			try
            {
                return Task.FromResult(operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
            }
            catch (Exception ex)
            {
                return TaskCompatibility.FromException<TResult>(ex);
            }
        }

		public static implicit operator Delegate(FuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult> funcWrapper) => funcWrapper.Delegate;
    }

	public class AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult> : IFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, Task<TResult>> operation;
        
		public Delegate Delegate => operation;

        public AsyncFuncWrapper(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, Task<TResult>> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public TResult Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16)
        {
            return AsyncTaskHelper.RunSync(() => operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
        }

        public Task<TResult> InvokeAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15, TArg16 arg16)
        {
            return operation(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

		public static implicit operator Delegate(AsyncFuncWrapper<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult> funcWrapper) => funcWrapper.Delegate;
    }

}