using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem
{
    [AppliesTo("RTools")]
    internal sealed class FileSystemExport
    {
        [Export(typeof(IFileSystem))]
        private IFileSystem FileSystem { get; } = new FileSystem();
    }
}