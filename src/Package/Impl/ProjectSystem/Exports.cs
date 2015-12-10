using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {

    [AppliesTo("RTools")]
    internal sealed class Export {

        [Export(typeof(IFileSystem))]
        private IFileSystem FileSystem { get; }

        public Export() {
            FileSystem = new FileSystem();
        }
    }
}