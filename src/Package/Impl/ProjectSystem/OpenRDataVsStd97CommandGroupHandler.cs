using System.ComponentModel.Composition;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [ExportCommandGroup("5EFC7975-14BC-11CF-9B2B-00AA00573819")]
    [AppliesTo("RTools")]
    [OrderPrecedence(100)]
    internal sealed class OpenRDataVsStd97CommandGroupHandler : OpenRDataCommandGroupHandler {
        [ImportingConstructor]
        public OpenRDataVsStd97CommandGroupHandler(UnconfiguredProject unconfiguredProject, IRSessionProvider sessionProvider)
            : base(unconfiguredProject, sessionProvider, (long)VSConstants.VSStd97CmdID.Open) {}
    }
}