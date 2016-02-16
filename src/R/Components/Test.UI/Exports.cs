using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Test.Fakes.Undo;
using Microsoft.R.Components.Test.StubFactories;
using Microsoft.R.Components.Test.Stubs;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.R.Components.Test.UI {
    /// <summary>
    /// Default exports for types that haven't been loaded from assemblies
    /// </summary>
    internal class Exports {
        [Export(typeof(ITextUndoHistoryRegistry))]
        public ITextUndoHistoryRegistry TextUndoHistoryRegistry { get; }

        [Export(typeof(IFileSystem))]
        public IFileSystem FileSystem { get; }

        [Export(typeof(IRSettings))]
        public IRSettings RSettings { get; }

        public Exports() {
            TextUndoHistoryRegistry = new UndoHistoryRegistryImpl();
            FileSystem = FileSystemStubFactory.CreateDefault();
            RSettings = RSettingsStubFactory.CreateForExistingRPath();
        }
    }
}
