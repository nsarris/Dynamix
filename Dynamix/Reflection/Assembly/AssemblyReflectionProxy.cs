using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    //Reworked code from https://www.codeproject.com/Articles/453778/Loading-Assemblies-from-Anywhere-into-a-New-AppDom


    internal class AssemblyReflectionProxy
#if NET45
        : MarshalByRefObject
#endif
    {
        private string _assemblyPath;

        public void LoadAssembly(string assemblyPath)
        {
            try
            {
                _assemblyPath = assemblyPath;
                Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            }
            catch (FileNotFoundException)
            {
                // Continue loading assemblies even if an assembly can not be loaded in the new AppDomain.
            }
        }

        public TResult Reflect<TResult>(Func<Assembly, TResult> func)
        {
            var directory = new FileInfo(_assemblyPath).Directory;

            Assembly resolveEventHandler(object s, ResolveEventArgs e) => OnReflectionOnlyResolve(e, directory);
            
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveEventHandler;

            var assembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().FirstOrDefault(a => a.Location.CompareTo(_assemblyPath) == 0);

            var result = func(assembly);

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;

            return result;
        }

        private Assembly OnReflectionOnlyResolve(ResolveEventArgs args, DirectoryInfo directory)
        {
            Assembly loadedAssembly =
                AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                    .FirstOrDefault(
                      asm => string.Equals(asm.FullName, args.Name,
                          StringComparison.OrdinalIgnoreCase));

            if (loadedAssembly != null)
                return loadedAssembly;
        
            var assemblyName = new AssemblyName(args.Name);
            string dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");

            if (File.Exists(dependentAssemblyFilename))
                return Assembly.ReflectionOnlyLoadFrom(
                    dependentAssemblyFilename);
            
            return Assembly.ReflectionOnlyLoad(args.Name);
        }
    }
}
