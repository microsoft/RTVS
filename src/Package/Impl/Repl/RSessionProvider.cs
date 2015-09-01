using System.ComponentModel.Composition;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl
{
    [Export(typeof (IRSessionProvider))]
    [AppliesTo("RTools")]
    internal class VsRSessionProvider : RSessionProvider
    {
    }
}