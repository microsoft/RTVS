// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UnitTests.Core {
    internal static class NativeMethods {
        [DllImport("kernel32.dll")]
        public static extern uint GetModuleFileName(IntPtr hModule, StringBuilder buffer, int cchBufferSize);
    }
}
