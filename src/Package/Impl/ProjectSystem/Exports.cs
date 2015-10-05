using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Repl.Session;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem
{
    [AppliesTo("RTools")]
    internal sealed class Export
    {
        [Export(typeof(IFileSystem))]
        private IFileSystem FileSystem { get; } = new FileSystem();

        [Export(typeof(IRSessionProvider))]
        private IRSessionProvider RSessionProvider { get; } = new RSessionProvider();
    }
}