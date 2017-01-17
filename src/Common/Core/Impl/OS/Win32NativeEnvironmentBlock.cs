// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Common.Core.OS {
    public sealed class Win32NativeEnvironmentBlock : SafeHandleZeroOrMinusOneIsInvalid {
        public int Length { get; }

        public IntPtr NativeEnvironmentBlock => handle;

        private Win32NativeEnvironmentBlock(int length) : base(true) {
            Length = length;
        }

        public static Win32NativeEnvironmentBlock Create(byte[] environmentData) {
            var eb = new Win32NativeEnvironmentBlock(environmentData.Length);

            RuntimeHelpers.PrepareConstrainedRegions();
            try { } finally {
                IntPtr ptr = Marshal.AllocHGlobal(environmentData.Length);
                eb.SetHandle(ptr);
                Marshal.Copy(environmentData, 0, ptr, environmentData.Length);
            }

            return eb;
        }

        protected override bool ReleaseHandle() {
            Marshal.FreeHGlobal(handle);
            return true;
        }
    }
}
