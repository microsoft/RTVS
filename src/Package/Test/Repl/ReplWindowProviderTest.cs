using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Editor.ContentType;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    public class ReplWindowProviderTest {
        //[Test]
        //[Category.Repl]
        //public void ReplWindowProvider_ConstructionTest() {
        //    RInteractiveWindowProvider provider = new RInteractiveWindowProvider();
        //}

        //[Test]
        //[Category.Repl]
        //public void ReplWindowProvider_InteractiveWindowCreateTest() {
        //    RInteractiveWindowProvider provider = new RInteractiveWindowProvider();
        //    ITextBufferFactoryService svc = VsAppShell.Current.ExportProvider.GetExportedValue<ITextBufferFactoryService>();
        //    IContentTypeRegistryService r = VsAppShell.Current.ExportProvider.GetExportedValue<IContentTypeRegistryService>();
        //    var window = provider.Create(0);
        //    window.Should().NotBeNull();
        //}
    }
}
