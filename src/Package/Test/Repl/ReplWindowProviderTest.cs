using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ReplWindowProviderTest {
        [TestMethod]
        [TestCategory("Repl")]
        public void ReplWindowProvider_ConstructionTest() {
            AppShell.Current = TestAppShell.Current;
            RInteractiveWindowProvider provider = new RInteractiveWindowProvider();
        }
    }
}
