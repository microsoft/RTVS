// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.R.Platform.OS {
    public sealed class SafeThreadHandle : SafeHandle {
        private bool _isInvalid;

        public SafeThreadHandle(IntPtr existingThreadHandle) : base(IntPtr.Zero, true) {
            SetHandle(existingThreadHandle);
        }

        protected override bool ReleaseHandle() {
            if (!IsInvalid) {
                NativeMethods.CloseHandle(handle);
                _isInvalid = true;
                return true;
            }
            return false;
        }

        public override bool IsInvalid => _isInvalid;
    }
}
