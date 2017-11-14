// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks {
    /// <summary>
    /// Mock implementation of ITextSnapshotLine based on mock snapshot for unit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class TextLineMock : ITextSnapshotLine {
        private readonly int _start;

        public TextLineMock(ITextSnapshot snapshot, int start, int length, int lineNumber) {
            _start = start;

            Snapshot = snapshot;
            Length = length;
            LineNumber = lineNumber;
        }

        #region ITextSnapshotLine Members

        public SnapshotPoint End => new SnapshotPoint(Snapshot, _start + Length);

        public SnapshotPoint EndIncludingLineBreak => _start + Length + 2 <= Snapshot.Length ?
            new SnapshotPoint(Snapshot, _start + Length + 2) :
            new SnapshotPoint(Snapshot, _start + Length);

        public SnapshotSpan Extent => new SnapshotSpan(Snapshot, new Span(_start, Length));

        public SnapshotSpan ExtentIncludingLineBreak => _start + Length + 2 <= Snapshot.Length ?
            new SnapshotSpan(Snapshot, new Span(_start, Length + 2)) :
            new SnapshotSpan(Snapshot, new Span(_start, Length));

        public string GetLineBreakText() => "\r\n";

        public string GetText() => Snapshot.GetText(_start, Length);
        public string GetTextIncludingLineBreak() => GetText() + GetLineBreakText();
        public int Length { get; }

        public int LengthIncludingLineBreak => Length + LineBreakLength;

        public int LineBreakLength {
            get {
                // Mock only handles \n or \r\n
                int end = _start + Length;
                int extra = 0;

                if (end < Snapshot.Length) {
                    if (Snapshot[end] == '\r') {
                        extra++;
                        end++;
                    }
                }

                if (end < Snapshot.Length) {
                    if (Snapshot[end] == '\n') {
                        extra++;
                    }
                }

                return extra;
            }
        }

        public int LineNumber { get; }
        public ITextSnapshot Snapshot { get; }
        public SnapshotPoint Start => new SnapshotPoint(Snapshot, _start);
        #endregion
    }
}
