using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    public interface ITestCompositionCatalog
    {
        CompositionContainer Container { get; }

        ICompositionService CompositionService { get; }

        ExportProvider ExportProvider { get; }
    }
}
