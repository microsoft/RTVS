using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class ImageHandleMock : IImageHandle {
        public ImageMoniker Moniker {
            get {
                return KnownMonikers.AboutBox;
            }
        }
    }
}
