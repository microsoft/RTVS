// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public static class VsSettingsStoreMock {
        public static IVsWritableSettingsStore Create() {
            return Substitute.For<IVsWritableSettingsStore>();
        }
    }
}
