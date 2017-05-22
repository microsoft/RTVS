// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// event arguments containing a text view
    /// </summary>
    public class TextViewListenerEventArgs : EventArgs {
        public ITextBuffer TextBuffer { get; }
        public ITextView TextView { get; }

        public TextViewListenerEventArgs(ITextView textView, ITextBuffer textBuffer) {
            TextBuffer = textBuffer;
            TextView = textView;
        }
    }
}
