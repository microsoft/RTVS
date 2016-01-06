using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    public class CranMirrorListTest {
        [Test]
        [Category.R.Package]
        public void CranMirrorList_DownloadTest() {
            ManualResetEventSlim evt = new ManualResetEventSlim();
            int eventCount = 0;

            CranMirrorList.DownloadComplete += (e, args) => {
                eventCount++;
                CranMirrorList.MirrorNames.Should().NotBeEmpty();
                CranMirrorList.MirrorUrls.Should().NotBeEmpty();
                CranMirrorList.UrlFromName("0 - Cloud[https]").Should().Be("https://cran.rstudio.com");
                evt.Set();
            };

            CranMirrorList.Download();
            evt.Wait(10000);
            eventCount.Should().Be(1);

            CranMirrorList.Download();
            eventCount.Should().Be(2);
        }
    }
}
