// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Microsoft.R.Components.InfoBar {
    public interface IInfoBarProvider {
        IInfoBar Create(Decorator host);
    }

    public interface IInfoBar {
        IDisposable Add(InfoBarItem item);
    }

    public struct InfoBarItem {
        public string Text { get; }
        public IReadOnlyDictionary<string, Action> LinkButtons { get; }
        public bool ShowCloseButton { get; }

        public InfoBarItem(string text, IReadOnlyDictionary<string, Action> linkButtons = null, bool showCloseButton = true) {
            Text = text;
            LinkButtons = linkButtons ?? new Dictionary<string, Action>();
            ShowCloseButton = showCloseButton;
        }
    }
}