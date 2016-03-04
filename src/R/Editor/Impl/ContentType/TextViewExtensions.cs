// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.ContentType {
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

        public static string GetItemUnderCaret(this ITextView textView, out Span span) {
            if (!textView.Caret.InVirtualSpace) {
                SnapshotPoint position = textView.Caret.Position.BufferPosition;
                ITextSnapshotLine line = position.GetContainingLine();
                return GetItem(line, position.Position, out span);
            }
            span = Span.FromBounds(0, 0);
            return string.Empty;
        }

        public static string GetItemBeforeCaret(this ITextView textView, out Span span) {
            if (!textView.Caret.InVirtualSpace) {
                SnapshotPoint position = textView.Caret.Position.BufferPosition;
                ITextSnapshotLine line = position.GetContainingLine();
                if (position.Position > line.Start) {
                    return GetItem(line, position.Position - 1, out span);
                }
            }
            span = Span.FromBounds(0, 0);
            return string.Empty;
        }

        private static string GetItem(ITextSnapshotLine line, int position, out Span span) {
            string lineText = line.GetText();
            int linePosition = position - line.Start;
            int start = 0;
            int end = lineText.Length;
            for (int i = linePosition - 1; i >= 0; i--) {
                char ch = lineText[i];
                if (!RTokenizer.IsIdentifierCharacter(ch)) {
                    start = i + 1;
                    break;
                }
            }
            for (int i = linePosition; i < lineText.Length; i++) {
                char ch = lineText[i];
                if (!RTokenizer.IsIdentifierCharacter(ch)) {
                    end = i;
                    break;
                }
            }

            if (end > start) {
                span = Span.FromBounds(start + line.Start, end + line.Start);
                return lineText.Substring(start, end - start);
            }

            span = Span.FromBounds(0, 0);
            return string.Empty;
        }
    }
}
