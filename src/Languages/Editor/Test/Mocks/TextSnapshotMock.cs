using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    public class TextSnapshotMock : ITextSnapshot
    {
        public ITextProvider TextProvider { get; private set; }
        public TextChangeMock Change { get; private set; }
        private TextVersionMock _version;

        public TextSnapshotMock CreateNextSnapshot(string content, TextChangeMock change)
        {
            Change = change;
            TextVersionMock nextVersion = _version.CreateNextVersion(change);
            TextSnapshotMock nextSnapshot = new TextSnapshotMock(content, TextBuffer, ContentType, nextVersion);

            return nextSnapshot;
        }

        private ITextBuffer _textBuffer;
        private ITextSnapshotLine[] _lines;

        public TextSnapshotMock(string content, ITextBuffer textBuffer, IContentType contentType, TextVersionMock version)
        {
            ContentType = contentType;
            TextProvider = new TextStream(content);
            _textBuffer = textBuffer;
            _lines = MakeLines(content);
            _version = version;

            Change = new TextChangeMock();
        }

        #region ITextSnapshot Members

        public IContentType ContentType { get; private set; }

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
            return new TrackingPointMock(_textBuffer, position, trackingMode, trackingFidelity);
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode)
        {
            return CreateTrackingPoint(position, trackingMode, TrackingFidelityMode.Forward);
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            return new TrackingSpanMock(_textBuffer, new Span(start, length), trackingMode, trackingFidelity);
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode)
        {
            return CreateTrackingSpan(start, length, trackingMode, TrackingFidelityMode.Forward);
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            return new TrackingSpanMock(_textBuffer, span, trackingMode, trackingFidelity);
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
                int length = _lines[i].LengthIncludingLineBreak;

                if (length == _lines[i].Length)
                {
                    if (start <= position && position <= start + length)
                        return _lines[i];
                }
                else
                {
                    if (start <= position && position < start + length)
                        return _lines[i];
                }
            }

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

        public ITextBuffer TextBuffer
        {
            get { return _textBuffer; }
        }

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
            var list = new List<ITextSnapshotLine>();
            int start = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch == '\r' || ch == '\n')
                {
                    list.Add(new TextLineMock(this, start, i - start, list.Count));

                    if (i < text.Length - 1 && (text[i + 1] == '\r' || text[i + 1] == '\n'))
                    {
                        i++;
                        start = i + 1;
                    }
                }
            }

            if (start < text.Length)
                list.Add(new TextLineMock(this, start, text.Length - start, list.Count));

            return list.ToArray();
        }
    }
}
