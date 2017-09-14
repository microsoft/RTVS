// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Common.Core;
using Xunit.Abstractions;

namespace Microsoft.UnitTests.Core.XUnit {
    [AttributeUsage(AttributeTargets.Assembly)]
    public abstract class AssemblyLoaderAttribute : Attribute, IDisposable {
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

            foreach (var path in new[] { Paths.Bin }.Concat(paths)) {
                EnumerateAssemblies(path);
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (var assembly in assembliesToResolve) {
                var assemblyName = Path.GetExtension(assembly).EqualsIgnoreCase(".dll")
                    ? Path.GetFileNameWithoutExtension(assembly) ?? assembly
                    : assembly;

                Assembly.Load(assemblyName);
            }
        }

        public void Dispose() {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var assemblyName = new AssemblyName(args.Name).Name;
            if (!Path.GetExtension(assemblyName).EqualsOrdinal(".dll")) {
                assemblyName += ".dll";
            }

            List<string> assemblyPaths;
            if (!_knownAssemblies.TryGetValue(assemblyName, out assemblyPaths)) {
                return null;
            }

            foreach (var assemblyPath in assemblyPaths) {
                var assembly = LoadAssembly(assemblyPath);
                if (assembly != null) {
                    return assembly;
                }
            }

            return null;
        }

        private static Assembly LoadAssembly(string assemblyPath) {
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
