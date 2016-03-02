// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Core.Bytes {
    /// <summary>
    /// Represents stream of bytes (rather than characters). 
    /// Allows reading before stream start of beyond steam end
    /// returning zeroes. Useful when parsing UTF-8 strings.
    /// </summary>
    public class ByteStream {
        private byte[] _text;
        private int _textLength;
        private int _index = 0;
        private int _lastIndex = 0;
        private byte _ch;

        public ByteStream(byte[] text) {
            _text = text;
            _textLength = _text.Length;
            _index = 0;
            _lastIndex = 0;
            _ch = _text.Length > 0 ? _text[0] : (byte)0;
        }

        public bool IsEndOfStream() {
            return _index >= _textLength;
        }

        public int DistanceFromEnd {
            get {
                return _textLength - _index;
            }
        }

        public byte CurrentChar {
            get {
                if (_lastIndex != _index) {
                    if (IsEndOfStream()) {
                        return (byte)0;
                    }

                    _ch = _text[_index];
                    _lastIndex = _index;
                }

                return _ch;
            }
        }

        public byte NextChar {
            get {
                return _index < _textLength - 1 ? _text[_index + 1] : (byte)0;
            }
        }

        public int Position {
            get {
                return _index;
            }

            set {
                _index = value;
            }
        }

        public int Length {
            get {
                return _textLength;
            }
        }

        public bool Advance(int c) {
            if (_index + c <= _textLength) {
                _index += c;
                return true;
            } else {
                _index = _textLength;
            }

            return false;
        }

        public bool MoveToNextChar() {
            return Advance(1);
        }

        public static bool IsWhiteSpace(char ch) {
            return ch <= ' ';
        }

        public bool IsWhiteSpace() {
            return IsWhiteSpace((char)CurrentChar);
        }

        public bool IsDigit() {
            return IsDigit((char)CurrentChar);
        }

        public static bool IsDigit(Char ch) {
            return ch >= '0' && ch <= '9';
        }

        public bool IsNewLineChar() {
            return CurrentChar == '\n' || CurrentChar == '\r';
        }

        public bool IsCharAt(int offset, byte ch) {
            return (_index + offset < _textLength) && (_text[_index + offset] == ch);
        }

        public bool IsAnsiLetter() {
            return (CurrentChar >= 'A' && CurrentChar <= 'z');
        }

        public bool CurrentStringEqualsTo(string s, int length) {
            if (length > (_text.Length - _index))
                return false;

            if (s.Length < length && length < (_text.Length - _index))
                return false;

            for (int i = 0; i < s.Length && i + _index < _text.Length; i++) {
                if (s[i] != _text[i + _index]) {
                    return false;
                }
            }

            return true;
        }
    }
}
