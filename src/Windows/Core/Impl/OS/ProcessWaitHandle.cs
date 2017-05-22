// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.OS {
    public class ProcessWaitHandle : WaitHandle {
        public ProcessWaitHandle(SafeProcessHandle processHandle) {
            IntPtr currentProcess = GetCurrentProcess();
            if(!DuplicateHandle(currentProcess, processHandle, currentProcess, out SafeWaitHandle handle, 0, false, (uint)DuplicateOptions.DUPLICATE_SAME_ACCESS)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            SafeWaitHandle = handle;
        }
    }
}
