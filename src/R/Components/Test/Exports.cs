using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Test.StubFactories;

namespace Microsoft.R.Components.Test.UI {
    /// <summary>
    /// Default exports for types that haven't been loaded from assemblies
    /// </summary>
    internal class Exports {
        [Export(typeof(IFileSystem))]
        public IFileSystem FileSystem { get; }

        [Export(typeof(IRSettings))]
        public IRSettings RSettings { get; }

        public Exports() {
            FileSystem = FileSystemStubFactory.CreateDefault();
            RSettings = RSettingsStubFactory.CreateForExistingRPath();
        }
    }
}
