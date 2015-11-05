using System;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    public sealed class VsImageServiceMock : IVsImageService2 {
        public void Add(string Name, IVsUIObject pIconObject) {
            throw new NotImplementedException();
        }

        public IImageHandle AddCustomCompositeImage(int virtualWidth, int virtualHeight, int layerCount, ImageCompositionLayer[] layers) {
            throw new NotImplementedException();
        }

        public IImageHandle AddCustomImage(IVsUIObject imageObject) {
            return new ImageHandleMock();
        }

        public IImageHandle AddCustomImageList(IVsImageMonikerImageList imageList) {
            return new ImageHandleMock();
        }

        public IVsImageMonikerImageList CreateMonikerImageListFromHIMAGELIST(IntPtr hImageList) {
            return new VsImageMonikerImageListMock();
        }

        public IVsUIObject Get(string Name) {
            throw new NotImplementedException();
        }

        public IVsUIObject GetIconForFile(string filename, __VSUIDATAFORMAT desiredFormat) {
            throw new NotImplementedException();
        }

        public IVsUIObject GetIconForFileEx(string filename, __VSUIDATAFORMAT desiredFormat, out uint iconSource) {
            throw new NotImplementedException();
        }

        public IVsUIObject GetImage(ImageMoniker moniker, ImageAttributes attributes) {
            throw new NotImplementedException();
        }

        public IVsImageMonikerImageList GetImageListImageMonikers(ImageMoniker moniker) {
            return new VsImageMonikerImageListMock();
        }

        public ImageMoniker GetImageMonikerForFile(string filename) {
            return KnownMonikers.AboutBox;
        }

        public ImageMoniker GetImageMonikerForHierarchyItem(IVsHierarchy hierarchy, uint hierarchyItemID, int hierarchyImageAspect) {
            return KnownMonikers.AboutBox;
        }

        public ImageMoniker GetImageMonikerForName(string imageName) {
            return KnownMonikers.AboutBox;
        }

        public uint GetImageMonikerType(ImageMoniker moniker) {
            return 0;
        }

        public void RemoveCustomImage(IImageHandle handle) {
            throw new NotImplementedException();
        }

        public void RemoveCustomImageList(IImageHandle handle) {
            throw new NotImplementedException();
        }

        public bool ThemeDIBits(int pixelCount, byte[] pixels, int width, int height, bool isTopDownBitmap, uint backgroundColor) {
            throw new NotImplementedException();
        }

        public bool TryAssociateNameWithMoniker(string imageName, ImageMoniker moniker) {
            throw new NotImplementedException();
        }

        public bool TryParseImageMoniker(string monikerAsString, out ImageMoniker moniker) {
            throw new NotImplementedException();
        }
    }
}
