// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Common.Core;
using FluentAssertions;

namespace Microsoft.UnitTests.Core.Mef {
    public static class CatalogFactory {
        public static AggregateCatalog CreateVsAssembliesCatalog(List<string> assemblies)
            => new VsAssembliesCatalogFactory(assemblies).CreateCatalog();
                    
        private static void EnumerateAssemblies(Dictionary<string, string> assemblyPaths, string directory) {
            foreach (var path in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories)) {
                var name = Path.GetFileName(path);
                if (name != null && !assemblyPaths.ContainsKey(name)) {
                    assemblyPaths[name] = path;
                }
            }
        }
        
        private static void ValidateCatalog(AggregateCatalog catalog) {
            foreach (var composablePartDefinition in catalog.Parts) {
                composablePartDefinition.ExportDefinitions.Should().NotBeNull();
                composablePartDefinition.ImportDefinitions.Should().NotBeNull();
            }
        }

        private class VsAssembliesCatalogFactory {
            private readonly List<string> _assemblies;
            private readonly string _binDirectoryPath;
            private readonly Dictionary<string, string> _knownVsAssemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            public VsAssembliesCatalogFactory(List<string> assemblies) {
                _assemblies = assemblies;
                _binDirectoryPath = Paths.Bin;
            }

            public AggregateCatalog CreateCatalog() {
                EnumerateAssemblies(_knownVsAssemblyPaths, Paths.VsPrivateAssemblies);
                EnumerateAssemblies(_knownVsAssemblyPaths, Paths.VsCommonExtensions);

                try {
                    var aggregateCatalog = new AggregateCatalog();

                    // First check for assemblies that are already loaded.
                    // Simply add them to catalog
                    for (var i = _assemblies.Count - 1; i >= 0; i--) {
                        var loadedAssembly = GetLoadedAssembly(_assemblies[i]);
                        if (loadedAssembly != null) {
                            aggregateCatalog.Catalogs.Add(new AssemblyCatalog(loadedAssembly));
                            _assemblies.RemoveAt(i);
                        }
                    }

                    // Then check for assemblies in bin folder
                    for (var i = _assemblies.Count - 1; i >= 0; i--) {
                        var loadedAssembly = LoadBinDirectoryAssembly(_assemblies[i]);
                        if (loadedAssembly != null) {
                            aggregateCatalog.Catalogs.Add(new AssemblyCatalog(loadedAssembly));
                            _assemblies.RemoveAt(i);
                        }
                    }

                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                    foreach (var assemblyName in _assemblies) {
                        aggregateCatalog.Catalogs.Add(new AssemblyCatalog(LoadVsAssembly(assemblyName)));
                    }

                    ValidateCatalog(aggregateCatalog);
                    return aggregateCatalog;
                } finally {
                    AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                }
            }
            
            private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
                var assemblyFile = new AssemblyName(args.Name).Name + ".dll";
                string assemblyPath;

                return _knownVsAssemblyPaths.TryGetValue(assemblyFile, out assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
            }

            private Assembly GetLoadedAssembly(string loadedAssembly) {
                if (Path.GetExtension(loadedAssembly).EqualsIgnoreCase(".dll")) {
                    loadedAssembly = Path.GetFileNameWithoutExtension(loadedAssembly);
                }

                return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.EqualsIgnoreCase(loadedAssembly));
            }

            private Assembly LoadBinDirectoryAssembly(string assemblyFile) {
                var path = Path.Combine(_binDirectoryPath, assemblyFile);
                if (!Path.GetExtension(path).EqualsOrdinal(".dll")) {
                    path += ".dll";
                }


                if (!File.Exists(path)) {
                    return null;
                }

                var assemblyName = assemblyFile;
                if (Path.GetExtension(assemblyName).EqualsIgnoreCase(".dll")) {
                    assemblyName = Path.GetFileNameWithoutExtension(assemblyName);
                }

                return Assembly.Load(assemblyName);
            }

            private Assembly LoadVsAssembly(string assemblyFile) {
                if (_knownVsAssemblyPaths.ContainsKey(assemblyFile)) {
                    return Assembly.Load(Path.GetFileNameWithoutExtension(assemblyFile));
                }

                return null;
            }
        }
    }
}
