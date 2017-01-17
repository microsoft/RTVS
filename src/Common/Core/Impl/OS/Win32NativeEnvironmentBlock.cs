// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Common.Core.OS {
    public sealed class Win32NativeEnvironmentBlock : SafeBuffer {
        public int Length { get; }

        public IntPtr NativeEnvironmentBlock => handle;

        private Win32NativeEnvironmentBlock(IntPtr env, int length) : base(true) {
            SetHandle(env);
            Length = length;
        }

        public static Win32NativeEnvironmentBlock Create(byte[] environmentData) {
            IntPtr ptr = Marshal.AllocHGlobal(environmentData.Length);
            Marshal.Copy(environmentData, 0, ptr, environmentData.Length);
            return new Win32NativeEnvironmentBlock(ptr, environmentData.Length);
        }

        protected override bool ReleaseHandle() {
            Marshal.FreeHGlobal(handle);
            return true;
        }
    }
}
