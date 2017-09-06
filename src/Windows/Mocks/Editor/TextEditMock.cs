// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class TextEditMock : ITextEdit {
        private readonly List<TextChangeMock> _changes = new List<TextChangeMock>();
        private readonly ITextBuffer _textBuffer;

        public TextEditMock(ITextBuffer textBuffer) {
            _textBuffer = textBuffer;
        }

        #region ITextEdit Members
        public bool Delete(int startPosition, int charsToDelete) {
            _changes.Add(new TextChangeMock(
                startPosition, charsToDelete,
                _textBuffer.CurrentSnapshot.GetText(startPosition, charsToDelete), String.Empty));

            return true;
        }

        public bool Delete(Span deleteSpan) {
            return Delete(deleteSpan.Start, deleteSpan.Length);
        }

        public bool HasEffectiveChanges {
            get { return _changes.Count > 0; }
        }

        public bool HasFailedChanges {
            get { return false; }
        }

        public bool Insert(int position, char[] characterBuffer, int startIndex, int length) {
            var sb = new StringBuilder();

            for (int i = startIndex; i < characterBuffer.Length; i++) {
                sb.Append(characterBuffer[i]);
            }

            return Insert(position, sb.ToString());
        }

        public bool Insert(int position, string text) {
            _changes.Add(new TextChangeMock(position, 0, String.Empty, text));
            return true;
        }

        public bool Replace(int startPosition, int charsToReplace, string replaceWith) {
            _changes.Add(new TextChangeMock(
                startPosition, charsToReplace,
                _textBuffer.CurrentSnapshot.GetText(startPosition, charsToReplace), replaceWith));

            return true;
        }

        public bool Replace(Span replaceSpan, string replaceWith) {
            return Replace(replaceSpan.Start, replaceSpan.Length, replaceWith);
        }

        #endregion

        #region ITextBufferEdit Members

        public ITextSnapshot Apply() {
            // Sort by position first then go backwards
            _changes.Sort();

            for (int i = _changes.Count - 1; i >= 0; i--) {
                var c = _changes[i];
                _textBuffer.Replace(new Span(c.OldPosition, c.OldLength), c.NewText);
            }

            return _textBuffer.CurrentSnapshot;
        }

        public void Cancel()  => Canceled = true;
        public bool Canceled { get; private set; }
        public ITextSnapshot Snapshot => _textBuffer.CurrentSnapshot;
        #endregion

        #region IDisposable Members
        public void Dispose() { }
        #endregion
    }
}
