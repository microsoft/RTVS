// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Text {
    [Export(typeof(ITextBufferListener))]
    [Name("WebTextBufferListenerEvents")]
    [ContentType("text")]
    public class TextBufferListenerEvents : ITextBufferListener {
        public static event EventHandler<TextBufferListenerEventArgs> TextBufferCreated;
        public static event EventHandler<TextBufferListenerEventArgs> TextBufferDisposed;

        public TextBufferListenerEvents() {
        }

        public void OnTextBufferCreated(ITextBuffer textBuffer) {
            if (TextBufferCreated != null) {
                TextBufferCreated(this, new TextBufferListenerEventArgs(textBuffer));
            }
        }

        public void OnTextBufferDisposed(ITextBuffer textBuffer) {
            if (TextBufferDisposed != null) {
                TextBufferDisposed(this, new TextBufferListenerEventArgs(textBuffer));
            }
        }
    }
}
