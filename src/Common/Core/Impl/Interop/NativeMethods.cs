// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;

namespace Microsoft.Common.Core.Interop {
    internal static class NativeMethods {
        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);
    }
}
