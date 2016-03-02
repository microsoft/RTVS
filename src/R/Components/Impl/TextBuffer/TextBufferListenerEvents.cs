// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Text {
    [Export(typeof(ITextBufferListener))]
    [Name(nameof(TextBufferListenerEvents))]
    [ContentType("text")]
    public class TextBufferListenerEvents : ITextBufferListener {
        public static event EventHandler<TextBufferListenerEventArgs> TextBufferCreated;
        public static event EventHandler<TextBufferListenerEventArgs> TextBufferDisposed;

        public void OnTextBufferCreated(ITextBuffer textBuffer) {
            TextBufferCreated?.Invoke(this, new TextBufferListenerEventArgs(textBuffer));
        }

        public void OnTextBufferDisposed(ITextBuffer textBuffer) {
            TextBufferDisposed?.Invoke(this, new TextBufferListenerEventArgs(textBuffer));
        }
    }
}
