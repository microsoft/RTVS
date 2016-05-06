// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.Html.Core.Parser {
    public sealed partial class HtmlParser {
        int prevPosition;
        // Top level state. Parsing begins here.
        // Expected objects: text, artifacts, <, </, entities.
        internal void OnTextState() {
            while (!_cs.IsEndOfStream() && _cs.Position < _softRangeEnd) {
                prevPosition = _cs.Position;

                if (_cs.CurrentChar == '<') {
                    if (_cs.NextChar == '/') {
                        OnEndTagState();
                    } else if (_cs.NextChar == '!' && _cs.LookAhead(2) == '-' && _cs.LookAhead(3) == '-') {
                        // TODO: do we need to recognize SGML comments? Of handling <!-- --> as delimiters
                        // is OK for editing purposes?

                        // HTML 5: http://www.w3.org/TR/html5/syntax.html#comments
                        // Comments must start with the four character sequence 
                        // U+003C LESS-THAN SIGN, U+0021 EXCLAMATION MARK, U+002D HYPHEN-MINUS, U+002D HYPHEN-MINUS (<!--). 
                        // Following this sequence, the comment may have text, with the additional restriction that the text 
                        // must not start with a single U+003E GREATER-THAN SIGN character (>), nor start with a U+002D HYPHEN-MINUS 
                        // character (-) followed by a U+003E GREATER-THAN SIGN (>) character, nor contain two consecutive 
                        // U+002D HYPHEN-MINUS characters (--), nor end with a U+002D HYPHEN-MINUS character (-). 
                        // Finally, the comment must be ended by the three character sequence U+002D HYPHEN-MINUS, 
                        // U+002D HYPHEN-MINUS, U+003E GREATER-THAN SIGN (-->).

                        // In HTML4 and earlier SGML rules apply: comment is a <! > block and comment actually begins
                        // and ends with -- and -- inside that block. For example, in <! -- AAA -- BB --> AA is 
                        // in a comment while BB is not (and neither last -- designate any comment.
                        OnCommentState();
                    } else {
                        OnStartTagState();
                    }
                } else if (_cs.CurrentChar == '&') {
                    OnEntityState();
                } else {
                    _cs.MoveToNextChar();
                }

                Debug.Assert(prevPosition < _cs.Position);
            }
        }
    }
}
