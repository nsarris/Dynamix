using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dynamix.Reflection.DelegateBuilders
{
	public partial class MethodInvokerDelegateBuilder
    {
		public Action<T, TParam1> BuildActionInstance<T, TParam1>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1) }, typeof(void));
        }

        public Func<T, TParam1, TResult> BuildFuncInstance<T, TParam1, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2> BuildActionInstance<T, TParam1, TParam2>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TResult> BuildFuncInstance<T, TParam1, TParam2, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3> BuildActionInstance<T, TParam1, TParam2, TParam3>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14) }, typeof(TResult));
        }
		public Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15> BuildActionInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15>(MethodInfo methodInfo)
        {
            return (Action<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14), typeof(TParam15) }, typeof(void));
        }

        public Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult> BuildFuncInstance<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>(MethodInfo methodInfo)
        {
            return (Func<T, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>)BuildFromTypes(methodInfo, typeof(T), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14), typeof(TParam15) }, typeof(TResult));
        }

		public Action<TParam1> BuildActionStatic<TParam1>(MethodInfo methodInfo)
        {
            return (Action<TParam1>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1) }, typeof(void));
        }
        public Func<TParam1, TResult> BuildFuncStatic<TParam1, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1) }, typeof(TResult));
        }
		public Action<TParam1, TParam2> BuildActionStatic<TParam1, TParam2>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2) }, typeof(void));
        }
        public Func<TParam1, TParam2, TResult> BuildFuncStatic<TParam1, TParam2, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3> BuildActionStatic<TParam1, TParam2, TParam3>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4> BuildActionStatic<TParam1, TParam2, TParam3, TParam4>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14) }, typeof(TResult));
        }
		public Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15> BuildActionStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15>(MethodInfo methodInfo)
        {
            return (Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14), typeof(TParam15) }, typeof(void));
        }
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult> BuildFuncStatic<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>(MethodInfo methodInfo)
        {
            return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>)BuildFromTypes(methodInfo, null, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14), typeof(TParam15) }, typeof(TResult));
        }
	}
}