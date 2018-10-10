using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Dynamix.Reflection
{
    internal class AssemblyResolverDescriptor
    {
        public string AssemblyName { get; internal set; }
        public string OriginalAssembly { get; internal set; }
        public string X86Assembly { get; internal set; }
        public string X64Assembly { get; internal set; }
    }

    public static class AssemblyArchitectureResolver
    {
        private static Dictionary<string, AssemblyResolverDescriptor> resolverDescriptors = new Dictionary<string, AssemblyResolverDescriptor>();

        static AssemblyArchitectureResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, arg) =>
            {
                foreach (var assembly in resolverDescriptors.Values)
                {
                    if (arg.Name == assembly.AssemblyName || arg.Name.StartsWith(assembly.AssemblyName + ",", StringComparison.OrdinalIgnoreCase))
                        return Assembly.LoadFile(IntPtr.Size == 4 ? assembly.X86Assembly : assembly.X64Assembly);
                }

                return null;
            };
        }

        internal static AssemblyResolverDescriptor GetResolverDescriptor(string assemblyName)
        {
            return resolverDescriptors.TryGetValue(assemblyName, out var descriptor) ? descriptor : null;
        }

        /// <summary>
        /// This will override the resolving of the specified assemblies with the correct architecture. The resolved files should be placed in x86 and x64 folders respectively.
        /// For this to work add pre build commands to copy the files in the afformentioned folders and post build commands to delete the original reference
        /// etc. PreBuild
        ///     xcopy /y "C:\Program Files\IBM\SQLLIB\BIN\netf40\IBM.Data.DB2.dll" "$(TargetDir)x64\"
        ///     xcopy /y "C:\Program Files\IBM\SQLLIB\BIN\netf40_32\IBM.Data.DB2.dll" "$(TargetDir)x86\"
        /// etc. PostBuild
        ///     del /q "$(TargetDir)IBM.Data.DB2.dll"
        /// </summary>
        /// <param name="assemblyName"></param>
        public static void ResolveArchitectureAssemblies(params string[] assemblyNames)
        {
            ResolveArchitectureAssemblies(assemblyNames as IEnumerable<string>);
        }



        public static void ResolveArchitectureAssemblies(IEnumerable<string> assemblyNames)
        {
            foreach (var assemblyName in assemblyNames)
            {
                if (!resolverDescriptors.ContainsKey(assemblyName))
                {
                    var executingAssembly = FindFile(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                    var assemblyDir = Path.GetDirectoryName(executingAssembly);

                    var a = new AssemblyResolverDescriptor
                    {
                        AssemblyName = assemblyName,
                        OriginalAssembly = Path.Combine(assemblyDir, assemblyName + ".dll"),
                        X86Assembly = Path.Combine(assemblyDir, "x86", assemblyName + ".dll"),
                        X64Assembly = Path.Combine(assemblyDir, "x64", assemblyName + ".dll")
                    };

                    resolverDescriptors.Add(a.AssemblyName, a);

                    if (assemblyDir != null && (File.Exists(a.OriginalAssembly)))
                    {
                        throw new InvalidOperationException(string.Format("Found {0}.dll which cannot exist. "
                            + "Must instead have x86\\{0}.dll and x64\\{0}.dll. Check your build settings.", assemblyName));
                    }
                }
            }
        }

        private static string FindFile(string directory, string filename)
        {
            var file = string.Empty;
            foreach (string f in Directory.GetFiles(directory))
            {
                if (Path.GetFileName(f) == filename)
                    return f;

                foreach (string d in Directory.GetDirectories(directory))
                    if (string.IsNullOrEmpty(file))
                        file = FindFile(d, filename);
                    else
                        break;
            }
            if (string.IsNullOrEmpty(file))
                return null;
            else
                return file;
        }

    }
}
