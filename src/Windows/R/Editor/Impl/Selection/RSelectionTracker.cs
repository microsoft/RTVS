// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Selection;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Selection {
    /// <summary>
    /// JScript selection tracker. Helps preserve selection and correct
    /// caret position during script autoformatting. Uses tokenizer to
    /// calculate where caret should be after autoatic formatting.
    /// </summary>
    internal sealed class RSelectionTracker : SelectionTracker {
        /// <summary>
        /// Index of token that is nearest to the caret
        /// </summary>
        private int _index;

        /// <summary>
        /// Offset from token to the caret position
        /// </summary>
        private int _offset;
        private ITextRange _changingRange;
        private readonly int _lengthBeforeChange;

        /// <summary>
        /// SelectionTracker constructor
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <param name="textBuffer">Editor text buffer (may be different from one attached to text view)</param>
        /// <param name="changingRange">Range that is changing in the buffer</param>
        public RSelectionTracker(ITextView textView, ITextBuffer textBuffer, ITextRange changingRange) : base(textView.ToEditorView()) {
            TextBuffer = textBuffer;
            _changingRange = changingRange;
            _lengthBeforeChange = TextBuffer.CurrentSnapshot.Length;
        }

        #region ISelectionTracker

        /// <summary>
        /// Editor text buffer.
        /// </summary>
        public ITextBuffer TextBuffer { get; }

        /// <summary>
        /// Saves current selection
        /// </summary>
        public override void StartTracking(bool automaticTracking) {
            var documentPosition = EditorView.GetCaretPosition(TextBuffer.ToEditorBuffer());
            if (documentPosition != null) {
                VirtualSpaces = EditorView.As<ITextView>().Caret.Position.VirtualSpaces;
                TokenFromPosition(TextBuffer.CurrentSnapshot, documentPosition.Position, out _index, out _offset);
            }
            base.StartTracking(false);
        }

        /// <summary>
        /// Restores saved selection
        /// </summary>
        public override void EndTracking() {
            int position = PositionFromTokens(TextBuffer.CurrentSnapshot, _index, _offset);
            if (position >= 0) {
                PositionAfterChanges = new SnapshotPoint(TextBuffer.CurrentSnapshot, position);
            }
            MoveToAfterChanges(VirtualSpaces);
        }
        #endregion

        private void TokenFromPosition(ITextSnapshot snapshot, int position, out int itemIndex, out int offset) {
            // Normally token stream does not change after formatting so we can simply rely on the fact 
            // that caret position is going to remain relative to the same token index
            itemIndex = -1;
            offset = 0;

            // Expand range to include the next line. This is needed when user introduces line break.
            var lineNumber = snapshot.GetLineNumberFromPosition(_changingRange.End);
            if (lineNumber < snapshot.LineCount - 1) {
                var end = snapshot.GetLineFromLineNumber(lineNumber + 1).End;
                _changingRange = TextRange.FromBounds(_changingRange.Start, end);
            }

            var tokenizer = new RTokenizer();
            var tokens = tokenizer.Tokenize(new TextProvider(snapshot), _changingRange.Start, _changingRange.Length, true);

            // Check if position is adjacent to previous token
            var prevItemIndex = tokens.GetFirstItemBeforePosition(position);
            if (prevItemIndex >= 0 && tokens[prevItemIndex].End == position) {
                itemIndex = prevItemIndex;
                offset = -tokens[itemIndex].Length;
                return;
            }

            var nextItemIndex = tokens.GetFirstItemAfterOrAtPosition(position);
            if (nextItemIndex >= 0) {
                // If two tokens are adjacent, gravity is negative, i.e. caret travels
                // with preceding token so it won't just to aniother line if, say, 
                // formatter decides to insert a new line between tokens.

                if (nextItemIndex > 0 && tokens[nextItemIndex - 1].End == tokens[nextItemIndex].Start) {
                    nextItemIndex--;
                }

                offset = tokens[nextItemIndex].Start - position;
                itemIndex = nextItemIndex;
                return;
            }

            // We are past last token
            if (tokens.Count > 0) {
                itemIndex = tokens.Count - 1;
                offset = tokens[itemIndex].Start - position;
            } else {
                itemIndex = -1;
                offset = position;
            }
        }

        private int PositionFromTokens(ITextSnapshot snapshot, int itemIndex, int offset) {
            var lengthChange = snapshot.Length - _lengthBeforeChange;
            var tokenizer = new RTokenizer();
            var tokens = tokenizer.Tokenize(
                new TextProvider(snapshot), _changingRange.Start, _changingRange.Length + lengthChange, true);

            if (itemIndex >= 0 && itemIndex < tokens.Count) {
                var position = tokens[itemIndex].Start - offset;

                position = Math.Min(position, snapshot.Length);
                position = Math.Max(position, 0);

                return position;
            }

            return _changingRange.End + lengthChange;
        }
    }
}
