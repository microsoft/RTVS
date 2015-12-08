using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public static class VsImageServiceMock {
        public static IVsImageService2 Create() {
            IVsImageService2 svc = Substitute.For<IVsImageService2>();
            svc.AddCustomImage(null).ReturnsForAnyArgs(ImageHandleMock.Create());
            svc.AddCustomImageList(null).ReturnsForAnyArgs(ImageHandleMock.Create());
            svc.GetImage(KnownMonikers.AboutBox, new ImageAttributes()).ReturnsForAnyArgs(VsUiObjectMock.Create());
            svc.GetImageMonikerForFile(null).ReturnsForAnyArgs(KnownMonikers.AboutBox);
            svc.GetImageMonikerForHierarchyItem(null, 0u, 0).ReturnsForAnyArgs(KnownMonikers.AboutBox);
            svc.GetImageMonikerForName(null).ReturnsForAnyArgs(KnownMonikers.AboutBox);
            svc.GetImageMonikerType(KnownMonikers.AboutBox).ReturnsForAnyArgs(0u);
            svc.CreateMonikerImageListFromHIMAGELIST(IntPtr.Zero).ReturnsForAnyArgs(VsImageMonikerImageListMock.Create());
            svc.GetImageListImageMonikers(KnownMonikers.AboutBox).ReturnsForAnyArgs(VsImageMonikerImageListMock.Create());
            return svc;
        }
    }
}
