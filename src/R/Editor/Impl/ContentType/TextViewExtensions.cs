// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor {
    public static class TextViewExtensions {

        public static SnapshotPoint? MapDownToR(this ITextView textView, int position) {
            if (textView.BufferGraph == null) {
                // Unit test case
                return new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, position);
            }

            return textView.BufferGraph.MapDownToFirstMatch(
                new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, position),
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
        /// </summary>
        public static string GetIdentifierUnderCaret(this ITextView textView, out Span span) {
            if (!textView.Caret.InVirtualSpace) {
                SnapshotPoint position = textView.Caret.Position.BufferPosition;
                ITextSnapshotLine line = position.GetContainingLine();
                return GetItemAtPosition(line, position.Position, (x) => x == RTokenType.Identifier, out span);
            }
            span = Span.FromBounds(0, 0);
            return string.Empty;
        }

        /// <summary>
        /// Extracts identifier or a keyword before caret. Typically used when inserting
        /// expansions (aka code snippets) at the caret location.
        /// </summary>
        public static string GetItemBeforeCaret(this ITextView textView, out Span span) {
            if (!textView.Caret.InVirtualSpace) {
                SnapshotPoint position = textView.Caret.Position.BufferPosition;
                ITextSnapshotLine line = position.GetContainingLine();
                if (position.Position > line.Start) {
                    return GetItemAtPosition(line, position.Position - 1,
                        (x) => x == RTokenType.Identifier || x == RTokenType.Keyword,
                        out span);
                }
            }
            span = Span.FromBounds(0, 0);
            return string.Empty;
        }

        public static string GetItemAtPosition(ITextSnapshotLine line, int position, Func<RTokenType, bool> tokenTypeCheck, out Span span) {
            string lineText = line.GetText();
            var tokenizer = new RTokenizer();
            var tokens = tokenizer.Tokenize(lineText);
            var tokenIndex = tokens.GetItemContaining(position - line.Start);
            if (tokenIndex >= 0 && tokenTypeCheck(tokens[tokenIndex].TokenType)) {
                span = new Span(tokens[tokenIndex].Start + line.Start, tokens[tokenIndex].Length);
                return lineText.Substring(tokens[tokenIndex].Start, tokens[tokenIndex].Length);
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
                }
            }
            return false;
        }
    }
}
