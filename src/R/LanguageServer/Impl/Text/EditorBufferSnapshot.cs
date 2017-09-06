// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class EditorBufferSnapshot : IEditorBufferSnapshot {
        private readonly string _content;
        private readonly TextRangeCollection<ITextRange> _lineRanges = new TextRangeCollection<ITextRange>();

        public EditorBufferSnapshot(IEditorBuffer editorBuffer, string content, int version) {
            EditorBuffer = editorBuffer;
            _content = content;
            Version = version;
            SplitTextIntoLines(content);
        }

        #region IEditorBufferSnapshot
        public int Length => _content.Length;
        public char this[int position] => _content[position];
        public string GetText() => _content;
        public string GetText(ITextRange range) => _content.Substring(range.Start, range.Length);

        public int IndexOf(string text, int startPosition, bool ignoreCase)
            => _content.IndexOf(text, startPosition, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        public int IndexOf(char ch, int startPosition) => _content.IndexOf(ch, startPosition);
        public int IndexOf(char ch, ITextRange range) => _content.IndexOf(ch, range.Start, range.Length);

        public int IndexOf(string text, ITextRange range, bool ignoreCase)
            => _content.IndexOf(text, range.Start, range.Length, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        public bool CompareTo(int position, int length, string text, bool ignoreCase)
            => string.Compare(_content, position, text, 0, length, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0;

        public ITextProvider Clone() => new TextStream(new string(_content.ToCharArray()));
        public int Version { get; }

        public T As<T>() where T : class => throw new NotImplementedException();

        public IEditorBuffer EditorBuffer { get; }

        public int LineCount => _lineRanges.Count;

        public IEditorLine GetLineFromPosition(int position)
            => GetLineFromLineNumber(_lineRanges.GetItemContaining(position));

        public IEditorLine GetLineFromLineNumber(int lineNumber)
            => lineNumber >= 0 && lineNumber < _lineRanges.Count ? new EditorLine(this, _lineRanges[lineNumber], lineNumber) : null;

        public int GetLineNumberFromPosition(int position) => _lineRanges.GetItemContaining(position);

        public ITrackingTextRange CreateTrackingRange(ITextRange range) => throw new NotImplementedException();
        #endregion

        private void SplitTextIntoLines(string text) {
            var lineStart = 0;

            for (var i = 0; i < text.Length; i++) {
                var ch = text[i];
                if (ch.IsLineBreak()) {
                    if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') {
                        i++;
                    } else if (ch == '\n' && i + 1 < text.Length && text[i + 1] == '\r') {
                        i++;
                    }
                    _lineRanges.Add(TextRange.FromBounds(lineStart, i + 1));
                    lineStart = i + 1;
                }
            }

            _lineRanges.Add(TextRange.FromBounds(lineStart, text.Length));
        }


#pragma warning disable 67
        public event EventHandler<TextChangeEventArgs> OnTextChange;
    }
}
