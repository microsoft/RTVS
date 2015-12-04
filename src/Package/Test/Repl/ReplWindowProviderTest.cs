using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ReplWindowProviderTest {
        [TestMethod]
        public void ReplWindowProvider_ConstructionTest() {
            AppShell.Current = TestAppShell.Current;
            RInteractiveWindowProvider provider = new RInteractiveWindowProvider();
        }

        //[TestMethod]
        [TestCategory("Repl")]
        public void ReplWindowProvider_InteractiveWindowCreateTest() {
            AppShell.Current = TestAppShell.Current;

            RInteractiveWindowProvider provider = new RInteractiveWindowProvider();
            IVsInteractiveWindow window = provider.Create(0);
        }
    }
}
