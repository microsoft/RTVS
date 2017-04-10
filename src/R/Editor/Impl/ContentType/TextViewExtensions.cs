// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor {
    public static class TextViewExtensions {

        public static SnapshotPoint? MapDownToR(this ITextView textView, SnapshotPoint position) {
            if (textView.BufferGraph == null) {
                // Unit test case
                return position;
            }

            return textView.BufferGraph.MapDownToFirstMatch(
                position,
                PointTrackingMode.Positive,
                x => x.ContentType.IsOfType(RContentTypeDefinition.ContentType),
                PositionAffinity.Successor
            );
        }

        public static NormalizedSnapshotSpanCollection MapDownToR(this ITextView textView, SnapshotSpan span) {
            if (textView.BufferGraph == null) {
                // Unit test case
                return new NormalizedSnapshotSpanCollection(span);
            }

            // There's no convenient method to get all of the lower buffers this span matches to,
            // any of the methods will only map down to a single buffer.  So here we map the span 
            // down, map the span back up to get the range in the top buffer, and then we continue
            // searching from the end of the span that we mapped down and back up.  We stop when
            // we hit the end of the requested spans or when we find no more spans.
            List<SnapshotSpan> spans = new List<SnapshotSpan>();
            for (;;) {
                // map down
                var languageSpans = textView.BufferGraph.MapDownToFirstMatch(
                    span,
                    SpanTrackingMode.EdgeExclusive,
                    x => x.ContentType.IsOfType(RContentTypeDefinition.ContentType)
                );

                // could yield multiple spans, but in the interactive only ever yields one
                int newStart = span.End;
                foreach (var lowerSpan in languageSpans) {
                    // map back up to get the end in the top buffer
                    foreach (var upperSpan in textView.BufferGraph.MapUpToBuffer(
                        lowerSpan,
                        SpanTrackingMode.EdgeInclusive,
                        textView.TextBuffer
                    )) {
                        spans.Add(upperSpan);
                        newStart = upperSpan.End;
                    }
                }

                if (newStart >= span.End) {
                    break;
                }

                // update the span that we're searching for to start at the end of the last span we found
                span = new SnapshotSpan(span.Snapshot, Span.FromBounds(newStart, span.End));
            }

            return new NormalizedSnapshotSpanCollection(spans);
        }

        /// <summary>
        /// Extracts identifier sequence at the caret location.
        /// Fetches parts of 'abc$def' rather than tne entire expression.
        /// If there is selection, returns complete selected item as is.
        /// </summary>
        public static string GetIdentifierUnderCaret(this ITextView textView, out Span span) {
            if (!textView.Caret.InVirtualSpace) {
                if (textView.Selection.Mode == TextSelectionMode.Stream) {
                    SnapshotPoint position = textView.Caret.Position.BufferPosition;
                    ITextSnapshotLine line = null;
                    int caretPosition = -1;
                    if (textView.Selection.SelectedSpans.Count > 0) {
                        span = textView.Selection.SelectedSpans[0];
                        if (span.Length > 0) {
                            return textView.TextBuffer.CurrentSnapshot.GetText(span);
                        }
                    }
                    line = line ?? position.GetContainingLine();
                    caretPosition = caretPosition >= 0 ? caretPosition : position.Position;
                    var item = GetItemAtPosition(line, caretPosition, x => x == RTokenType.Identifier, out span);
                    if (string.IsNullOrEmpty(item)) {
                        item = textView.GetItemBeforeCaret(out span, x => x == RTokenType.Identifier);
                    }
                    return item;
                }
            }
            span = Span.FromBounds(0, 0);
            return string.Empty;
        }

        /// <summary>
        /// Extracts identifier or a keyword before caret. Typically used when inserting
        /// expansions (aka code snippets) at the caret location.
        /// </summary>
        public static string GetItemBeforeCaret(this ITextView textView, out Span span, Func<RTokenType, bool> tokenTypeCheck = null) {
            if (!textView.Caret.InVirtualSpace) {
                SnapshotPoint position = textView.Caret.Position.BufferPosition;
                ITextSnapshotLine line = position.GetContainingLine();
                tokenTypeCheck = tokenTypeCheck ?? new Func<RTokenType, bool>((x) => x == RTokenType.Identifier);
                if (position.Position > line.Start) {
                    return GetItemAtPosition(line, position.Position - 1, tokenTypeCheck, out span);
                }
            }
            span = Span.FromBounds(0, 0);
            return string.Empty;
        }

        public static string GetItemAtPosition(ITextSnapshotLine line, int position, Func<RTokenType, bool> tokenTypeCheck, out Span span) {
            string lineText = line.GetText();
            var offset = 0;
            var positionInTokens = position - line.Start;
            var tokenizer = new RTokenizer();
            var tokens = tokenizer.Tokenize(lineText);
            var tokenIndex = tokens.GetItemContaining(positionInTokens);
            if (tokenIndex >= 0) {
                var token = tokens[tokenIndex];
                if (token.TokenType == RTokenType.Comment) {
                    // Tokenize inside comment since we do want F1 to work inside 
                    // commented out code, code samples or Roxygen blocks.
                    positionInTokens -= token.Start;
                    var positionAfterHash = token.Start + 1;
                    tokens = tokenizer.Tokenize(lineText.Substring(positionAfterHash, token.Length - 1));
                    tokenIndex = tokens.GetItemContaining(positionInTokens);
                    if (tokenIndex >= 0) {
                        token = tokens[tokenIndex];
                        offset = positionAfterHash;
                    }
                }
                if (tokenTypeCheck(token.TokenType)) {
                    var start = token.Start + offset;
                    var end = Math.Min(start + token.Length, line.End);
                    span = Span.FromBounds(line.Start + start, line.Start + end); // return view span
                    return lineText.Substring(start, end - start);
                }
            }

            span = Span.FromBounds(0, 0);
            return string.Empty;
        }

        /// <summary>
        /// Extracts complete variable name under the caret. In '`abc`$`def` returns 
        /// the complete expression rather than its parts. Typically used to get data 
        /// for completion of variable members as in when user typed 'abc$def$'
        /// Since method does not perform semantic analysis, it does not guaratee 
        /// syntactically correct expression, it may return 'a$$b'.
        /// </summary>
        public static string GetVariableNameBeforeCaret(this ITextView textView) {
            if (textView.Caret.InVirtualSpace) {
                return string.Empty;
            }
            var position = textView.Caret.Position.BufferPosition;
            var line = position.GetContainingLine();

            // For performance reasons we won't be using AST here
            // since during completion it is most probably damaged.
            string lineText = line.GetText();
            var tokenizer = new RTokenizer();
            var tokens = tokenizer.Tokenize(lineText);

            var tokenPosition = position - line.Start;
            var index = tokens.GetFirstItemBeforePosition(tokenPosition);
            // Preceding token must be right next to caret position
            if (index < 0 || tokens[index].End < tokenPosition || !IsVariableNameToken(lineText, tokens[index])) {
                return string.Empty;
            }

            if (index == 0) {
                return IsVariableNameToken(lineText, tokens[0]) ? lineText.Substring(tokens[0].Start, tokens[0].Length) : string.Empty;
            }

            // Walk back through tokens allowing identifier and specific
            // operator tokens. No whitespace is permitted between tokens.
            // We have at least 2 tokens here.
            int i = index;
            for (; i > 0; i--) {
                var precedingToken = tokens[i - 1];
                var currentToken = tokens[i];
                if (precedingToken.End < currentToken.Start || !IsVariableNameToken(lineText, precedingToken)) {
                    break;
                }
            }

            return lineText.Substring(tokens[i].Start, tokens[index].End - tokens[i].Start);
        }

        private static bool IsVariableNameToken(string lineText, RToken token) {
            if (token.TokenType == RTokenType.Identifier) {
                return true;
            }
            if (token.TokenType == RTokenType.Operator) {
                if (token.Length == 1) {
                    return lineText[token.Start] == '$' || lineText[token.Start] == '@';
                } else if (token.Length == 2) {
                    return lineText[token.Start] == ':' && lineText[token.Start + 1] == ':';
                }
            }
            return false;
        }
    }
}
