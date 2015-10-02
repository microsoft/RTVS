using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.VisualStudio.Shell.Mocks
{
    public sealed class ComponentModelMock : IComponentModel
    {
        private ITestCompositionCatalog _catalog;

        public ComponentModelMock(ITestCompositionCatalog catalog)
        {
            _catalog = catalog;
        }

        public ComposablePartCatalog DefaultCatalog
        {
            get
            {
                return _catalog.Container.Catalog;
            }
        }

        public ICompositionService DefaultCompositionService
        {
            get
            {
                return _catalog.CompositionService;
            }
        }

        public ExportProvider DefaultExportProvider
        {
            get
            {
                return _catalog.ExportProvider;
            }
        }

        public ComposablePartCatalog GetCatalog(string catalogName)
        {
            return _catalog.Container.Catalog;
        }

        public IEnumerable<T> GetExtensions<T>() where T : class
        {
            return _catalog.ExportProvider.GetExportedValues<T>();
        }

        public T GetService<T>() where T : class
        {
            return _catalog.ExportProvider.GetExport<T>().Value;
        }
    }
}
