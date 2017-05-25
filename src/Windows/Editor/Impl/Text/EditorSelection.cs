// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Text {
    public sealed class EditorSelection : IEditorSelection {
        private readonly ITextView _textView;
        public EditorSelection(ITextView textView) {
            _textView = textView;
        }

        public SelectionMode Mode => _textView.Selection.Mode == TextSelectionMode.Stream ? SelectionMode.Stream : SelectionMode.Block;
        public ITextRange SelectedRange {
            get {
                var span = _textView.Selection.StreamSelectionSpan;
                return new TextRange(span.SnapshotSpan.Start.Position, span.Length);
            }
        }
    }
}
