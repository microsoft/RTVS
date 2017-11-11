// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Test.Fakes.ToolWindows {
    internal sealed class TestToolWindow : ContentControl, IToolWindow {
        public void Show(bool focus, bool immediate) => Visibility = Visibility.Visible;

        public object ViewModel { get; set; }

        public void Dispose() {
            (Content as IDisposable)?.Dispose();
            (ViewModel as IDisposable)?.Dispose();
        }
    }
}