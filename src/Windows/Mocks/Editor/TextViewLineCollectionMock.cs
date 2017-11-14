// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Editor.Mocks {
    public sealed class TextViewLineCollectionMock : ITextViewLineCollection {
        private ITextBuffer _textBuffer;

        public TextViewLineCollectionMock(ITextBuffer textBuffer) {
            _textBuffer = textBuffer;
        }

        private ITextSnapshot Snapshot => _textBuffer.CurrentSnapshot;

        public ITextViewLine this[int index] {
            get { return new TextViewLineMock(Snapshot.GetLineFromLineNumber(index)); }
            set { throw new NotImplementedException(); }
        }

        public int Count => Snapshot.LineCount;
        public ITextViewLine FirstVisibleLine {
            get { return new TextViewLineMock(Snapshot.GetLineFromLineNumber(0)); }
        }

        public SnapshotSpan FormattedSpan {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly => false;
        public bool IsValid => true;

        public ITextViewLine LastVisibleLine {
            get { return new TextViewLineMock(Snapshot.GetLineFromLineNumber(Snapshot.LineCount - 1)); }
        }

        public void Add(ITextViewLine item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(ITextViewLine item) {
            return item.Snapshot == Snapshot;
        }

        public bool ContainsBufferPosition(SnapshotPoint bufferPosition) {
            return bufferPosition.Snapshot == Snapshot;
        }

        public void CopyTo(ITextViewLine[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public TextBounds GetCharacterBounds(SnapshotPoint bufferPosition) {
            throw new NotImplementedException();
        }

        public IEnumerator<ITextViewLine> GetEnumerator() {
            throw new NotImplementedException();
        }

        public int GetIndexOfTextLine(ITextViewLine textLine) {
            throw new NotImplementedException();
        }

        public Collection<TextBounds> GetNormalizedTextBounds(SnapshotSpan bufferSpan) {
            throw new NotImplementedException();
        }

        public SnapshotSpan GetTextElementSpan(SnapshotPoint bufferPosition) {
            throw new NotImplementedException();
        }

        public ITextViewLine GetTextViewLineContainingBufferPosition(SnapshotPoint bufferPosition) {
            return new TextViewLineMock(Snapshot.GetLineFromPosition(bufferPosition.Position));
        }

        public ITextViewLine GetTextViewLineContainingYCoordinate(double y) {
            throw new NotImplementedException();
        }

        public Collection<ITextViewLine> GetTextViewLinesIntersectingSpan(SnapshotSpan bufferSpan) {
            throw new NotImplementedException();
        }

        public int IndexOf(ITextViewLine item) {
            throw new NotImplementedException();
        }

        public void Insert(int index, ITextViewLine item) {
            throw new NotImplementedException();
        }

        public bool IntersectsBufferSpan(SnapshotSpan bufferSpan) {
            throw new NotImplementedException();
        }

        public bool Remove(ITextViewLine item) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}
