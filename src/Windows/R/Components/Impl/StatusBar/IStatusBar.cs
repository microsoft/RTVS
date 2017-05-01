// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace Microsoft.R.Components.StatusBar {
    public interface IStatusBar {
        IDisposable AddItem(UIElement item);
    }
}
