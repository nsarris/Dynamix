using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public static class AssemblyReflector
    {
#if NET45
        static AssemblyReflectionManager manager = new AssemblyReflectionManager();
#endif

        public static string GetSearchPath()
        {
            return (AppDomain.CurrentDomain.RelativeSearchPath == null)
                    ? AppDomain.CurrentDomain.BaseDirectory
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath);
        }

        public static string GetBasePath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static IEnumerable<Assembly> GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public static List<Type> FindTypesInAssemblies(Func<Type, bool> predicate, bool lookInBaseDirectory = true, params string[] extraPaths)
        {
            var loadedAssemblies = GetLoadedAssemblies().ToList();

            var loadedTypes =
                     loadedAssemblies
                    .Where(x => !x.IsDynamic)
                    .SelectMany(x => { try { return x.GetTypes().Where(t => { try { return predicate(t); } catch { return false; } }); } catch { return Enumerable.Empty<Type>(); } })
                    .ToList();

            var loadedAssemblyNames = loadedAssemblies.Select(a => a.GetName().FullName).ToList();

            var paths = (lookInBaseDirectory ? new[] { GetBasePath() } : Enumerable.Empty<string>())
                .Concat(extraPaths ?? Enumerable.Empty<string>());

            foreach (var folder in paths)
            {
                if (Directory.Exists(folder))
                {
                    foreach (var fileName in Directory.GetFiles(folder))
                    {
                        var file = new FileInfo(fileName);
                        if (file.Extension == ".dll")
                        {
                            try
                            {
                                var assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                                if (!loadedAssemblyNames.Any(x => x == assemblyName.FullName)
                                 && ReflectionOnlyQuery(file.FullName, a => a.GetTypes().Any(predicate)))
                                {
                                    loadedTypes.AddRange(Assembly.LoadFrom(file.FullName).GetTypes().Where(predicate));
                                }
                            }
                            catch
                            {
                                //Ignore assemblies that can't be loaded, perhaps log them
                            }
                        }
                    }
                }
            }

            return loadedTypes;
        }

        public static TResult ReflectionOnlyQuery<TResult>(string assemblyPath, Func<Assembly, TResult> query)
        {
#if NET45
            if (manager.LoadAssembly(assemblyPath, "TypeSearchDomain"))
            {
                var results = manager.Reflect(assemblyPath, query);

                manager.UnloadAssembly(assemblyPath);

                return results;
            }
            return default;
#else
            var proxy = new AssemblyReflectionProxy();
            proxy.LoadAssembly(assemblyPath);
            return proxy.Reflect(query);
#endif
        }

        public static Type FindTypeInAssembly(string assemblyName, string typeName, bool lookInBaseDirectory = true, params string[] extraPaths)
        {
            var assembly = FindOrLoadAssembly(assemblyName, lookInBaseDirectory, extraPaths);
            if (assembly == null)
                throw new InvalidOperationException("Cannot load assembly " + assemblyName);

            return assembly.GetType(typeName);
        }

        public static List<Type> FindTypesInAssembly(string assemblyName, Func<Type, bool> predicate, bool lookInBaseDirectory = true, params string[] extraPaths)
        {
            var assembly = FindOrLoadAssembly(assemblyName, lookInBaseDirectory, extraPaths);
            if (assembly == null)
                throw new InvalidOperationException("Cannot load assembly " + assemblyName);

            return assembly.GetTypes().Where(predicate).ToList();
        }

        public static Assembly FindOrLoadAssembly(string name, bool lookInBaseDirectory = true, params string[] extraPaths)
        {
            var loadedAssembly = GetLoadedAssemblies()
                .FirstOrDefault(x => !x.IsDynamic && x.GetName().Name == name);

            if (loadedAssembly != null)
                return loadedAssembly;

            var paths = (lookInBaseDirectory ? new[] { GetBasePath() } : Enumerable.Empty<string>())
                .Concat(extraPaths ?? Enumerable.Empty<string>());

            foreach (var folder in paths)
            {
                if (Directory.Exists(folder))
                {
                    foreach (var fileName in Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories))
                    {
                        try
                        {
                            var assemblyName = AssemblyName.GetAssemblyName(fileName);
                            if (assemblyName.Name == name &&
                                (assemblyName.ProcessorArchitecture == ProcessorArchitecture.MSIL
                                || (assemblyName.ProcessorArchitecture == ProcessorArchitecture.X86 && !Environment.Is64BitProcess)
                                || (assemblyName.ProcessorArchitecture == ProcessorArchitecture.Amd64 && Environment.Is64BitProcess)))
                            {
                                return Assembly.LoadFrom(fileName);
                            }
                        }
                        catch {
                            //TODO: Add option to throw or consider return as out param
                        }
                    }
                }
            }
            return null;
        }

        public static void LoadAllAssemblies()
        {
            LoadAllAssemblies(GetBasePath());
        }

        public static void LoadAllAssemblies(string path)
        {
            var loadedAssemblies = GetLoadedAssemblies()
               .Where(x => !x.IsDynamic).Select(x => x.GetName()).ToList();

            foreach (var fileName in Directory.GetFiles(path))
            {
                var file = new FileInfo(fileName);
                if (file.Extension == ".dll")
                {
                    var assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                    if (loadedAssemblies.Any(x => x.FullName != assemblyName.FullName))
                    {
                        try
                        {
                            Assembly.LoadFrom(file.FullName);
                        }
                        catch {
                            //TODO: Add option to throw or consider return as out param
                        }
                    }
                }
            }
        }
    }
}