// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal enum GridUpdateType {
        Invalid,
        LineUp,
        LineDown,
        LineLeft,
        LineRight,
        FocusUp,
        FocusDown,
        FocusLeft,
        FocusRight,
        HeaderFocusLeft,
        HeaderFocusRight,
        PageUp,
        PageDown,
        PageLeft,
        PageRight,
        FocusPageUp,
        FocusPageDown,
        FocusPageLeft,
        FocusPageRight,
        SetHorizontalOffset,
        SetVerticalOffset,
        SetFocus,
        SetHeaderFocus,
        MouseWheel,
        SizeChange,
        Refresh,
        Sort,
        ScrollIntoView
    }
}
