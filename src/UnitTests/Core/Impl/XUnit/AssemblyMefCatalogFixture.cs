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

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public abstract class AssemblyMefCatalogFixture : MefCatalogFixture {
        private readonly Dictionary<string, string> _knownProductAssemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _knownVsAssemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string _idePath;

        protected override ComposablePartCatalog CreateCatalog() {
            _idePath = GetDevEnvIdePath();

            EnumerateAssemblies(_knownProductAssemblyPaths, Path.GetDirectoryName(GetType().Assembly.GetAssemblyPath()));
            EnumerateAssemblies(_knownVsAssemblyPaths, Path.Combine(_idePath, @"PrivateAssemblies\"));
            EnumerateAssemblies(_knownVsAssemblyPaths, Path.Combine(_idePath, @"CommonExtensions\"));

            var aggregateCatalog = new AggregateCatalog();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            try {
                var nugetAssemblies = GetNugetAssemblies().ToList();
                if (nugetAssemblies.Any()) {
                    FindInCurrentAssemblyFolder(nugetAssemblies, aggregateCatalog);
                }

                foreach (var assemblyName in GetBinDirectoryAssemblies()) {
                    LoadAssembly(assemblyName, _knownProductAssemblyPaths, aggregateCatalog);
                }

                foreach (var assemblyName in GetVsAssemblies()) {
                    LoadAssembly(assemblyName, _knownVsAssemblyPaths, aggregateCatalog);
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
        protected virtual IEnumerable<string> GetNugetAssemblies() => Enumerable.Empty<string>();
        protected virtual IEnumerable<string> GetVsAssemblies() => Enumerable.Empty<string>();

        private List<string> FindInCurrentAssemblyFolder(IEnumerable<string> assemblyNames, AggregateCatalog catalog) {
            var directory = Path.GetDirectoryName(GetType().Assembly.GetAssemblyPath());
            var unresolved = new List<string>();

            foreach (var assemblyName in assemblyNames) {
                var path = Path.Combine(directory, assemblyName);
                try {
                    var assembly = Assembly.LoadFrom(path);
                    catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                } catch (IOException) {
                    unresolved.Add(assemblyName);
                }
            }

            return unresolved;
        }
        
        private static void EnumerateAssemblies(Dictionary<string, string> assemblyPaths, string directory) {
            foreach (var path in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories)) {
                var name = Path.GetFileName(path);
                if (name != null && !assemblyPaths.ContainsKey(name)) {
                    assemblyPaths[name] = path;
                }
            }
        }

        private static void LoadAssembly(string assemblyFile, Dictionary<string, string> knownAssemblyPaths, AggregateCatalog catalog) {
            string assemblyPath;
            if (!knownAssemblyPaths.TryGetValue(assemblyFile, out assemblyPath)) {
                throw new FileNotFoundException(assemblyFile);
            }

            var assembly = Assembly.LoadFrom(assemblyPath);
            catalog.Catalogs.Add(new AssemblyCatalog(assembly));
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

        private static string GetDevEnvIdePath() {
            return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\14.0", "InstallDir", string.Empty);
        }
    }
}