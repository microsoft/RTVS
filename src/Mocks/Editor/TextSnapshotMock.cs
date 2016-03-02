﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Core.Text;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class TextSnapshotMock : ITextSnapshot
    {
        public ITextProvider TextProvider { get; private set; }
        public TextChangeMock Change { get; private set; }
        private TextVersionMock _version;
        private ITextSnapshotLine[] _lines;

        public TextSnapshotMock(string content, ITextBuffer textBuffer, TextVersionMock version)
        {
            TextProvider = new TextStream(content);
            TextBuffer = textBuffer;
            _lines = MakeLines(content);
            _version = version;

            Change = new TextChangeMock();
        }

        public TextSnapshotMock CreateNextSnapshot(string content, TextChangeMock change)
        {
            Change = change;
            TextVersionMock nextVersion = _version.CreateNextVersion(change);
            TextSnapshotMock nextSnapshot = new TextSnapshotMock(content, TextBuffer, nextVersion);

            return nextSnapshot;
        }

        #region ITextSnapshot Members

        public IContentType ContentType
        {
            get { return TextBuffer.ContentType; }
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            var text = TextProvider.GetText(new TextRange(sourceIndex, count));

            for (int i = 0; i < count; i++)
            {
                destination[destinationIndex + i] = text[i];
            }
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            return new TrackingPointMock(TextBuffer, position, trackingMode, trackingFidelity);
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode)
        {
            return CreateTrackingPoint(position, trackingMode, TrackingFidelityMode.Forward);
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            return new TrackingSpanMock(TextBuffer, new Span(start, length), trackingMode, trackingFidelity);
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode)
        {
            return CreateTrackingSpan(start, length, trackingMode, TrackingFidelityMode.Forward);
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            return new TrackingSpanMock(TextBuffer, span, trackingMode, trackingFidelity);
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode)
        {
            return CreateTrackingSpan(span, trackingMode, TrackingFidelityMode.Forward);
        }

        public ITextSnapshotLine GetLineFromLineNumber(int lineNumber)
        {
            return _lines[lineNumber];
        }

        public ITextSnapshotLine GetLineFromPosition(int position)
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                int start = _lines[i].Start.Position;
                int length = _lines[i].Length;

                if (length == _lines[i].Length)
                {
                    if ((start <= position && position <= start + length) || start == position + 1)
                        return _lines[i];
                }
                else
                {
                    if (start <= position && position < start + length)
                        return _lines[i];
                }
            }

            if (position == 0)
                return _lines[0];

            if (position >= _lines[_lines.Length - 1].Length)
                return _lines[_lines.Length - 1];

            return null;
        }

        public int GetLineNumberFromPosition(int position)
        {
            return GetLineFromPosition(position).LineNumber;
        }

        public string GetText()
        {
            return TextProvider.GetText(new TextRange(0, TextProvider.Length));
        }

        public string GetText(int startIndex, int length)
        {
            return TextProvider.GetText(new TextRange(startIndex, length));
        }

        public string GetText(Span span)
        {
            return TextProvider.GetText(new TextRange(span.Start, span.Length));
        }

        public int Length
        {
            get { return TextProvider.Length; }
        }

        public int LineCount
        {
            get { return _lines.Length; }
        }

        public IEnumerable<ITextSnapshotLine> Lines
        {
            get { return _lines; }
        }

        public ITextBuffer TextBuffer { get; private set; }

        public char[] ToCharArray(int startIndex, int length)
        {
            return GetText(startIndex, length).ToArray();
        }

        public ITextVersion Version
        {
            get
            {
                return _version;
            }
        }

        public void Write(TextWriter writer)
        {
            writer.Write(GetText());
        }

        public void Write(TextWriter writer, Span span)
        {
            writer.Write(GetText(span));
        }

        public char this[int position]
        {
            get { return TextProvider[position]; }
        }

        #endregion

        private ITextSnapshotLine[] MakeLines(string text)
        {
            // Only handles \n or \r\n

            var list = new List<ITextSnapshotLine>();
            int start = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch.IsLineBreak())
                {
                    list.Add(new TextLineMock(this, start, i - start, list.Count));

                    if (ch == '\r' && i < text.Length - 1 && text[i + 1] == '\n')
                    {
                        i++;
                    }

                    start = i + 1;
                }
            }

            if (list.Count > 0 && list[list.Count - 1].End < text.Length)
            {
                start = list[list.Count - 1].End;

                if (start < text.Length && text[start].IsLineBreak())
                {
                    start++;

                    if (text[start - 1] == '\r' && start < text.Length && text[start] == '\n')
                        start++;
                }

                list.Add(new TextLineMock(this, start, text.Length - start, list.Count));
            }

            if (list.Count == 0)
            {
                list.Add(new TextLineMock(this, 0, text.Length, 0));
            }

            return list.ToArray();
        }
    }
}
