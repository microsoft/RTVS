// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Controllers.Views;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Commands {
    public class RTextViewConnectionListener : TextViewConnectionListener {
        private IContainedLanguageHost _containedLanguageHost;
        private ITextBuffer _textBuffer;

        protected override void OnTextViewConnected(ITextView textView, ITextBuffer textBuffer) {
            RMainController.Attach(textView, textBuffer, Shell.Services);
            if (textBuffer != textView.TextBuffer) {
                // Projected scenario
                _containedLanguageHost = ContainedLanguageHost.GetHost(textView, textBuffer, Shell);
                if (_containedLanguageHost != null) {
                    _containedLanguageHost.Closing += OnContainedLanguageHostClosing;
                    _textBuffer = textBuffer;

                    var mainController = RMainController.FromTextView(textView);
                    var nextTarget = _containedLanguageHost.SetContainedCommandTarget(textView, mainController);
                    // Convert chained target to ICommandTarget (chained target might be IOleCommandTarget and host will create a shim then).
                    mainController.ChainedController = nextTarget;
                }
            }
            base.OnTextViewConnected(textView, textBuffer);
        }

        protected override void OnTextViewDisconnected(ITextView textView, ITextBuffer textBuffer) {
            if (textBuffer != textView.TextBuffer) {
                if (_containedLanguageHost != null) {
                    _containedLanguageHost.Closing -= OnContainedLanguageHostClosing;
                    _containedLanguageHost.RemoveContainedCommandTarget(textView);
                }
            }
            base.OnTextViewDisconnected(textView, textBuffer);
        }

        /// <summary>
        /// When CSS is contained within HTML, the host's Closing event will come in before
        /// the OnTextBufferDisposing event. Waiting for OnTextBufferDisposing would be too late,
        /// all services would already be removed from the CSS buffer.
        /// </summary>
        private void OnContainedLanguageHostClosing(object sender, EventArgs eventArgs) {
            if (_textBuffer != null) {
                OnTextBufferDisposing(_textBuffer);
            }
        }

        protected override void OnTextBufferDisposing(ITextBuffer textBuffer) {
            var viewModel = textBuffer.GetService<IEditorViewModel>();
            if (viewModel != null) {
                viewModel.Dispose();
            } else {
                var document = textBuffer.GetService<IREditorDocument>();
                document?.Dispose();
            }
            base.OnTextBufferDisposing(textBuffer);
        }
    }
}