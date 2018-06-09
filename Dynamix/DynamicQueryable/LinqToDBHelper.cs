using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    internal static class LinqToDBHeper
    {
        private class LinqToDBMethods
        {
            public MethodInfo GetTableT;
        }

        static LinqToDBMethods linqToDBMethods;
        static readonly object l2dblock = new object();
        public static DynamicQueryable FromLinqToDBDataConnection(object dataConnection, Type modelType)
        {
            if (dataConnection == null)
                throw new ArgumentNullException(nameof(dataConnection));

            if (dataConnection.GetType().FullName != "LinqToDB.Data.DataConnection")
                throw new ArgumentException("Object is not a LinqToDB DataConnection", nameof(dataConnection));

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
                throw new InvalidOperationException("GetTable method not found on DataConnection");
            else
            {
                return new DynamicQueryable((IQueryable)linqToDBMethods.GetTableT.MakeGenericMethodCached(modelType).Invoke(dataConnection, new object[0]));
            }
        }
    }
}
