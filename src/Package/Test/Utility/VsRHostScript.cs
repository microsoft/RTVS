using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class VsRHostScript : RHostScript {
        public VsRHostScript() : 
            base(VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>()) {
        }
    }
}
