using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Common.Core.Shell {
    [Export(typeof(IAppShellInitialization))]
    public sealed class AppShell: IAppShellInitialization {
        private static IAppCompositionCatalog _catalog;
        private static IApplicationShell _instance;
        private static object _lock = new object();

        public static IApplicationShell Current {
            get {
                lock(_lock) {
                    if (_catalog != null) {
                        var shellProvider = _catalog.ExportProvider.GetExportedValue<IApplicationShellProvider>();
                        _instance = shellProvider.Shell as IApplicationShell;
                    }
                    else {
                        TryCreateTestInstance();
                    }

                    if(_instance == null) {
                        throw new InvalidOperationException("Unable to obtain application shell services");
                    }
                    return _instance;
                }
            }
        }

        public void SetCompositionCatalog(IAppCompositionCatalog catalog) {
            lock(_lock) {
                _catalog = catalog;
            }
        }

        private static void TryCreateTestInstance() {
            AppDomain ad = AppDomain.CurrentDomain;
            Assembly[] loadedAssemblies = ad.GetAssemblies();

            Assembly testAssembly = loadedAssemblies.FirstOrDefault((asm) => {
                AssemblyName assemblyName = asm.GetName();
                string name = assemblyName.Name;
                return name.IndexOf("R.Package.Test", StringComparison.OrdinalIgnoreCase) >= 0;
            });

            if (testAssembly != null) {
                Type[] types = testAssembly.GetTypes();
                IEnumerable<Type> classes = types.Where(x => x.IsClass);

                Type compositionCatalogType = classes.FirstOrDefault(c => c.Name == "TestCompositionCatalog");
                Debug.Assert(compositionCatalogType != null);

                Type providerType = classes.FirstOrDefault(c => 
                    c.GetInterfaces()
                    .FirstOrDefault(i => i == typeof(IApplicationShellProvider)) != null);
                Debug.Assert(providerType != null);

                var catalog = testAssembly.CreateInstance(compositionCatalogType.Name) as IAppCompositionCatalog;
                Debug.Assert(catalog != null);

                var shellInit = testAssembly.CreateInstance(providerType.Name) as IAppShellInitialization;
                Debug.Assert(shellInit != null);

                shellInit.SetCompositionCatalog(catalog);
            }
        }
    }
}
