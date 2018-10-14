using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    internal static class EntityFrameworkHelper
    {
        private class EFMethods
        {
            public MethodInfo Set;
            public MethodInfo SetOfT;
        }

        static EFMethods efmethods;
        static readonly object eflock = new object();
        public static DynamicQueryable FromDbContextSet(object dbContext, Type entityType, bool StateTracking = false)
        {
            //DbSet<T> not working properly, disabling StateTracking by default
            if (efmethods == null)
            {
                lock (eflock)
                {
                    if (efmethods == null)
                    {
                        var setMethods = dbContext.GetType().GetMethods().Where(m => m.Name == "Set")
                            .Select(m => new
                            {
                                Method = m,
                                Params = m.GetParameters(),
                                Args = m.GetGenericArguments()
                            });

                        if (setMethods != null)
                        {
                            efmethods = new EFMethods
                            {
                                SetOfT = setMethods.Single(x => x.Args.Length == 1).Method,
                                Set = setMethods.Single(x => x.Args.Length == 0).Method
                            };
                        }
                    }
                }
            }

            if (efmethods is null || efmethods.Set is null)
                throw new InvalidOperationException("DbSet method not found on DbContext");
            else
            {
                if (StateTracking)
                {
                    return new DynamicQueryable((IQueryable)efmethods.SetOfT.MakeGenericMethod(entityType).Invoke(dbContext, null));
                }
                else
                    return new DynamicQueryable((IQueryable)efmethods.Set.Invoke(dbContext, new[] { entityType }));
            }
        }
    }
}
