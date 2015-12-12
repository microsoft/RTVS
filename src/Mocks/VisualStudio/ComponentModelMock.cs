using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class ComponentModelMock : IComponentModel {
        private ICompositionCatalog _catalog;

        public ComponentModelMock(ICompositionCatalog catalog) {
            _catalog = catalog;
        }

        public ComposablePartCatalog DefaultCatalog {
            get {
                return _catalog as ComposablePartCatalog;
            }
        }

        public ICompositionService DefaultCompositionService {
            get {
                return _catalog.CompositionService;
            }
        }

        public ExportProvider DefaultExportProvider {
            get {
                return _catalog.ExportProvider;
            }
        }

        public ComposablePartCatalog GetCatalog(string catalogName) {
            return _catalog as ComposablePartCatalog;
        }

        public IEnumerable<T> GetExtensions<T>() where T : class {
            return _catalog.ExportProvider.GetExportedValues<T>();
        }

        public T GetService<T>() where T : class {
            return _catalog.ExportProvider.GetExport<T>().Value;
        }
    }
}
