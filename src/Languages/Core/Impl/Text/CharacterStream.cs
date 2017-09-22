// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Helper class that represents stream of characters for a parser or tokenizer
    /// </summary>
    [DebuggerDisplay("[{Text} {CurrentChar}]")]
    public class CharacterStream {
        private readonly TextRange _range;
        private int _position;
        private bool _isEndOfStream;

        #region Constructors

        [DebuggerStepThrough]
        public CharacterStream(ITextProvider textProvider)
            : this(textProvider, TextRange.FromBounds(0, textProvider.Length)) {
        }

        public CharacterStream(ITextProvider textProvider, ITextRange range) {
            Text = textProvider;

            var end = Math.Min(Text.Length, range.End);
            _range = TextRange.FromBounds(range.Start, end);

            Position = _range.Start;
            CurrentChar = Text.Length > 0 ? Text[_range.Start] : '\0';
        }

        [DebuggerStepThrough]
        public CharacterStream(string text)
            : this(new TextStream(text)) {
        }
        #endregion

        /// <summary>
        /// Text provider that supplies underlying text. May be a string, a text buffer or a buffer snapshot.
        /// </summary>
        public ITextProvider Text { get; }

        /// <summary>
        /// Determines if current position is at the end of text
        /// </summary>
        /// <returns>True if position is at the end of stream</returns>
        public bool IsEndOfStream() => _isEndOfStream;

        public int DistanceFromEnd => _range.End - Position;

        /// <summary>
        /// Returns character at a given position. If position is beyond text limits, returns '\0'
        /// </summary>
        /// <param name="position">Stream position</param>
        public char this[int position] => Text[position];

        public string GetSubstringAt(int position, int length) => Text.GetText(new TextRange(position, length));
        public int IndexOf(string text, int start, bool ignoreCase) => Text.IndexOf(text, start, ignoreCase);
        public int IndexOf(char ch, int start) => Text.IndexOf(ch, start);

        public bool CompareTo(int position, int length, string text, bool ignoreCase)
            => Text.CompareTo(position, length, text, ignoreCase);

        public char CurrentChar { get; private set; }

        public char NextChar => Position + 1 < _range.End ? Text[Position + 1] : '\0';

        public char PrevChar => Position > _range.Start ? Text[Position - 1] : '\0';

        /// <summary>
        /// Returns characters at an offset from the current position
        /// </summary>
        /// <param name="offset">Offset from the current position</param>
        /// <returns>Character or '\0' if offset is beyond text boundaries</returns>
        public char LookAhead(int offset) {
            int pos = Position + offset;

            if (pos < 0 || pos >= Text.Length) {
                return '\0';
            }

            return Text[pos];
        }

        /// <summary>
        /// Current stream position
        /// </summary>
        public int Position {
            get => _position;
            set {
                _position = value;
                CheckBounds();
            }
        }

        /// <summary>
        /// Length of the stream
        /// </summary>
        public int Length => _range.Length;

        /// <summary>
        /// Moves current position forward or backward
        /// </summary>
        /// <param name="offset">Offset to move by</param>
        public void Advance(int offset) {
            Position += offset;
        }

        /// <summary>
        /// Moves position to the next character if possible.
        /// Returns false if position is at the end of stream.
        /// </summary>
        public bool MoveToNextChar() {
            if (_position < _range.End - 1) {
                // Most common case, no need to check bounds extensively
                _position++;
                CurrentChar = Text[_position];
                return true;
            }

            Advance(1);
            return !_isEndOfStream;
        }

        /// <summary>
        /// Detemines if current character is a whitespace
        /// </summary>
        public bool IsWhiteSpace() => CurrentChar <= ' ' || CurrentChar == 0x200B;

        /// <summary>
        /// Determines if current character is a new line character
        /// </summary>
        public bool IsAtNewLine() => IsNewLine(CurrentChar);

        public static bool IsNewLine(char currentCharacter) {
            return (currentCharacter == '\n' || currentCharacter == '\r');
        }

        public void SkipLineBreak() {
            if (CurrentChar == '\n') {
                MoveToNextChar();
                if (CurrentChar == '\r') {
                    MoveToNextChar();
                }
            } else if (CurrentChar == '\r') {
                MoveToNextChar();
                if (CurrentChar == '\n') {
                    MoveToNextChar();
                }
            }
        }

        public void SkipToEol() {
            while (!IsEndOfStream() && !IsAtNewLine()) {
                MoveToNextChar();
            }
        }

        public void SkipToWhitespace() {
            while (!IsEndOfStream() && !IsWhiteSpace()) {
                MoveToNextChar();
            }
        }

        public void SkipWhitespace() {
            while (!IsEndOfStream() && IsWhiteSpace()) {
                MoveToNextChar();
            }
        }

        /// <summary>
        /// Determines if current character is a letter
        /// </summary>
        public bool IsLetter() => IsLetter(CurrentChar);

        /// <summary>
        /// Determines if current character is a letter
        /// </summary>
        public static bool IsLetter(char character) => char.IsLetter(character);

        /// <summary>
        /// Determines if character is a hexadecimal digit
        /// </summary>
        public bool IsHex()
            => IsDecimal() || (CurrentChar >= 'A' && CurrentChar <= 'F') || (CurrentChar >= 'a' && CurrentChar <= 'f');

        public static bool IsHex(char character)
            => IsDecimal(character) || (character >= 'A' && character <= 'F') || (character >= 'a' && character <= 'f');

        /// <summary>
        /// Determines if character is a decimal digit
        /// </summary>
        public bool IsDecimal() => IsDecimal(CurrentChar);

        private void CheckBounds() {
            if (_position < 0) {
                _position = 0;
            }

            int maxPosition = Math.Min(Text.Length, _range.End);

            _isEndOfStream = _position >= maxPosition;
            if (_isEndOfStream) {
                _position = maxPosition;
            }

            CurrentChar = _isEndOfStream ? '\0' : Text[Position];
        }

        /// <summary>
        /// Determines if character is a decimal digit
        /// </summary>
        public static bool IsDecimal(char character) => (character >= '0' && character <= '9');

        /// <summary>
        /// Determines if current character is an ANSI letter
        /// </summary>
        public bool IsAnsiLetter() => IsAnsiLetter(CurrentChar);

        /// <summary>
        /// Determines if current character is an ANSI letter
        /// </summary>
        public static bool IsAnsiLetter(char character)
            => (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z');

        /// <summary>
        /// Determines if current character starts a string (i.e. current character is a single or double quote).
        /// </summary>
        public bool IsAtString() => (CurrentChar == '\'' || CurrentChar == '\"');

        [DebuggerStepThrough]
        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture, "@{0} ({1})", Position, Text[Position]);
    }
}
