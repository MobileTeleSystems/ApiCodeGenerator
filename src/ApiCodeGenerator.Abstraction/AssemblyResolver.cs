using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ApiCodeGenerator.Abstraction
{
    internal static class AssemblyResolver
    {
        private static readonly Type assemblyLoadContextType = Type.GetType("System.Runtime.Loader.AssemblyLoadContext, System.Runtime.Loader");
        private static readonly Type assemblyDependencyResolverType = Type.GetType("System.Runtime.Loader.AssemblyDependencyResolver, System.Runtime.Loader");

        private static readonly ConcurrentBag<string> _probingPaths = new();
        private static readonly ConcurrentBag<Func<AssemblyName, string>> _resolvers = new();
        private static readonly ConcurrentDictionary<string, (AssemblyName AssemblyName, string Path)> _asmPaths = new();
        private static object? _assemblyLoadContext;

        public static void AddProbingPath(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (!_probingPaths.Contains(path))
                {
                    _probingPaths.Add(path);
                    foreach (var filePath in Directory.EnumerateFiles(path, "*.dll"))
                    {
                        AssemblyName asmName;
                        try
                        {
                            asmName = AssemblyName.GetAssemblyName(filePath);
                        }
                        catch
                        {
                            continue;
                        }

                        var key = asmName.Name;
                        if (_asmPaths.TryGetValue(key, out var item))
                        {
                            if (item.AssemblyName.Version < asmName.Version)
                            {
                                _asmPaths.TryUpdate(key, (asmName, filePath), item);
                            }
                        }
                        else
                        {
                            _asmPaths.TryAdd(key, (asmName, filePath));
                        }

                        if (assemblyDependencyResolverType != null && File.Exists(Path.ChangeExtension(filePath, ".deps.json")))
                        {
                            var resolver = Activator.CreateInstance(assemblyDependencyResolverType, filePath);
                            var method = assemblyDependencyResolverType.GetMethod("ResolveAssemblyToPath");
                            _resolvers.Add((Func<AssemblyName, string>)method.CreateDelegate(typeof(Func<AssemblyName, string>), resolver));
                        }
                    }
                }
            }
        }

        public static void Register(object? assemblyLoadContext)
        {
            _assemblyLoadContext = assemblyLoadContext;
            if (assemblyLoadContextType is null)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
            else
            {
                var current = GetLoadContext();
                var ev = assemblyLoadContextType.GetEvent("Resolving");
                ev.AddEventHandler(current, AssemblyLoadContext_Resolving);
            }
        }

        public static void Unregister()
        {
            if (assemblyLoadContextType is null)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
            else
            {
                var current = GetLoadContext();
                var ev = assemblyLoadContextType.GetEvent("Resolving");
                ev.RemoveEventHandler(current, AssemblyLoadContext_Resolving);
            }
        }

        public static Assembly? LoadAssembly(AssemblyName assemblyName)
        {
            return assemblyLoadContextType is null
                ? CurrentDomain_AssemblyResolve(AppDomain.CurrentDomain, new ResolveEventArgs(assemblyName.FullName))
                : AssemblyLoadContext_Resolving(GetLoadContext(), assemblyName);
        }

        private static Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var key = new AssemblyName(args.Name).Name;
            if (_asmPaths.TryGetValue(key, out var item))
            {
                var assembly = Assembly.Load(item.AssemblyName);
                return assembly;
            }

            return null;
        }

        private static Assembly? AssemblyLoadContext_Resolving(object context, AssemblyName assemblyName)
        {
            var thisAsm = Assembly.GetCallingAssembly();
            if (assemblyName == thisAsm.GetName())
            {
                return null;
            }

            var path = _resolvers.Select(r => r.Invoke(assemblyName)).FirstOrDefault(p => p != null);

            var key = assemblyName.Name;

            if (path == null && _asmPaths.TryGetValue(key, out var item))
            {
                path = item.Path;
            }

            return path == null
                ? null
                : assemblyLoadContextType.GetMethod("LoadFromAssemblyPath").Invoke(context, new object[] { path }) as Assembly;
        }

        private static object GetLoadContext()
        {
            return _assemblyLoadContext ?? assemblyLoadContextType.GetMethod("GetLoadContext").Invoke(null, new object[] { Assembly.GetCallingAssembly() });
        }
    }
}
