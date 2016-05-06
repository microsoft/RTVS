// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    // Note that primary goal of this tokenizer is to be part of an editor. This means it 
    // cannot simply follow HTML BNF rules and tokenize to the spec. Instead, it must be able
    // to handle incomplete constructs and help parser and tree builder to construct a tree 
    // that then intellisense and colorizer can use. For example, in
    //
    //  <element foo="bar="some_value"> or
    //  <div id="nowrap>
    // 
    // standard tokenizer would interpret "bar=" as a value of foo attribute. This is however, 
    // quite useless for the editor since most probably user just typed foo=" and expects 
    // intellisense to bring list of values for foo. Thus, foo=" should be parsed as 
    // 'unterminated attribute value' and bar="some_value" must come as a separate 
    // and well-formed attribute.
    //
    // Worse still, consider
    //
    //  <a b="c d ="e"    
    //  <f g="h <i j="k"
    //
    // Unfortunately, many of such cases are very ambiguous and we have to resort to heuristics 
    // and implement some semantic analysis here in the tokenizer. As a side effect user may 
    // have to put entities into values to make editor work correctly since
    //
    //  <input type="button" value="foo=bar" /> 
    //
    // is actually a legitimate, albeit rare piece of HTML. It is difficult to distinguish from, 
    //
    //  <input type="button" value="foo=bar"
    //    
    // Logic:
    //
    //  a. Find out if string is suspicious. It is suspicious if it contains leading or trailing
    //     spaces since according to CDATA definition http://www.w3.org/TR/html4/types.html#type-cdata
    //     "User agents may ignore leading and trailing white space in CDATA attribute values 
    //      (e.g., "   myval   " may be interpreted as "myval"). Authors should not declare attribute 
    //       values with leading or trailing white space.
    //
    //  b. Quote followed by a newline means string is not terminated
    //  c. String ending in = is unusual
    //
    //  d. When handling string, look if it contains '=', '<name', '>' or newline. If so, try make 
    //     sense out of it by stopping at < or newline
    //
    //  e. When parsing element start tag, look ahead for > the walk text forwards and backwards. 
    //     Reverse walk should be able to help with <a b="c d ="e" and correctly find that 'e' 
    //     is value and 'd' is attribute name while "c is unterminated string.
    //
    // HTML tokenizer is stateful, it is not a plain lexer. For example, entities (&lt; &amp;)
    // can appear in text section (element content and attribute values) but they are not valid
    // as, say, an element name.
    //
    // Since tokenizer effectively handles 'work in progress', we are going to follow relaxed rules
    // and assume that in <1 2> '1' is element name and '2' is a standalone attribute even that element 
    // and attribute names can't begin with a number. We'll leave it to validation code
    // to flag the error and tell user that 'element name cannot begin with a number'.
    //
    // Note also that this is performance-critical piece so we err on not handling
    // some valid, but rare/exotic cases like <a b="c<d" e="f" and rather encourage users 
    // to use entities in attribute values. VS does the same and there have been very few 
    // complaints about this since workaround is trivial: <a b="c&amp;d" which is good thing
    // since it make file conformant to XML. And &amp; is required in XHTML anyway since
    // XML does not allow < in attribute values, even when quoted.

    internal class HtmlTokenizer {
        HtmlCharStream _cs;
        StringClosure _stringClosure;
        public HtmlTokenizer(HtmlCharStream cs) {
            _cs = cs;
            _stringClosure = new StringClosure(cs);
        }

        /// <summary>
        /// Retrieves unquoted attribute value
        /// </summary>
        /// <returns>Attribute value token</returns>
        public IHtmlAttributeValueToken GetUnquotedAttributeValue(int tagEnd) {
            // Need to handle <a b=c=d /> and figure out that c=d is a valid pair while 
            // b= is missing a value. One way is to check if 'c' happens to be
            // one of the known HTML attributes, but it only works for plain HTML 
            // and won't work for ASP.NET controls that can define any attributes
            // via .NET properties.

            // Options (value is unquoted)
            //      a. Attribute value is either a sequence of characters or a number
            //         except if it is ID or CLASS. 
            //      b. Attribute name is typically a sequence of characters and does not include digits
            //      c. Attribute value is not normally followed by =

            Debug.Assert(!_cs.IsAtString());

            AttributeValueToken token = null;
            int start = _cs.Position;
            int end = _cs.Position;

            ITextRange nextTokenRange;
            NextTokenType nextTokenType = NextToken.PeekNextToken(_cs, tagEnd, out nextTokenRange);

            switch (nextTokenType) {
                case NextTokenType.None:    // attrName={EOF}
                case NextTokenType.Tag:     // attName=<foo
                case NextTokenType.Equals:  // attrName==
                    return null;

                case NextTokenType.Number:
                // attrName = 1.0
                case NextTokenType.Unknown:
                // id=#
                case NextTokenType.Identifier:
                    // attrName = foo12. There are no know attributes of this form. 
                    end = nextTokenRange.End;
                    break;

                case NextTokenType.Letters:
                    // Legal value. Need to check against attribute names to be sure.
                    bool isKnownAttribute = AttributeTable.IsKnownAttribute(_cs.GetSubstringAt(nextTokenRange.Start, nextTokenRange.Length));
                    if (!isKnownAttribute)
                        end = nextTokenRange.End;
                    break;
            }

            char closeQuote = '\0';
            if (end > start) {
                closeQuote = _cs[end - 1];
                if (closeQuote != '\'' && closeQuote != '\"')
                    closeQuote = '\0';
            }

            token = AttributeValueToken.Create(HtmlToken.FromBounds(start, end), '\0', closeQuote);
            _cs.Position = end;

            return token;
        }

        /// <summary>
        /// Retrieves quoted attribute value token at the current character stream position
        /// </summary>
        /// <param name="isScript">True if attribute is a script, like onclick</param>
        /// <returns>Attribute value token</returns>
        public IHtmlAttributeValueToken GetQuotedAttributeValue(bool isScript, int tagEnd) {
            Debug.Assert(_cs.IsAtString());

            int start = _cs.Position;
            char openQuote = _cs.CurrentChar;

            int stringEnd = _stringClosure.GetStringClosureLocation(tagEnd);
            _cs.Position = start;

            var valueTokens = new List<IHtmlToken>();
            valueTokens.Add(HtmlToken.FromBounds(HtmlTokenType.String, _cs.Position, stringEnd));

            char closeQuote = '\0';
            if (openQuote != '\0' && stringEnd - 1 != start) {
                closeQuote = _cs[stringEnd - 1];
                if (closeQuote != openQuote)
                    closeQuote = '\0';
            }

            _cs.Position = stringEnd;

            if (valueTokens.Count < 2) {
                if (isScript)
                    return new ScriptAttributeValueToken(valueTokens[0], openQuote, closeQuote);
                else
                    return AttributeValueToken.Create(valueTokens[0], openQuote, closeQuote);
            } else
                return new CompositeAttributeValueToken(valueTokens.ToArray(), openQuote, closeQuote, isScript);
        }

        // This is the most complex case (see comments above)
        public IHtmlToken[] GetComment() {
            int start = _cs.Position;
            var tokens = new List<IHtmlToken>();

            Debug.Assert(_cs.CurrentChar == '<' && _cs.LookAhead(1) == '!' && _cs.LookAhead(2) == '-' && _cs.LookAhead(3) == '-');
            _cs.Advance(4);

            bool continueLoop = true;

            while (continueLoop) {
                if (_cs.IsEndOfStream()) {
                    // If we hit end of stream, the comment is not well formed. We are going to add 
                    // final token even if it is zero-length so we can expand it when user types 
                    // at the end of the file. The token though should be marked as malformed.

                    tokens.Add(HtmlToken.FromBounds(HtmlTokenType.Comment, start, _cs.Position, wellFormed: false));
                    break;
                }

                if (_cs.CurrentChar == '-' && _cs.NextChar == '-' && _cs.LookAhead(2) == '>') {
                    _cs.Advance(3);

                    tokens.Add(HtmlToken.FromBounds(HtmlTokenType.Comment, start, _cs.Position, wellFormed: true));
                    break;
                }

                _cs.MoveToNextChar();
            }

            return tokens.ToArray();
        }

        // Similar to GetNextToken but also handles namespaces
        // It is typically called when parser is entering start tag.
        // Note that we do not allow artifacts in names
        public NameToken GetNameToken(int tagEnd = Int32.MaxValue, bool artifactTag = false) {
            // < at the end of file or missing tag name
            if (_cs.IsEndOfStream() || _cs.IsAtString() || _cs.IsWhiteSpace() || _cs.CurrentChar == '=' || _cs.CurrentChar == '/')
                return null;

            int start = _cs.Position;
            int colonPos = -1;

            // Technically namespace and name starts with ANSI letter or underscore. Subsequent characters 
            // can include digits except if it is second character after leading underscore, i.e. _2 is illegal 
            // and also all underscores are illegal. However, we'll leave validation to the validator and will 
            // only handle : and = as separators

            while (!_cs.IsEndOfStream() && _cs.Position < tagEnd && !_cs.IsWhiteSpace() && _cs.CurrentChar != '=' && _cs.CurrentChar != '/') {
                if (!artifactTag && _cs.IsAtTagDelimiter())
                    break;

                if (colonPos < 0 && _cs.CurrentChar == ':')
                    colonPos = _cs.Position;

                _cs.MoveToNextChar();
            }

            bool hasPrefix = colonPos > start;
            bool hasName = false;
            int nameStart = -1;
            int nameEnd = -1;
            if (hasPrefix || colonPos >= 0) {
                hasName = _cs.Position > colonPos + 1;
                if (hasName) {
                    nameStart = colonPos + 1;
                    nameEnd = _cs.Position;
                }
            } else {
                hasName = _cs.Position > start;
                if (hasName) {
                    nameStart = start;
                    nameEnd = _cs.Position;
                }
            }

            if (!hasPrefix && colonPos < 0 && !hasName)
                return null;

            if (hasPrefix || colonPos >= 0) {
                return NameToken.Create(start, colonPos - start, colonPos, nameStart, nameEnd - nameStart);
            } else {
                return NameToken.Create(nameStart, nameEnd - nameStart);
            }
        }

        public void SkipWhitespace() {
            while (!_cs.IsEndOfStream()) {
                // Razor artifacts can start with whitespace so we have to check
                // if we are currently at an artifact. 
                if (!_cs.IsWhiteSpace())
                    break;

                _cs.MoveToNextChar();
            }
        }
    }
}
