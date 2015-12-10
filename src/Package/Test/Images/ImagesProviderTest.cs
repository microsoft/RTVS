using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Editor.Imaging;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ImagesProviderTest {
        [TestMethod]
        public void ImagesProvider_Test() {
            VsAppShell.Current = TestAppShell.Current;

            IImagesProvider p = VsAppShell.Current.ExportProvider.GetExportedValue<IImagesProvider>();
            Assert.IsNotNull(p);

            Assert.IsNotNull(p.GetFileIcon("foo.R"));
            Assert.IsNotNull(p.GetFileIcon("foo.rproj"));
            Assert.IsNotNull(p.GetFileIcon("foo.rdata"));

            Assert.IsNotNull(p.GetImage("RProjectNode"));
            Assert.IsNotNull(p.GetImage("RFileNode"));
            Assert.IsNotNull(p.GetImage("RDataNode"));
        }
    }
}
