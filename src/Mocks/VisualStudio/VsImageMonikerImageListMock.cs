using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    public sealed class VsImageMonikerImageListMock : IVsImageMonikerImageList {
        public int ImageCount {
            get {
                return 1;
            }
        }

        public void GetImageMonikers(int firstImageIndex, int imageMonikerCount, ImageMoniker[] imageMonikers) {
            for(int i =0; i < imageMonikerCount; i++) {
                imageMonikers[i] = KnownMonikers.AboutBox;
            }
        }
    }
}
