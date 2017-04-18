// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Text {
    public sealed class ViewCaret : IViewCaret {
        private readonly ITextView _textView;
        private readonly ITextCaret _caret;

        public ViewCaret(ITextView textView) {
            _caret = textView.Caret;
            _caret.PositionChanged += OnCaretPositionChanged;
        }

        #region IViewCaret
        public bool InVirtualSpace => _caret.InVirtualSpace;
        public IViewCaretPosition Position {
            get {
                var p = _caret.Position.BufferPosition;
                return new CaretPosition(new EditorSnapshotPoint(p.Snapshot.TextBuffer.ToEditorBuffer().CurrentSnapshot, p), _caret.Position.VirtualSpaces);
            }
        }
        public event EventHandler<ViewCaretPositionChangedEventArgs> PositionChanged;
        #endregion

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) 
            => PositionChanged?.Invoke(this, new ViewCaretPositionChangedEventArgs(_textView.ToEditorView()));
    }
}
