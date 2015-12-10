using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CranMirrorListTest {
        [TestMethod]
        [TestCategory("R.Packages")]
        public void CranMirrorList_DownloadTest() {
            ManualResetEventSlim evt = new ManualResetEventSlim();
            int eventCount = 0;

            CranMirrorList.DownloadComplete += (e, args) => {
                eventCount++;
                Assert.IsTrue(CranMirrorList.MirrorNames.Length > 0);
                Assert.IsTrue(CranMirrorList.MirrorUrls.Length > 0);
                Assert.AreEqual("https://cran.rstudio.com", CranMirrorList.UrlFromName("0 - Cloud[https]"));
                evt.Set();
            };

            CranMirrorList.Download();
            evt.Wait(10000);
            Assert.AreEqual(1, eventCount);

            CranMirrorList.Download();
            Assert.AreEqual(2, eventCount);
        }
    }
}
