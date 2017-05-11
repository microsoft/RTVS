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
        private readonly byte[] _text;
        private int _lastIndex;
        private byte _ch;

        public ByteStream(byte[] text) {
            _text = text;
            Length = _text.Length;
            _lastIndex = 0;
            _ch = _text.Length > 0 ? _text[0] : (byte)0;
        }

        public bool IsEndOfStream() => Position >= Length;
        public int DistanceFromEnd => Length - Position;

        public byte CurrentChar {
            get {
                if (_lastIndex != Position) {
                    if (IsEndOfStream()) {
                        return (byte)0;
                    }

                    _ch = _text[Position];
                    _lastIndex = Position;
                }

                return _ch;
            }
        }

        public byte NextChar => Position < Length - 1 ? _text[Position + 1] : (byte)0;
        public int Position { get; set; }
        public int Length { get; }

        public bool Advance(int c) {
            if (Position + c <= Length) {
                Position += c;
                return true;
            } else {
                Position = Length;
            }

            return false;
        }

        public bool MoveToNextChar() => Advance(1);
        public static bool IsWhiteSpace(char ch) => ch <= ' ';
        public bool IsWhiteSpace() => IsWhiteSpace((char)CurrentChar);
        public bool IsDigit() => IsDigit((char)CurrentChar);
        public static bool IsDigit(Char ch) => ch >= '0' && ch <= '9';
        public bool IsNewLineChar() => CurrentChar == '\n' || CurrentChar == '\r';

        public bool IsCharAt(int offset, byte ch) {
            return (Position + offset < Length) && (_text[Position + offset] == ch);
        }

        public bool IsAnsiLetter() => (CurrentChar >= 'A' && CurrentChar <= 'z');

        public bool CurrentStringEqualsTo(string s, int length) {
            if (length > (_text.Length - Position)) {
                return false;
            }

            if (s.Length < length && length < (_text.Length - Position)) {
                return false;
            }

            for (int i = 0; i < s.Length && i + Position < _text.Length; i++) {
                if (s[i] != _text[i + Position]) {
                    return false;
                }
            }

            return true;
        }
    }
}
