// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// event arguments containing a text buffer
    /// </summary>
    public class TextBufferListenerEventArgs : EventArgs {
        public ITextBuffer TextBuffer { get; private set; }

        public TextBufferListenerEventArgs(ITextBuffer textBuffer) {
            TextBuffer = textBuffer;
        }
    }
}
