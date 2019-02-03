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
        private static readonly string[] assemblyExtensions = new[] { "dll", "exe" };

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

        private static IEnumerable<string> FindAssembliesInPath(string path, bool recursive)
            => FindAssembliesInPaths(new[] { path }, recursive);

        private static IEnumerable<string> FindAssembliesInPaths(IEnumerable<string> paths, bool recursive)
        {
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return paths
                    .Where(folder => Directory.Exists(folder))
                    .SelectMany(folder =>
                        assemblyExtensions
                        .SelectMany(extension =>
                            Directory.GetFiles(folder, $"*.{extension}", searchOption)))
                    .Distinct()
                    .ToList();
        }

        private static IEnumerable<string> GetLookupPaths(bool lookInBaseDirectory, string[] extraPaths)
            => (lookInBaseDirectory ? new[] { GetBasePath() } : Enumerable.Empty<string>())
                .Concat(extraPaths ?? Enumerable.Empty<string>());

        public static List<Type> FindTypesInAssemblies(Func<Type, bool> predicate, bool lookInBaseDirectory = true, bool recursive = true, params string[] extraPaths)
            => FindTypesInAssemblies(predicate, null, lookInBaseDirectory, recursive, extraPaths);

        public static List<Type> FindTypesInAssemblies(Func<Type, bool> predicate, Func<AssemblyName, bool> assemblyPredicate, bool lookInBaseDirectory = true, bool recursive = true, params string[] extraPaths)
        {
            var loadedAssemblies = GetLoadedAssemblies().ToList();

            var loadedTypes =
                     loadedAssemblies
                    .Where(x => !x.IsDynamic)
                    .Where(x => assemblyPredicate == null || assemblyPredicate(x.GetName()))
                    .SelectMany(x => { try { return x.GetTypes().Where(t => { try { return predicate(t); } catch { return false; } }); } catch { return Enumerable.Empty<Type>(); } })
                    .ToList();

            var loadedAssemblyNames = loadedAssemblies.Select(a => a.GetName().FullName).ToList();

            var paths = GetLookupPaths(lookInBaseDirectory, extraPaths);

            foreach (var file in FindAssembliesInPaths(paths, recursive).Select(x => new FileInfo(x)))
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                    if ((assemblyPredicate == null || assemblyPredicate(assemblyName))
                        && !loadedAssemblyNames.Any(x => x == assemblyName.FullName)
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

        public static Type FindTypeInAssembly(string assemblyName, string typeName, bool lookInBaseDirectory = true, bool recursive = true, params string[] extraPaths)
        {
            return FindTypesInAssemblies(x => x.Name == typeName, x => x.Name == assemblyName, lookInBaseDirectory, recursive, extraPaths).FirstOrDefault();
        }

        public static List<Type> FindTypesInAssembly(string assemblyName, Func<Type, bool> predicate, bool lookInBaseDirectory = true, bool recursive = true, params string[] extraPaths)
        {
            return FindTypesInAssemblies(predicate, x => x.Name == assemblyName, lookInBaseDirectory, recursive, extraPaths);
        }

        public static Assembly FindOrLoadAssembly(string name, bool lookInBaseDirectory = true, bool recursive = true, params string[] extraPaths)
        {
            var loadedAssembly = GetLoadedAssemblies()
                .FirstOrDefault(x => !x.IsDynamic && x.GetName().Name == name);

            if (loadedAssembly != null)
                return loadedAssembly;

            var paths = (lookInBaseDirectory ? new[] { GetBasePath() } : Enumerable.Empty<string>())
                .Concat(extraPaths ?? Enumerable.Empty<string>());

            foreach (var fileName in FindAssembliesInPaths(paths, recursive))
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
                catch
                {
                    //TODO: Add option to throw or consider return as out param
                }
            }

            return null;
        }

        public static void LoadAllAssemblies(bool recursive = true)
        {
            LoadAllAssemblies(GetBasePath(), recursive);
        }

        public static void LoadAllAssemblies(string path, bool recursive = true)
        {
            var loadedAssemblies = GetLoadedAssemblies()
               .Where(x => !x.IsDynamic).Select(x => x.GetName()).ToList();

            foreach (var file in FindAssembliesInPath(path, recursive).Select(x => new FileInfo(x)))
            {
                var assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                if (loadedAssemblies.Any(x => x.FullName != assemblyName.FullName))
                {
                    try
                    {
                        Assembly.LoadFrom(file.FullName);
                    }
                    catch
                    {
                        //TODO: Add option to throw or consider return as out param
                    }
                }
            }
        }
    }
}