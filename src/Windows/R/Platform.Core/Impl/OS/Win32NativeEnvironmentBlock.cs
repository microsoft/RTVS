// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.R.Platform.OS {
    public sealed class Win32NativeEnvironmentBlock : SafeHandle {
        private bool _isInvalid = true;

        public int Length { get; }

        public IntPtr NativeEnvironmentBlock => handle;

        private Win32NativeEnvironmentBlock(IntPtr handle, int length) : base(IntPtr.Zero, true) {
            Length = length;
            SetHandle(handle);
        }

        public static Win32NativeEnvironmentBlock Create(byte[] environmentData) {
            //RuntimeHelpers.PrepareConstrainedRegions();
            var ptr = Marshal.AllocHGlobal(environmentData.Length);
            Marshal.Copy(environmentData, 0, ptr, environmentData.Length);

            return new Win32NativeEnvironmentBlock(ptr, environmentData.Length);
        }

        protected override bool ReleaseHandle() {
            if (!_isInvalid) {
                Marshal.FreeHGlobal(handle);
                _isInvalid = true;
                return true;
            }
            return false;
        }

        public override bool IsInvalid => _isInvalid;
    }
}
