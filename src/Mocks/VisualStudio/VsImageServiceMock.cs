// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

            IImageHandle h = ImageHandleMock.Create();
            svc.AddCustomImage(null).ReturnsForAnyArgs(h);
            svc.AddCustomImageList(null).ReturnsForAnyArgs(h);

            IVsUIObject uiObj = VsUiObjectMock.Create();
            svc.GetImage(Arg.Any<ImageMoniker>(), Arg.Any<ImageAttributes>()).ReturnsForAnyArgs(uiObj);
            svc.GetImageMonikerForFile(null).ReturnsForAnyArgs(KnownMonikers.AboutBox);
            svc.GetImageMonikerForHierarchyItem(null, 0u, 0).ReturnsForAnyArgs(KnownMonikers.AboutBox);
            svc.GetImageMonikerForName(null).ReturnsForAnyArgs(KnownMonikers.AboutBox);
            svc.GetImageMonikerType(Arg.Any<ImageMoniker>()).ReturnsForAnyArgs(0u);

            IVsImageMonikerImageList mock = VsImageMonikerImageListMock.Create();
            svc.CreateMonikerImageListFromHIMAGELIST(IntPtr.Zero).ReturnsForAnyArgs(mock);
            svc.GetImageListImageMonikers(Arg.Any<ImageMoniker>()).ReturnsForAnyArgs(mock);
            return svc;
        }
    }
}
