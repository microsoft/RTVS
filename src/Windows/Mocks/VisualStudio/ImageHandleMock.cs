// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public static class ImageHandleMock {
        public static IImageHandle Create() {
            IImageHandle h = Substitute.For<IImageHandle>();
            h.Moniker.Returns(KnownMonikers.AboutBox);
            return h;
        }
    }
}
