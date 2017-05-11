// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Editor.Text {
    public sealed class ViewCaretPositionChangedEventArgs : EventArgs {
        public IEditorView View { get; }
        public ViewCaretPositionChangedEventArgs(IEditorView view) {
            View = view;
        }
    }
}
