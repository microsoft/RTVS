// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Win32;
using static System.FormattableString;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public abstract class AssemblyMefCatalogFixture : MefCatalogFixture {
        private readonly Dictionary<string, string> _knownVsAssemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string _binDirectoryPath;

        protected override ComposablePartCatalog CreateCatalog() {
            _binDirectoryPath = Paths.Bin;

            EnumerateAssemblies(_knownVsAssemblyPaths, Paths.VsPrivateAssemblies);
            EnumerateAssemblies(_knownVsAssemblyPaths, Paths.VsCommonExtensions);

            try {
                var aggregateCatalog = new AggregateCatalog();

                foreach (var loadedAssembly in GetLoadedAssemblies()) {
                    aggregateCatalog.Catalogs.Add(new AssemblyCatalog(GetLoadedAssembly(loadedAssembly)));
                }

                foreach (var assemblyName in GetBinDirectoryAssemblies()) {
                    aggregateCatalog.Catalogs.Add(new AssemblyCatalog(LoadBinDirectoryAssembly(assemblyName)));
                }

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                foreach (var assemblyName in GetVsAssemblies()) {
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
            return LoadVsAssembly(assemblyFile);
        }

        protected virtual IEnumerable<string> GetBinDirectoryAssemblies() => Enumerable.Empty<string>();
        protected virtual IEnumerable<string> GetLoadedAssemblies() => Enumerable.Empty<string>();
        protected virtual IEnumerable<string> GetVsAssemblies() => Enumerable.Empty<string>();

        private static void EnumerateAssemblies(Dictionary<string, string> assemblyPaths, string directory) {
            foreach (var path in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories)) {
                var name = Path.GetFileName(path);
                if (name != null && !assemblyPaths.ContainsKey(name)) {
                    assemblyPaths[name] = path;
                }
            }
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

            return Assembly.LoadFrom(path);
        }

        private Assembly LoadVsAssembly(string assemblyFile) {
            string assemblyPath;
            return _knownVsAssemblyPaths.TryGetValue(assemblyFile, out assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        }

        private static void ValidateCatalog(AggregateCatalog catalog) {
            foreach (var composablePartDefinition in catalog.Parts) {
                composablePartDefinition.ExportDefinitions.Should().NotBeNull();
                composablePartDefinition.ImportDefinitions.Should().NotBeNull();
            }
        }
    }
}