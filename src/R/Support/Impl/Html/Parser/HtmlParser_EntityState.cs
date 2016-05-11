// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser {
    public sealed partial class HtmlParser {
        internal void OnEntityState() {
            Debug.Assert(_cs.CurrentChar == '&');

            int start = _cs.Position;

            // Try to parse entity. If fail, rewind back and return letting text state 
            // process stream as text. 

            // HMTL4 defines 252 entities and does not include &apos; while XML only 
            // supports 5 and XHTML defines 253 (including &apos;) so it is technically 
            // schema-dependent information... We could just treat anything that is
            // &{non-ws-sequence}; as an entity and leave validation to sort it out, 
            // but then we'll treat 'a&b as an entity which is wrong. It is also nice 
            // to have known entities colorized properly. So we'll assume XHTML.
            // If there is perf issue with the search, it can be switched off.

            // &{ident}; and &#nnnn; and &#xhhhh; 
            // The nnnn or hhhh may be any number of digits and may include leading zeros.
            // Standard says semicolon is required but browsers don't require it
            // if whitespace follows the entity.

            // Old VS parser only recognized 16 bit Unicode, but GB18030 is longer than that
            // and Unicode actually has 17 x 16 bit planes (up to 10FFFF). So we'll
            // recognize numbers up to 17 x 2^16 = 1114111 (dec) or 10FFFF (hex).

            _cs.MoveToNextChar();

            bool invalidEntity = false;

            if (_cs.CurrentChar == '#') {
                int numDigits = 0;
                // numercal entity
                if (_cs.NextChar == 'x' || (ParsingMode == ParsingMode.Html && _cs.NextChar == 'X')) {
                    _cs.Advance(2);
                    // hex entity, up to 10FFFF value (6 digits).
                    for (numDigits = 0; numDigits < 6; numDigits++) {
                        if (_cs.IsEndOfStream() || _cs.IsWhiteSpace())
                            break;

                        if (_cs.CurrentChar == ';') {
                            _cs.MoveToNextChar();
                            break;
                        }

                        if (!_cs.IsHex()) {
                            invalidEntity = true;
                            break;
                        }

                        _cs.MoveToNextChar();
                    }

                    // Number seems to be valid, but may be too large. Check it.
                    if (!invalidEntity && numDigits == 6) {
                        char ch1 = _cs[start + 2];
                        char ch2 = _cs[start + 3];

                        if (ch1 > '1' || (ch1 == '1' && ch2 > '0')) {
                            invalidEntity = true;
                        }
                    }
                } else {
                    _cs.MoveToNextChar();

                    // verify decimal, up to 1114111 (7 digits)
                    int value = 0;
                    for (numDigits = 0; numDigits < 7; numDigits++) {
                        if (_cs.IsEndOfStream() || _cs.IsWhiteSpace())
                            break;

                        if (_cs.CurrentChar == ';') {
                            _cs.MoveToNextChar();
                            break;
                        }

                        if (!_cs.IsDecimal()) {
                            invalidEntity = true;
                            break;
                        }

                        value = 10 * value + ((int)_cs.CurrentChar - (int)'0');
                        _cs.MoveToNextChar();
                    }

                    if (value > 1114111)
                        invalidEntity = true;
                }

                if (invalidEntity) {
                    _cs.Position = start + 1;
                    return;
                } else {
                    if (EntityFound != null)
                        EntityFound(this, new HtmlParserRangeEventArgs(TextRange.FromBounds(start, _cs.Position)));

                    return;
                }
            }

            // Character entity. Technically can be of any length, but there are not known
            // entities longer than 8 characters and we are going to check against
            // known entity table anyway.

            int numChars = 0;
            for (numChars = 0; numChars < 10; numChars++) {
                if (_cs.IsEndOfStream() || _cs.IsWhiteSpace()) {
                    invalidEntity = true;
                    break;
                }

                if (_cs.CurrentChar == ';') {
                    _cs.MoveToNextChar();
                    break;
                }

                if (!_cs.IsAnsiLetter()) {
                    invalidEntity = true;
                    break;
                }

                _cs.MoveToNextChar();
            }

            if (numChars > 8)
                invalidEntity = true;

            if (!invalidEntity) {
                char mapperChar;
                if (!EntityTable.IsEntity(_cs.GetSubstringAt(start + 1, numChars), out mapperChar))
                    invalidEntity = true;
            }

            if (invalidEntity) {
                _cs.Position = start + 1;
                return;
            }

            if (EntityFound != null)
                EntityFound(this, new HtmlParserRangeEventArgs(TextRange.FromBounds(start, _cs.Position)));
        }
    }
}
