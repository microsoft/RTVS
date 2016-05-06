// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Html.Core.Parser.Tokens;

namespace Microsoft.Html.Core.Parser {
    public sealed partial class HtmlParser {
        internal AttributeToken OnAttributeState(bool artifactTag = false) {
            return OnAttributeState(_cs.Position + _cs.DistanceFromEnd, artifactTag);
        }

        internal AttributeToken OnAttributeState(int tagEnd, bool artifactTag = false) {
            AttributeToken attributeToken = GetAttributeState(tagEnd, artifactTag, true);

            if ((AttributeFound != null) && (attributeToken != null))
                AttributeFound(this, new HtmlParserAttributeEventArgs(attributeToken));

            return attributeToken;
        }

        internal AttributeToken GetAttributeState(int tagEnd, bool artifactTag, bool getValueInfo) {
            int eqPos = -1;
            bool isScript = false;
            IHtmlToken nameToken = null;
            AttributeToken attributeToken = null;

            _tokenizer.SkipWhitespace();
            if (!_cs.IsEndOfStream()) {
                nameToken = _tokenizer.GetNameToken(tagEnd, artifactTag);

                // Allow whitespace before = per HTML standard
                _tokenizer.SkipWhitespace();

                if (_cs.CurrentChar != '=' && nameToken != null) {
                    // standalone attribute
                    return new AttributeToken(nameToken);
                }

                if (_cs.CurrentChar == '=') {
                    eqPos = _cs.Position;
                    _cs.MoveToNextChar();
                }

                IHtmlAttributeValueToken value = null;
                if (getValueInfo) {
                    // Allow whitespace before = per HTML standard
                    _tokenizer.SkipWhitespace();

                    // Check if attribute name begins with 'on' which means its value is script like 
                    // onclick="...". Script can legally include < > (like in if(x < y)... so we are 
                    // going to assume that everything between quotes is the script code. Note also 
                    // that script should always be quoted. If quote is missing, we assume that 
                    // attribute has no value. We cannot tell if attribute is script if attribute name
                    // is an artifact, so we don't support <% %> = "script code".
                    if (_cs.IsAtString() && nameToken != null && nameToken.Length >= 2) {
                        char c1 = _cs[nameToken.Start];
                        char c2 = _cs[nameToken.Start + 1];

                        if ((c1 == 'o' || c1 == 'O') && (c2 == 'n' || c2 == 'N')) {
                            isScript = true;
                        }
                    }

                    value = GetAttributeValue(isScript, tagEnd);

                    // In some odd cases we may end up with no name, no equals sign and no value.
                    // Check if this is the case and if so, advance character stream position
                    // and try again.
                    if (nameToken != null || eqPos >= 0 || value != null) {
                        attributeToken = new AttributeToken(nameToken, eqPos, value);
                    } else {
                        // We could not make sense at all - move on.
                        _cs.MoveToNextChar();
                    }
                }
            }

            return attributeToken;
        }

        IHtmlAttributeValueToken GetAttributeValue(bool isScript, int tagEnd) {
            if (_cs.IsAtString())
                return _tokenizer.GetQuotedAttributeValue(isScript, tagEnd);

            // Value is not quoted so it can only consist of a single item
            return _tokenizer.GetUnquotedAttributeValue(tagEnd);
        }
    }
}
