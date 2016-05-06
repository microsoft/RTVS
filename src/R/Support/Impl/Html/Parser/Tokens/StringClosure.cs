// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    /// <summary>
    /// Type of token in a string
    /// </summary>
    internal enum StringTokenType {
        OpenTag, // <
        CloseTag, // >
        SelfCloseTag, // />
        EqualsSign,
        Whitespace,
        NewLine,
        Quote,
        Artifact,
        Content
    }

    internal struct StringToken {
        public StringTokenType Type;
        public int Start;
        public int End;
        public int Length { get { return End - Start; } }

        public StringToken(StringTokenType t, int start, int end) {
            Type = t;
            Start = start;
            End = end;
        }

        public StringToken(StringTokenType t, int start) {
            Type = t;
            Start = start;
            End = start + 1;
        }
    }

    /// <summary>
    /// Helps to find out where quoted attribute value should be closed
    /// when dealing with partially typed constructs like &lt;a b=" &lt;b
    /// or &lt;a b="c="d" &gt;
    /// </summary>
    internal class StringClosure {
        private readonly HtmlCharStream _cs;

        public StringClosure(HtmlCharStream cs) {
            _cs = cs;
        }

        /// <summary>
        /// Check string for suspicious content (if it contains markup, equal signs etc)
        /// that may tell if string is really an attribute that is missing its closing quote.
        /// </summary>
        /// <returns>String closure information, i.e. where it is better to terminate the string</returns>
        internal int GetStringClosureLocation(int tagEnd) {
            Debug.Assert(_cs.IsAtString());

            char quote = _cs.CurrentChar;
            _cs.MoveToNextChar();

            int positionAfterFirstQuote = _cs.Position;

            // Tokenize string content until closing quote or end of stream or end 
            // after 1K characters since attribute values are never really that long.
            List<StringToken> tokens = TokenizeStringContent(quote, tagEnd);

            if (tokens.Count == 0)
                return positionAfterFirstQuote;

            int firstMarkupLocation = -1;
            int lastWhitespaceLocationOnFirstLine = -1;
            bool newLineSeen = false;

            for (int i = 0; i < tokens.Count; i++) {
                StringToken curToken = tokens[i];
                switch (curToken.Type) {
                    case StringTokenType.NewLine:
                        if (!newLineSeen) {
                            lastWhitespaceLocationOnFirstLine = curToken.Start;
                            newLineSeen = true;
                        }

                        if (firstMarkupLocation == -1)
                            firstMarkupLocation = curToken.Start;
                        break;
                    case StringTokenType.CloseTag:
                    case StringTokenType.OpenTag:
                    case StringTokenType.SelfCloseTag:
                        if (newLineSeen)
                            return firstMarkupLocation;

                        // multiline attributes are quite uncommon and having < or > in them at the same time is odd.
                        if (firstMarkupLocation == -1)
                            firstMarkupLocation = curToken.Start;
                        break;
                    case StringTokenType.Whitespace:
                        // Keep track off the last whitespace seen on the initial line
                        if (!newLineSeen)
                            lastWhitespaceLocationOnFirstLine = curToken.Start;
                        break;
                    case StringTokenType.EqualsSign:
                        // This indicates an equals sign right before the string ended. This is suspicious, so we'll
                        //    use the last whitespace location location found in the first line of the string.
                        if (lastWhitespaceLocationOnFirstLine != -1)
                            return lastWhitespaceLocationOnFirstLine;

                        break;
                    case StringTokenType.Quote:
                        return curToken.End;
                }
            }

            // The string wasn't closed before the end of the given range. If we found markup, use that as the string end.
            if (firstMarkupLocation != -1)
                return firstMarkupLocation;

            return tokens[tokens.Count - 1].End;
        }

        /// <summary>
        /// Simple string content tokenizer that helps determine where
        /// quoted attribute value must be closed.
        /// </summary>
        /// <param name="cs">Character stream</param>
        /// <param name="quote">Type of opening quote</param>
        /// <returns>List of tokens</returns>
        List<StringToken> TokenizeStringContent(char quote, int tagEnd) {
            int startPosition = _cs.Position;
            List<StringToken> tokens = new List<StringToken>();

            // We don't allow space after opening quote, we assume string is not 
            // closed as in <a b=" c="d">. Whitespace in the beginning of an
            // attribute value is unusual and is not recommended by W3C.
            if (_cs.IsWhiteSpace())
                return tokens;

            while (!_cs.IsEndOfStream() && _cs.Position < tagEnd) {
                StringToken? tokenToAdd = null;
                switch (_cs.CurrentChar) {
                    case '<':
                        tokenToAdd = new StringToken(StringTokenType.OpenTag, _cs.Position);
                        break;

                    case '=':
                        if (_cs.NextChar == quote) {
                            int remainingQuotesOnLine = CountRemainingQuotesOnLine(tagEnd, quote);
                            if (remainingQuotesOnLine % 2 == 0) {
                                // Only return EqualsSign token if the next char is a quote terminating the string
                                //   and there are an even number of quotes remaining on the line including it.
                                tokenToAdd = new StringToken(StringTokenType.EqualsSign, _cs.Position);
                            }
                        }

                        break;

                    case '/':
                        if (_cs.NextChar == '>') {
                            tokenToAdd = new StringToken(StringTokenType.SelfCloseTag, _cs.Position, _cs.Position + 2);
                        }

                        break;

                    case '>':
                        tokenToAdd = new StringToken(StringTokenType.CloseTag, _cs.Position);
                        break;

                    case '\r': {
                            int start = _cs.Position;

                            _cs.MoveToNextChar();
                            if (_cs.CurrentChar == '\n')
                                _cs.MoveToNextChar();

                            tokenToAdd = new StringToken(StringTokenType.NewLine, start, _cs.Position);
                        }
                        break;

                    case '\n':
                        tokenToAdd = new StringToken(StringTokenType.NewLine, _cs.Position);
                        break;

                    default:
                        if (_cs.CurrentChar == quote) {
                            tokenToAdd = new StringToken(StringTokenType.Quote, _cs.Position);
                        } else if (_cs.IsWhiteSpace()) {
                            int start = _cs.Position;
                            _cs.MoveToNextChar();

                            while (_cs.IsWhiteSpace() && !_cs.IsAtNewLine())
                                _cs.MoveToNextChar();

                            tokenToAdd = new StringToken(StringTokenType.Whitespace, start, _cs.Position);
                        }

                        break;
                }

                if (tokenToAdd != null) {
                    AddPendingContentToken(tokens, startPosition, tokenToAdd.Value.Start);

                    tokens.Add(tokenToAdd.Value);
                    _cs.Position = tokenToAdd.Value.End;

                    if (tokenToAdd.Value.Type == StringTokenType.Quote) {
                        break;
                    }
                } else {
                    _cs.MoveToNextChar();
                }
            }

            VerifyTokenList(tokens);

            _cs.Position = startPosition;
            return tokens;
        }

        private int CountRemainingQuotesOnLine(int tagEnd, char quote) {
            int startPosition = _cs.Position;
            int quoteCount = 0;

            while (!_cs.IsEndOfStream() && _cs.Position < tagEnd) {
                char currentChar = _cs.CurrentChar;
                if (currentChar == quote) {
                    quoteCount++;
                } else if (CharacterStream.IsNewLine(currentChar)) {
                    break;
                }

                _cs.MoveToNextChar();
            }

            // reset the stream back to it's original position
            _cs.Position = startPosition;

            return quoteCount;
        }

        [Conditional("DEBUG")]
        private void VerifyTokenList(List<StringToken> tokens) {
            if (tokens.Count >= 2) {
                for (int i = 1; i < tokens.Count; i++) {
                    if (tokens[i].Start != tokens[i - 1].End) {
                        Debug.Assert(false, "missing location from token list");
                    }
                }
            }
        }

        private static void AddPendingContentToken(List<StringToken> tokens, int stringStart, int currentTokenStart) {
            int prevTokenEnd = stringStart;
            if (tokens.Count > 0) {
                prevTokenEnd = tokens[tokens.Count - 1].End;
            }

            if (prevTokenEnd != currentTokenStart) {
                // there was some space between the last token added and this new token, create a Content token for that space
                // TODO: might be off by 1
                tokens.Add(new StringToken(StringTokenType.Content, prevTokenEnd, currentTokenStart));
            }
        }
    }
}
