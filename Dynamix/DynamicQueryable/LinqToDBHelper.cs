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
        static Lazy<Type> linq2dbDataType = new Lazy<Type>(() =>
        AssemblyReflector.FindTypeInAssembly("linq2db", "LinqToDB.Data.DataConnection"));

        public static DynamicQueryable FromLinqToDBDataConnection(object dataConnection, Type modelType)
        {
            if (dataConnection == null)
                throw new ArgumentNullException(nameof(dataConnection));

            Type connectionType = null;
            try
            {
                connectionType = linq2dbDataType.Value;
            }
            catch
            {
                throw new InvalidOperationException("Linq2db is not loaded");
            }

            if (!dataConnection.GetType().IsOrSubclassOf(connectionType))
                throw new ArgumentException("Object is not a LinqToDB DataConnection", nameof(dataConnection));

            if (linqToDBMethods == null)
            {
                lock (l2dblock)
                {
                    if (linqToDBMethods == null)
                    {
                        linqToDBMethods = new LinqToDBMethods()
                        {
                            GetTableT = dataConnection.GetType().GetMethod("GetTable", Type.EmptyTypes)
                        };
                    }
                }
            }

            if (linqToDBMethods.GetTableT == null)
                throw new InvalidOperationException("GetTable method not found on DataConnection");
            else
            {
                return new DynamicQueryable((IQueryable)linqToDBMethods.GetTableT.MakeGenericMethodCached(modelType).Invoke(dataConnection, Constants.EmptyObjectArray));
            }
        }
    }
}
