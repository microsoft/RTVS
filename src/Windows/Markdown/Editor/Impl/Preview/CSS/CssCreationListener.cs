// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Preview.Css {
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("CSS")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class CssCreationListener : IVsTextViewCreationListener {
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;

        [ImportingConstructor]
        public CssCreationListener(IVsEditorAdaptersFactoryService efs, ITextDocumentFactoryService tdfs) {
            _editorAdaptersFactoryService = efs;
            _textDocumentFactoryService = tdfs;
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter) {
            var textView = _editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (_textDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out ITextDocument document)) {
                document.FileActionOccurred += DocumentSaved;
            }
        }

        private void DocumentSaved(object sender, TextDocumentFileActionEventArgs e) {
            if (e.FileActionType == FileActionTypes.ContentSavedToDisk) {
                StylesheetUpdated?.Invoke(this, new StylesheetUpdatedEventArgs(e.FilePath));
            }
        }

        public static event EventHandler<StylesheetUpdatedEventArgs> StylesheetUpdated;
    }
}
