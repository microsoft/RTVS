using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using Microsoft.Languages.Editor.Test.Composition;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks
{
    public sealed class ComponentModelMock : IComponentModel
    {
        public ComposablePartCatalog DefaultCatalog
        {
            get
            {
                return TestCompositionCatalog.Container.Catalog;
            }
        }

        public ICompositionService DefaultCompositionService
        {
            get
            {
                return TestCompositionCatalog.CompositionService;
            }
        }

        public ExportProvider DefaultExportProvider
        {
            get
            {
                return TestCompositionCatalog.ExportProvider;
            }
        }

        public ComposablePartCatalog GetCatalog(string catalogName)
        {
            return TestCompositionCatalog.Container.Catalog;
        }

        public IEnumerable<T> GetExtensions<T>() where T : class
        {
            return TestCompositionCatalog.ExportProvider.GetExportedValues<T>();
        }

        public T GetService<T>() where T : class
        {
            return TestCompositionCatalog.ExportProvider.GetExport<T>().Value;
        }
    }
}
