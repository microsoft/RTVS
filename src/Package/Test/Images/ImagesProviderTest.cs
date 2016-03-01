using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Editor.Imaging;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Shell;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Images {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ImagesProviderTest {
        [Test]
        [Category.Project.Services]
        public void ImagesProvider_Test() {
            IImagesProvider p = VsAppShell.Current.ExportProvider.GetExportedValue<IImagesProvider>();
            p.Should().NotBeNull();

            p.GetFileIcon("foo.R").Should().NotBeNull();
            p.GetFileIcon("foo.rproj").Should().NotBeNull();
            p.GetFileIcon("foo.rdata").Should().NotBeNull();

            p.GetImage("RProjectNode").Should().NotBeNull();
            p.GetImage("RFileNode").Should().NotBeNull();
            p.GetImage("RDataNode").Should().NotBeNull();
        }
    }
}
