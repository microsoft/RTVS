using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.IO;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExportsTest {
        [TestMethod]
        [TestCategory("Exports")]
        public void FileSystem_ExportTest() {
            AppShell.Current = TestAppShell.Current;
            Lazy<IFileSystem> lazy = AppShell.Current.ExportProvider.GetExport<IFileSystem>();
            Assert.IsNotNull(lazy.Value);
        }

        [TestMethod]
        [TestCategory("Exports")]
        public void RSessionProvider_ExportTest() {
            AppShell.Current = TestAppShell.Current;
            Lazy<IRSessionProvider> lazy = AppShell.Current.ExportProvider.GetExport<IRSessionProvider>();
            Assert.IsNotNull(lazy.Value);
        }

        [TestMethod]
        [TestCategory("Exports")]
        public void ReplHistoryProvider_ExportTest() {
            AppShell.Current = TestAppShell.Current;
            Lazy<IRHistoryProvider> provider = AppShell.Current.ExportProvider.GetExport<IRHistoryProvider>();
            Assert.IsNotNull(provider.Value);
        }
    }
}
