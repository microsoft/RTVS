// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Win32.SafeHandles;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.OS {
    public class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid {
        public SafeThreadHandle(IntPtr existingThreadHandle) : base(true) {
            SetHandle(existingThreadHandle);
        }

        protected override bool ReleaseHandle() => CloseHandle(handle);
    }
}
