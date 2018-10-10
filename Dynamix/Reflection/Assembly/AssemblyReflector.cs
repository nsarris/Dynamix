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
        public static List<Type> FindTypesInAssemblies(Func<Type, bool> predicate, bool lookInBaseDirectory = true, params string[] extraPaths)
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loadedAssemblyNames = loadedAssemblies.Select(a => a.GetName().FullName).ToList();

            var loadedTypes =
                     loadedAssemblies
                    .Where(x => !x.IsDynamic)
                    .SelectMany(x => { try { return x.GetTypes().Where(t => { try { return predicate(t); } catch { return false; } }); } catch { return Enumerable.Empty<Type>(); } })
                    .ToList();

            var manager = new AssemblyReflectionManager();

            var paths = (lookInBaseDirectory ? new[] { AppDomain.CurrentDomain.BaseDirectory } : Enumerable.Empty<string>())
                .Concat(extraPaths ?? Enumerable.Empty<string>());

            foreach (var folder in paths)
            {
                if (System.IO.Directory.Exists(folder))
                {
                    foreach (var fileName in System.IO.Directory.GetFiles(folder))
                    {
                        var file = new System.IO.FileInfo(fileName);
                        if (file.Extension == ".dll")
                        {
                            try
                            {
                                var assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                                if (!loadedAssemblyNames.Any(x => x == assemblyName.FullName))
                                {
                                    var results = manager.Reflect(file.FullName, (a) =>
                                    {
                                        return a.GetTypes();
                                    });

                                    if (results.Any(predicate))
                                    {
                                        loadedTypes.AddRange(Assembly.LoadFrom(file.FullName).GetTypes().Where(predicate));
                                    }

                                    manager.UnloadAssembly(fileName);
                                }
                            }
                            catch {
                                //Ignore assemblies that can't be loaded, perhaps log them
                            }
                        }
                    }
                }
            }

            return loadedTypes;
        }

        public static Type FindTypeInAssembly(string assemblyName, string typeName, bool lookInBaseDirectory = true, params string[] extraPaths)
        {
            return FindTypesInAssembly(assemblyName, x => x.Name == typeName, lookInBaseDirectory, extraPaths).FirstOrDefault();
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
            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => !x.IsDynamic && x.GetName().Name == name);

            if (loadedAssembly != null)
                return loadedAssembly;

            var paths = (lookInBaseDirectory ? new[] { AppDomain.CurrentDomain.BaseDirectory } : Enumerable.Empty<string>())
                .Concat(extraPaths ?? Enumerable.Empty<string>());

            foreach (var folder in paths)
            {
                if (System.IO.Directory.Exists(folder))
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
                        catch { }
                    }
                }
            }
            return null;
        }
        public static void LoadAllAssemblies()
        {
            LoadAllAssemblies(AppDomain.CurrentDomain.BaseDirectory);
        }
        public static void LoadAllAssemblies(string path)
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
               .Where(x => !x.IsDynamic).Select(x => x.GetName()).ToList();

            foreach (var fileName in System.IO.Directory.GetFiles(path))
            {
                var file = new System.IO.FileInfo(fileName);
                if (file.Extension == ".dll")
                {
                    var assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                    if (loadedAssemblies.Any(x => x.FullName != assemblyName.FullName))
                    {
                        try
                        {
                            Assembly.LoadFrom(file.FullName);
                        }
                        catch { }
                    }
                }
            }
        }      
    }
}