// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsExpansionEnumerationMock : IVsExpansionEnumeration {
        public int GetCount(out uint pCount) {
            pCount = 0;
            return VSConstants.S_OK;
        }

        public int Next(uint celt, IntPtr[] rgelt, out uint pceltFetched) {
            pceltFetched = 0;
            return VSConstants.S_FALSE;
        }

        public int Reset() {
            return VSConstants.S_OK;
        }
    }
}
