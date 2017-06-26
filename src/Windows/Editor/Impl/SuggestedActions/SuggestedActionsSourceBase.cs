// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.SuggestedActions {
    public abstract class SuggestedActionsSourceBase : IDisposable {
        protected IEnumerable<ISuggestedActionProvider> SuggestedActionProviders { get; }
        protected ITextBuffer TextBuffer { get; }
        protected ITextView TextView { get; }
        protected IEditorDocument Document { get; }
        protected IServiceContainer Services { get; }
        protected bool IsDisposed { get; private set; }

        protected SuggestedActionsSourceBase(ITextView textView, ITextBuffer textBuffer, IEnumerable<ISuggestedActionProvider> suggestedActionProviders, IServiceContainer services) {
            TextBuffer = textBuffer;
            TextView = textView;
            Services = services;
            SuggestedActionProviders = suggestedActionProviders;

            TextView.Caret.PositionChanged += OnCaretPositionChanged;

            Document = TextBuffer.GetEditorDocument<IEditorDocument>();
            Document.Closing += OnDocumentClosing;
        }

        protected abstract void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e);
        private void OnDocumentClosing(object sender, EventArgs e) => Dispose();

        public void Dispose() {
            TextView.Caret.PositionChanged -= OnCaretPositionChanged;
            Document.Closing -= OnDocumentClosing;
            Dispose(true);
            IsDisposed = true;
        }

        protected virtual void Dispose(bool disposing) { }
    }
}
