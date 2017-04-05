// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Text;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Text provider that implements ITextProvider over Visual Studio 
    /// core editor's ITextBuffer or ITextSnapshot 
    /// </summary>
    public class TextProvider : ITextProvider, ITextSnapshotProvider {
        private const int DefaultBlockLength = 16384;
        private const int DefaultPreBlockLength = 128;

        private string _cachedBlock;
        private int _basePosition;
        private ITextSnapshot _snapshot;
        private bool _partial;
        private int _partialBlockLength;

        public TextProvider(ITextSnapshot snapshot)
            : this(snapshot, 0) {
        }

        public TextProvider(ITextSnapshot snapshot, bool partial)
            : this(snapshot, partial ? DefaultBlockLength : 0) {
        }

        public TextProvider(ITextSnapshot snapshot, int partialBlockLength) {
            _snapshot = snapshot;
            Length = _snapshot.Length;
            _partial = partialBlockLength > 0;
            _partialBlockLength = partialBlockLength;
        }

        private void UpdateCachedBlock(int position, int length) {
            if (!_partial) {
                if (_cachedBlock == null) {
                    _cachedBlock = _snapshot.GetText(0, _snapshot.Length);
                }
            } else {
                if (_cachedBlock == null || position < _basePosition || (_basePosition + _cachedBlock.Length < position + length)) {
                    if (position < DefaultPreBlockLength) {
                        length += position;
                        position = 0;
                    } else {
                        length += DefaultPreBlockLength;
                        position -= DefaultPreBlockLength;
                    }

                    length = Math.Max(length, _partialBlockLength);
                    length = Math.Min(length, _snapshot.Length - position);

                    _cachedBlock = _snapshot.GetText(position, length);
                    _basePosition = position;
                }
            }
        }

        public int Length { get; private set; }

        public char this[int position] {
            get {
                if (position < 0 || position >= Length)
                    return '\0';

                UpdateCachedBlock(position, 1);
                return _cachedBlock[position - _basePosition];
            }
        }

        public string GetText(int position, int length) {
            UpdateCachedBlock(position, length);
            int start = position - _basePosition;
            Debug.Assert((start >= 0) && (start + length <= _cachedBlock.Length));
            return _cachedBlock.Substring(start, length);
        }

        public string GetText(ITextRange range) {
            return GetText(range.Start, range.Length);
        }

        public int IndexOf(char ch, int startPosition) {
            for (int i = startPosition; i < Length; i++) {
                if (this[i] == ch) {
                    return i;
                }
            }

            return -1;
        }

        public int IndexOf(char ch, ITextRange range) {
            int limit = Math.Min(Length, range.End);
            for (int i = range.Start; i < limit; i++) {
                if (this[i] == ch) {
                    return i;
                }
            }

            return -1;
        }

        public int IndexOf(string text, int startPosition, bool ignoreCase) {
            return IndexOf(text, TextRange.FromBounds(startPosition, this.Length), ignoreCase);
        }

        public int IndexOf(string text, ITextRange range, bool ignoreCase) {
            if (String.IsNullOrEmpty(text))
                return range.Start;

            int end = range.End - text.Length;
            for (int i = range.Start; i <= end; i++) {
                bool found = true;
                int k = i;
                int j;

                for (j = 0; j < text.Length; j++, k++) {
                    char ch1 = text[j];
                    char ch2 = this[k];

                    if (ignoreCase) {
                        ch1 = Char.ToLowerInvariant(ch1);
                        ch2 = Char.ToLowerInvariant(ch2);
                    }

                    if (ch1 != ch2) {
                        found = false;
                        break;
                    }
                }

                if (found && j == text.Length) {
                    return i;
                }
            }

            return -1;
        }

        public bool CompareTo(int position, int length, string text, bool ignoreCase) {
            if (text.Length != length)
                return false;

            UpdateCachedBlock(position, Math.Max(length, text.Length));

            return String.Compare(_cachedBlock, position - _basePosition,
                                  text, 0, text.Length,
                                  ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0;
        }

        public ITextProvider Clone() {
            return new TextProvider(_snapshot, _partial);
        }

        public int Version {
            get { return _snapshot.Version.VersionNumber; }
        }

#pragma warning disable 0067
        public event System.EventHandler<TextChangeEventArgs> OnTextChange;

        #region ITextSnapshotProvider

        public ITextSnapshot Snapshot {
            get { return _snapshot; }
        }

        #endregion
    }
}
