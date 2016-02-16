using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    public class ExportsTest {
        [Test]
        [Category.Repl]
        public void FileSystem_ExportTest() {
            Lazy<IFileSystem> lazy = VsAppShell.Current.ExportProvider.GetExport<IFileSystem>();
            lazy.Should().NotBeNull();
            lazy.Value.Should().NotBeNull();
        }

        [Test]
        [Category.Repl]
        public void RSessionProvider_ExportTest() {
            Lazy<IRSessionProvider> lazy = VsAppShell.Current.ExportProvider.GetExport<IRSessionProvider>();
            lazy.Should().NotBeNull();
            lazy.Value.Should().NotBeNull();
        }
    }
}
