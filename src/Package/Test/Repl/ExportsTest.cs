using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.IO;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExportsTest {
        [TestMethod]
        public void FileSystem_ExportTest() {
            VsAppShell.Current = TestAppShell.Current;
            Lazy<IFileSystem> lazy = VsAppShell.Current.ExportProvider.GetExport<IFileSystem>();
            Assert.IsNotNull(lazy.Value);
        }

        [TestMethod]
        public void RSessionProvider_ExportTest() {
            VsAppShell.Current = TestAppShell.Current;
            Lazy<IRSessionProvider> lazy = VsAppShell.Current.ExportProvider.GetExport<IRSessionProvider>();
            Assert.IsNotNull(lazy.Value);
        }

        [TestMethod]
        public void ReplHistoryProvider_ExportTest() {
            VsAppShell.Current = TestAppShell.Current;
            Lazy<IRHistoryProvider> provider = VsAppShell.Current.ExportProvider.GetExport<IRHistoryProvider>();
            Assert.IsNotNull(provider.Value);
        }
    }
}
