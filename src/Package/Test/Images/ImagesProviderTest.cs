using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Imaging;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ImagesProviderTest {
        [TestMethod]
        [TestCategory("Project.Services")]
        public void ImagesProvider_Test() {
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
