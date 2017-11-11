// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core.XUnit {
    [AttributeUsage(AttributeTargets.Assembly)]
    public abstract class AssemblyLoaderAttribute : Attribute, IDisposable {
        private readonly string[] _paths;
        private readonly string[] _assembliesToResolve;

        public static IList<AssemblyLoaderAttribute> GetAssemblyLoaders(IAssemblyInfo assemblyInfo) {
            return assemblyInfo.GetCustomAttributes(typeof(AssemblyLoaderAttribute))
                .OfType<IReflectionAttributeInfo>()
                .Select(ai => ai.Attribute)
                .OfType<AssemblyLoaderAttribute>()
                .ToList();
        }

        private readonly Dictionary<string, List<string>> _knownAssemblies = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        protected AssemblyLoaderAttribute(string[] paths, string[] assembliesToResolve) {
            if (paths == null) {
                throw new ArgumentNullException(nameof(paths));
            }

            if (paths.Length == 0) {
                throw new ArgumentException($"{nameof(paths)} should not be empty", nameof(paths));
            }
            _paths = paths;
            _assembliesToResolve = assembliesToResolve;
        }

        public void Initialize() {
            foreach (var path in _paths.Concat(new [] {Paths.Bin})) {
                EnumerateAssemblies(path);
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (var assemblyName in _assembliesToResolve) {
                ResolveAssembly(assemblyName, AssemblyLoad);
            }
        }

        public void Dispose() {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var assemblyName = new AssemblyName(args.Name).Name;
            return ResolveAssembly(assemblyName, AssemblyLoadFrom);
        }

        private Assembly ResolveAssembly(string assemblyName, Func<string, string, Assembly> assemblyLoader) {
            if (!Path.GetExtension(assemblyName).EqualsOrdinal(".dll")) {
                assemblyName += ".dll";
            }

            if (!_knownAssemblies.TryGetValue(assemblyName, out var assemblyPaths)) {
                return null;
            }

            foreach (var assemblyPath in assemblyPaths) {
                var assembly = assemblyLoader(assemblyName, assemblyPath);
                if (assembly != null) {
                    return assembly;
                }
            }

            return null;
        }
        
        private static Assembly AssemblyLoad(string assemblyName, string assemblyPath) {
            try {
                return Assembly.Load(new AssemblyName {
                    Name = assemblyName,
                    CodeBase = new Uri(assemblyPath).AbsoluteUri
                });
            } catch (FileLoadException) {
                return null;
            } catch (IOException) {
                return null;
            } catch (BadImageFormatException) {
                return null;
            }
        }

        private static Assembly AssemblyLoadFrom(string assemblyName, string assemblyPath) {
            try {
                return Assembly.LoadFrom(assemblyPath);
            } catch (FileLoadException) {
                return null;
            } catch (IOException) {
                return null;
            } catch (BadImageFormatException) {
                return null;
            }
        }

        private void EnumerateAssemblies(string directory) {
            foreach (var path in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories)) {
                var name = Path.GetFileName(path);
                if (name != null) {
                    List<string> paths;
                    if (_knownAssemblies.TryGetValue(name, out paths)) {
                        paths.Add(path);
                    } else {
                        _knownAssemblies[name] = new List<string>{ path };
                    }
                }
            }
        }
    }
}
