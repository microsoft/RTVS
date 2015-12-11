using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Composition;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class ComponentModelMock : IComponentModel {
        public ComposablePartCatalog DefaultCatalog {
            get {
                return TestCompositionCatalog.Current.Container.Catalog;
            }
        }

        public ICompositionService DefaultCompositionService {
            get {
                return TestCompositionCatalog.Current.CompositionService;
            }
        }

        public ExportProvider DefaultExportProvider {
            get {
                return TestCompositionCatalog.Current.ExportProvider;
            }
        }

        public ComposablePartCatalog GetCatalog(string catalogName) {
            return TestCompositionCatalog.Current.Container.Catalog;
        }

        public IEnumerable<T> GetExtensions<T>() where T : class {
            return TestCompositionCatalog.Current.ExportProvider.GetExportedValues<T>();
        }

        public T GetService<T>() where T : class {
            return TestCompositionCatalog.Current.ExportProvider.GetExport<T>().Value;
        }
    }
}
