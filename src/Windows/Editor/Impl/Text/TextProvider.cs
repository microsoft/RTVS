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
    [DebuggerDisplay("{" + nameof(GetText) + "()}")]
    public class TextProvider : ITextProvider, ITextSnapshotProvider {
        private const int DefaultBlockLength = 16384;
        private const int DefaultPreBlockLength = 128;

        private readonly int _partialBlockLength;
        private readonly bool _partial;
        private string _cachedBlock;
        private int _basePosition;

        [DebuggerStepThrough]
        public TextProvider(ITextSnapshot snapshot) : this(snapshot, 0) { }

        [DebuggerStepThrough]
        public TextProvider(ITextSnapshot snapshot, bool partial)
            : this(snapshot, partial ? DefaultBlockLength : 0) { }

        public TextProvider(ITextSnapshot snapshot, int partialBlockLength) {
            Snapshot = snapshot;
            Length = Snapshot.Length;
            _partial = partialBlockLength > 0;
            _partialBlockLength = partialBlockLength;
        }

        private void UpdateCachedBlock(int position, int length) {
            if (!_partial) {
                if (_cachedBlock == null) {
                    _cachedBlock = Snapshot.GetText(0, Snapshot.Length);
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
                    length = Math.Min(length, Snapshot.Length - position);

                    _cachedBlock = Snapshot.GetText(position, length);
                    _basePosition = position;
                }
            }
        }

        public int Length { get; }

        public char this[int position] {
            get {
                if (position < 0 || position >= Length) {
                    return '\0';
                }
                UpdateCachedBlock(position, 1);
                return _cachedBlock[position - _basePosition];
            }
        }

        public string GetText() => GetText(0, Length);

        public string GetText(int position, int length) {
            UpdateCachedBlock(position, length);
            var start = position - _basePosition;
            Debug.Assert((start >= 0) && (start + length <= _cachedBlock.Length));
            return _cachedBlock.Substring(start, length);
        }

        public string GetText(ITextRange range) => GetText(range.Start, range.Length);

        public int IndexOf(char ch, int startPosition) {
            for (var i = startPosition; i < Length; i++) {
                if (this[i] == ch) {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOf(char ch, ITextRange range) {
            var limit = Math.Min(Length, range.End);
            for (var i = range.Start; i < limit; i++) {
                if (this[i] == ch) {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOf(string text, int startPosition, bool ignoreCase)
            => IndexOf(text, TextRange.FromBounds(startPosition, Length), ignoreCase);

        public int IndexOf(string text, ITextRange range, bool ignoreCase) {
            if (string.IsNullOrEmpty(text)) {
                return range.Start;
            }

            var end = range.End - text.Length;
            for (int i = range.Start; i <= end; i++) {
                var found = true;
                var k = i;
                int j;

                for (j = 0; j < text.Length; j++, k++) {
                    var ch1 = text[j];
                    var ch2 = this[k];

                    if (ignoreCase) {
                        ch1 = char.ToLowerInvariant(ch1);
                        ch2 = char.ToLowerInvariant(ch2);
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
            if (text.Length != length) {
                return false;
            }

            UpdateCachedBlock(position, Math.Max(length, text.Length));
            return string.Compare(_cachedBlock, position - _basePosition,
                                  text, 0, text.Length,
                                  ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0;
        }

        public ITextProvider Clone() => new TextProvider(Snapshot, _partial);

        public int Version => Snapshot.Version.VersionNumber;

        #region ITextSnapshotProvider
        public ITextSnapshot Snapshot { get; }
        #endregion

#pragma warning disable 0067
        public event System.EventHandler<TextChangeEventArgs> OnTextChange;
    }
}
