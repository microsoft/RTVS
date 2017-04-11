// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Test.Stubs.VisualComponents {
    [ExcludeFromCodeCoverage]
    public class VisualComponentContainerStub<T> : IVisualComponentContainer<T> where T : IVisualComponent {
        public T Component { get; set; }
        public string CaptionText { get; set; }
        public string StatusText { get; set; }
        public bool IsOnScreen { get; set; }
        public void Hide() { }
        public void Show(bool focus, bool immediate) => IsOnScreen = IsOnScreen | focus;
        public void UpdateCommandStatus(bool immediate) { }
        public void ShowContextMenu(CommandId commandId, Point position) { }
    }
}
