using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dynamix.Reflection.DelegateBuilders
{
	public partial class ConstructorInvokerDelegateBuilder
    {
		public Func<TParam1, TResult> Build<TParam1, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1) });
		}
		public Func<TParam1, TParam2, TResult> Build<TParam1, TParam2, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2) });
		}
		public Func<TParam1, TParam2, TParam3, TResult> Build<TParam1, TParam2, TParam3, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TResult> Build<TParam1, TParam2, TParam3, TParam4, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14), typeof(TParam15) });
		}
		public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult> Build<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>(ConstructorInfo ctorInfo)
		{
			return (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>)BuildFromTypes(ctorInfo, typeof(TResult), new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10), typeof(TParam11), typeof(TParam12), typeof(TParam13), typeof(TParam14), typeof(TParam15), typeof(TParam16) });
		}
	}
}