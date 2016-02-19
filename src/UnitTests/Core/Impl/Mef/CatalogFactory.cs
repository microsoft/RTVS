using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core.Mef {
    public static class CatalogFactory {
        public static ComposablePartCatalog FromAssemblies(params string[] assemblies) {
            return new AggregateCatalog(assemblies.Select(a => new AssemblyCatalog(Assembly.Load(a))));
        }

        private static Assembly LoadAssembly(AssemblyName assemblyName) {
            var codeBase = assemblyName.CodeBase;
            try {
                return codeBase != null ? Assembly.LoadFrom(new Uri(codeBase).LocalPath) : Assembly.Load(assemblyName);
            } catch (IOException) {
                return null;
            }
        }
    }
}
