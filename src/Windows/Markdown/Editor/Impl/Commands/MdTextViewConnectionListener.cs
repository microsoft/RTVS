// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Controllers.Views;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands {
    // In HTML case document creation and controller connection happens either in
    // application-specific listener or in text buffer / editor factory.
    public class MdTextViewConnectionListener : TextViewConnectionListener {
        protected override void OnTextViewConnected(ITextView textView, ITextBuffer textBuffer) {
            MdMainController.Attach(textView, textBuffer, Shell);
            base.OnTextViewConnected(textView, textBuffer);
        }

        protected override void OnTextBufferDisposing(ITextBuffer textBuffer) {
            var evm = textBuffer.GetService<IEditorViewModel>();
            evm?.Dispose();
            base.OnTextBufferDisposing(textBuffer);
        }
    }
}