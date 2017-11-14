// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public static class VsImageMonikerImageListMock {
        public static IVsImageMonikerImageList Create() {
            IVsImageMonikerImageList il = Substitute.For<IVsImageMonikerImageList>();
            il.ImageCount.Returns(1);
            il
                .When(x => x.GetImageMonikers(
                    Arg.Is<int>(first => first >= 0),
                    Arg.Is<int>(count => count >= 0),
                    Arg.Is<ImageMoniker[]>(array => array != null)))
                .Do(x => {
                    ImageMoniker[] array = x[2] as ImageMoniker[];
                    for (int i = 0; i < array.Length; i++) {
                        array[i] = KnownMonikers.AboutBox;
                    }
                });
            return il;
        }
    }
}
