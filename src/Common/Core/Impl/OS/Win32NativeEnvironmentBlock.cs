// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.OS {
    public class Win32NativeEnvironmentBlock : IDisposable {
        public IntPtr NativeEnvironmentBlock { get; }

        public Win32NativeEnvironmentBlock(IntPtr env) {
            NativeEnvironmentBlock = env;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // dispose managed state (managed objects).
                }
                Marshal.FreeHGlobal(NativeEnvironmentBlock);
                disposedValue = true;
            }
        }

        ~Win32NativeEnvironmentBlock() {
            Dispose(false);
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
