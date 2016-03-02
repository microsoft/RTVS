// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal enum ScrollType {
        Invalid,
        LineUp,
        LineDown,
        LineLeft,
        LineRight,
        PageUp,
        PageDown,
        PageLeft,
        PageRight,
        SetHorizontalOffset,
        SetVerticalOffset,
        MouseWheel,
        SizeChange,
        Refresh,
    }
}
