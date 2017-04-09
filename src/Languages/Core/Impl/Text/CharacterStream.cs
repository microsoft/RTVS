// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Helper class that represents stream of characters for a parser or tokenizer
    /// </summary>
    [DebuggerDisplay("[{Text} {CurrentChar}]")]
    public class CharacterStream {
        private char _currentChar;
        private ITextProvider _text;
        private TextRange _range;
        private int _position = 0;
        private bool _isEndOfStream = false;

        #region Constructors

        [DebuggerStepThrough]
        public CharacterStream(ITextProvider textProvider)
            : this(textProvider, TextRange.FromBounds(0, textProvider.Length)) {
        }

        public CharacterStream(ITextProvider textProvider, ITextRange range) {
            _text = textProvider;

            int end = Math.Min(_text.Length, range.End);

            _range = TextRange.FromBounds(range.Start, end);

            Position = _range.Start;
            _currentChar = _text[_range.Start];
        }

        [DebuggerStepThrough]
        public CharacterStream(string text)
            : this(new TextStream(text)) {
        }
        #endregion

        /// <summary>
        /// Text provider that supplies underlying text. May be a string, a text buffer or a buffer snapshot.
        /// </summary>
        public ITextProvider Text {
            get { return _text; }
        }

        /// <summary>
        /// Determines if current position is at the end of text
        /// </summary>
        /// <returns>True if position is at the end of stream</returns>
        public bool IsEndOfStream() {
            return _isEndOfStream;
        }

        public int DistanceFromEnd {
            get { return _range.End - Position; }
        }

        /// <summary>
        /// Returns character at a given position. If position is beyond text limits, returns '\0'
        /// </summary>
        /// <param name="position">Stream position</param>
        public char this[int position] {
            get {
                return _text[position];
            }
        }

        public string GetSubstringAt(int position, int length) {
            return _text.GetText(new TextRange(position, length));
        }

        public int IndexOf(string text, int start, bool ignoreCase) {
            return _text.IndexOf(text, start, ignoreCase);
        }

        public int IndexOf(char ch, int start) {
            return _text.IndexOf(ch, start);
        }

        public bool CompareTo(int position, int length, string text, bool ignoreCase) {
            return _text.CompareTo(position, length, text, ignoreCase);
        }

        public char CurrentChar { get { return _currentChar; } }

        public char NextChar {
            get { return Position + 1 < _range.End ? _text[Position + 1] : '\0'; }
        }

        public char PrevChar {
            get { return Position > _range.Start ? _text[Position - 1] : '\0'; }
        }

        /// <summary>
        /// Returns characters at an offset from the current position
        /// </summary>
        /// <param name="offset">Offset from the current position</param>
        /// <returns>Character or '\0' if offset is beyond text boundaries</returns>
        public char LookAhead(int offset) {
            int pos = Position + offset;

            if (pos < 0 || pos >= _text.Length)
                return '\0';

            return _text[pos];
        }

        /// <summary>
        /// Current stream position
        /// </summary>
        public int Position {
            get {
                return _position;
            }
            set {
                _position = value;
                CheckBounds();
            }
        }

        /// <summary>
        /// Length of the stream
        /// </summary>
        public int Length {
            get { return _range.Length; }
        }

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
                _currentChar = _text[_position];
                return true;
            }

            Advance(1);
            return !_isEndOfStream;
        }

        /// <summary>
        /// Detemines if current character is a whitespace
        /// </summary>
        public bool IsWhiteSpace() {
            // Char.IsWhiteSpace is slow
            return _currentChar <= ' ' || _currentChar == 0x200B;
        }

        /// <summary>
        /// Determines if current character is a new line character
        /// </summary>
        public bool IsAtNewLine() {
            return IsNewLine(_currentChar);
        }

        public static bool IsNewLine(char currentCharacter) {
            return (currentCharacter == '\n' || currentCharacter == '\r');
        }

        public void SkipLineBreak() {
            if (CurrentChar == '\n') {
                MoveToNextChar();
                if (CurrentChar == '\r')
                    MoveToNextChar();
            } else if (CurrentChar == '\r') {
                MoveToNextChar();
                if (CurrentChar == '\n')
                    MoveToNextChar();
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
        public bool IsLetter() {
            return IsLetter(_currentChar);
        }

        /// <summary>
        /// Determines if current character is a letter
        /// </summary>
        public static bool IsLetter(char character) {
            return char.IsLetter(character);
        }

        /// <summary>
        /// Determines if character is a hexadecimal digit
        /// </summary>
        public bool IsHex() {
            return IsDecimal() || (_currentChar >= 'A' && _currentChar <= 'F') || (_currentChar >= 'a' && _currentChar <= 'f');
        }

        public static bool IsHex(char character) {
            return IsDecimal(character) || (character >= 'A' && character <= 'F') || (character >= 'a' && character <= 'f');
        }

        /// <summary>
        /// Determines if character is a decimal digit
        /// </summary>
        public bool IsDecimal() {
            return IsDecimal(_currentChar);
        }

        private void CheckBounds() {
            if (_position < 0)
                _position = 0;

            int maxPosition = Math.Min(_text.Length, _range.End);

            _isEndOfStream = _position >= maxPosition;
            if (_isEndOfStream)
                _position = maxPosition;

            _currentChar = _isEndOfStream ? '\0' : _text[Position];
        }

        /// <summary>
        /// Determines if character is a decimal digit
        /// </summary>
        public static bool IsDecimal(char character) {
            return (character >= '0' && character <= '9');
        }

        /// <summary>
        /// Determines if current character is an ANSI letter
        /// </summary>
        public bool IsAnsiLetter() {
            return IsAnsiLetter(_currentChar);
        }

        /// <summary>
        /// Determines if current character is an ANSI letter
        /// </summary>
        public static bool IsAnsiLetter(char character) {
            return (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z');
        }

        /// <summary>
        /// Determines if current character starts a string (i.e. current character is a single or double quote).
        /// </summary>
        public bool IsAtString() {
            return (_currentChar == '\'' || _currentChar == '\"');
        }
  
        [DebuggerStepThrough]
        public override string ToString() {
            return string.Format(CultureInfo.InvariantCulture, "@{0} ({1})", Position, _text[Position]);
        }
    }
}
