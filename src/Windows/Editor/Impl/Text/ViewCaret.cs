// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Text {
    public sealed class ViewCaret : IViewCaret {
        private readonly ITextView _textView;

        public ViewCaret(ITextView textView) {
            _textView = textView;
            textView.Caret.PositionChanged += OnCaretPositionChanged;
        }

        #region IViewCaret
        public bool InVirtualSpace => _textView.Caret.InVirtualSpace;
        public IViewCaretPosition Position {
            get {
                var p = _textView.Caret.Position.BufferPosition;
                return new CaretPosition(new EditorSnapshotPoint(p.Snapshot.TextBuffer.CurrentSnapshot, p), _textView.Caret.Position.VirtualSpaces);
            }
        }

        public event EventHandler PositionChanged;

        public void MoveTo(int point, int virtualSpaces)
            => _textView.Caret.MoveTo(new VirtualSnapshotPoint(new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, point), virtualSpaces));
        #endregion

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) 
            => PositionChanged?.Invoke(this, EventArgs.Empty);
    }
}
