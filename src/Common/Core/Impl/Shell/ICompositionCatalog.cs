using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Microsoft.Common.Core.Shell {
    public interface ICompositionCatalog {
        /// <summary>
        /// Host application MEF composition service.
        /// </summary>
        ICompositionService CompositionService { get; }

        /// <summary>
        /// Visual Studio MEF export provider.
        /// </summary>
        ExportProvider ExportProvider { get; }
    }
}
