// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class TextSelectionMock : ITextSelection {
        private ITextRange _range;

        public TextSelectionMock(ITextView textView, int position) :
            this(textView, new TextRange(position, 0)) {
        }

        public TextSelectionMock(ITextView textView, ITextRange range) {
            TextView = textView;
            _range = range;
        }

        public bool ActivationTracksFocus { get; set; }

        public VirtualSnapshotPoint ActivePoint => AnchorPoint;
        public VirtualSnapshotPoint AnchorPoint => new VirtualSnapshotPoint(new SnapshotPoint(TextView.TextBuffer.CurrentSnapshot, _range.Start));

        public VirtualSnapshotPoint End => new VirtualSnapshotPoint(new SnapshotPoint(TextView.TextBuffer.CurrentSnapshot, _range.End));

        public bool IsActive { get; set; } = true;
        public bool IsEmpty { get; set; } = true;
        public bool IsReversed { get; set; }
        public TextSelectionMode Mode { get; set; } = TextSelectionMode.Stream;

        public NormalizedSnapshotSpanCollection SelectedSpans => new NormalizedSnapshotSpanCollection(new SnapshotSpan(TextView.TextBuffer.CurrentSnapshot, _range.Start, _range.Length));
        public VirtualSnapshotPoint Start => AnchorPoint;

        public VirtualSnapshotSpan StreamSelectionSpan {
            get {
                return new VirtualSnapshotSpan(
                    new SnapshotSpan(TextView.TextBuffer.CurrentSnapshot, _range.Start, _range.Length));
            }
        }

        public ITextView TextView { get; private set; }
        public ReadOnlyCollection<VirtualSnapshotSpan> VirtualSelectedSpans => new ReadOnlyCollection<VirtualSnapshotSpan>(new VirtualSnapshotSpan[] { StreamSelectionSpan });

        public void Clear() {
            _range = IsReversed ? new TextRange(_range.End, 0) : new TextRange(_range.Start, 0);
            IsReversed = false;
        }

        public VirtualSnapshotSpan? GetSelectionOnTextViewLine(ITextViewLine line) {
            throw new NotImplementedException();
        }

        public void Select(VirtualSnapshotPoint anchorPoint, VirtualSnapshotPoint activePoint) {
            int anchor = anchorPoint.Position.Position;
            int pt = activePoint.Position.Position;

            IsReversed = pt < anchor;
            _range = IsReversed ? TextRange.FromBounds(pt, anchor) : TextRange.FromBounds(anchor, pt);

            SelectionChanged?.Invoke(TextView, EventArgs.Empty);
        }

        public void Select(SnapshotSpan selectionSpan, bool isReversed) {
            IsReversed = isReversed;

            _range = isReversed
                ? TextRange.FromBounds(selectionSpan.End.Position, selectionSpan.Start.Position)
                : TextRange.FromBounds(selectionSpan.Start.Position, selectionSpan.End.Position);

            SelectionChanged?.Invoke(TextView, EventArgs.Empty);
        }

#pragma warning disable 67
        public event EventHandler SelectionChanged;
    }
}
