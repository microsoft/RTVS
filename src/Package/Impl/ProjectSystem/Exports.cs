using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;

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