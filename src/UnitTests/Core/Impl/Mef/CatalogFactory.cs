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
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.UnitTests.Core.Mef {
    public static class CatalogFactory {
        public static AggregateCatalog CreateAssembliesCatalog(IEnumerable<string> assemblies) {
            var aggregateCatalog = new AggregateCatalog();
            var assembliesList = new List<string>(assemblies);

            for (var i = assembliesList.Count - 1; i >= 0; i--) {
                var loadedAssembly = GetLoadedAssembly(assembliesList[i]) ?? LoadAssembly(assembliesList[i]);
                if (loadedAssembly != null) {
                    aggregateCatalog.Catalogs.Add(new AssemblyCatalog(loadedAssembly));
                    assembliesList.RemoveAt(i);
                }
            }

            if (assembliesList.Count > 0) {
                throw new InvalidOperationException($@"Can't load assemblies:
    {string.Join("    ", assembliesList)}.
Please use {nameof(AssemblyLoaderAttribute)}-derived types to provide paths for assembly resolution.");
            }

            //ValidateCatalog(aggregateCatalog);
            return aggregateCatalog;
        }

        private static Assembly GetLoadedAssembly(string loadedAssembly) {
            if (Path.GetExtension(loadedAssembly).EqualsIgnoreCase(".dll")) {
                loadedAssembly = Path.GetFileNameWithoutExtension(loadedAssembly);
            }

            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.EqualsIgnoreCase(loadedAssembly));
        }

        private static Assembly LoadAssembly(string assemblyFile) {
            var assemblyName = Path.GetExtension(assemblyFile).EqualsIgnoreCase(".dll") 
                ? Path.GetFileNameWithoutExtension(assemblyFile) ?? assemblyFile
                : assemblyFile;

            try {
                return Assembly.Load(assemblyName);
            } catch (FileLoadException) {
                return null;
            }
        }

        private static void ValidateCatalog(AggregateCatalog catalog) {
            foreach (var composablePartDefinition in catalog.Parts) {
                composablePartDefinition.ExportDefinitions.Should().NotBeNull();
                composablePartDefinition.ImportDefinitions.Should().NotBeNull();
            }
        }
    }
}
