// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Text {
    [Export(typeof(ITextViewListener))]
    [Name("DefaultTextViewListener")]
    [ContentType("text")]
    public class TextViewListenerEvents : ITextViewListener {
        public static event EventHandler<TextViewListenerEventArgs> TextViewConnected;
        public static event EventHandler<TextViewListenerEventArgs> TextViewDisconnected;
        
        public void OnTextViewConnected(ITextView textView, ITextBuffer textBuffer) {
            TextViewConnected?.Invoke(this, new TextViewListenerEventArgs(textView, textBuffer));
        }

        public void OnTextViewDisconnected(ITextView textView, ITextBuffer textBuffer) {
            TextViewDisconnected?.Invoke(this, new TextViewListenerEventArgs(textView, textBuffer));
        }
    }
}
