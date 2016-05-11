// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Common.Core;
using Xunit.Abstractions;

namespace Microsoft.UnitTests.Core.XUnit {
    public abstract class AssemblyLoaderAttribute : Attribute, IDisposable {
        public static IList<AssemblyLoaderAttribute> GetAssemblyLoaders(IAssemblyInfo assemblyInfo) {
            return assemblyInfo.GetCustomAttributes(typeof(AssemblyLoaderAttribute))
                .OfType<IReflectionAttributeInfo>()
                .Select(ai => ai.Attribute)
                .OfType<AssemblyLoaderAttribute>()
                .ToList();
        }

        private readonly string[] _paths;

        protected AssemblyLoaderAttribute(string[] paths, string[] assembliesToResolve) {
            if (paths == null) {
                throw new ArgumentNullException(nameof(paths));
            }

            if (paths.Length == 0) {
                throw new ArgumentException($"{nameof(paths)} should not be empty", nameof(paths));
            }

            _paths = paths;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (var assemblyName in assembliesToResolve) {
                ResolveAssembly(assemblyName);
            }
        }

        public void Dispose() {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            return ResolveAssembly(args.Name);
        }

        private Assembly ResolveAssembly(string name) {
            foreach (var path in _paths) {
                var assembly = ResolveAssembly(path, name);
                if (assembly != null) {
                    return assembly;
                }
            }

            return null;
        }

        private Assembly ResolveAssembly(string directory, string name) {
            var assemblyName = new AssemblyName(name);

            var path = Path.Combine(directory, assemblyName.Name);
            if (!Path.GetExtension(path).EqualsOrdinal(".dll")) {
                path += ".dll";
            }

            try {
                var localAssemblyName = AssemblyName.GetAssemblyName(path);
                return Assembly.Load(localAssemblyName);
            } catch (IOException) {
                return null;
            } catch (BadImageFormatException) {
                return null;
            }
        }
    }
}
