// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Design;
using System.Windows;

namespace Microsoft.R.Components.View {
    /// <summary>
    /// Represents UI element that holds visual component
    /// (typically a tool window)
    /// </summary>
    public interface IVisualComponentContainer<out T> where T : IVisualComponent {
        T Component { get; }
        string CaptionText { get; set; }
        string StatusText { get; set; }
        bool IsOnScreen { get; }

        void Show(bool focus, bool immediate);
        void ShowContextMenu(CommandID commandId, Point position);
        void Hide();
        void UpdateCommandStatus(bool immediate);
    }
}

