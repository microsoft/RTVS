// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Components.StatusBar;

namespace Microsoft.R.Components.Test.Fakes.StatusBar {
    [ExcludeFromCodeCoverage]
    public class TestStatusBar : IStatusBar {
        public IDisposable AddItem(UIElement item) => Disposable.Empty;
    }
}
