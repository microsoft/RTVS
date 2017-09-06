// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.R.Platform.OS {
    public class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid {
        public SafeThreadHandle(IntPtr existingThreadHandle) : base(true) {
            SetHandle(existingThreadHandle);
        }

        protected override bool ReleaseHandle() => NativeMethods.CloseHandle(handle);
    }
}
