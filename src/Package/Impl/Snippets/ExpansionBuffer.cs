// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.R.Package.Snippets {
    /// <summary>
    /// Facilitates the interop calls to the methods exposed by IVsExpansionEnumeration.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ExpansionBuffer {
        public IntPtr pathPtr;
        public IntPtr titlePtr;
        public IntPtr shortcutPtr;
        public IntPtr descriptionPtr;
    }
}
