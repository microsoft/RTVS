// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class PersistentSpanMock : IPersistentSpan {
        private ITextBuffer _textBuffer;
        private Span _span;

        public PersistentSpanMock(ITextBuffer textBuffer, Span span, string filePath) {
            _textBuffer = textBuffer;
            _span = span;
            FilePath = filePath;
        }

        public ITextDocument Document  => new TextDocumentMock(_textBuffer, FilePath);
        public string FilePath { get; }
        public bool IsDocumentOpen => true;
        public ITrackingSpan Span => new TrackingSpanMock(_textBuffer, _span, SpanTrackingMode.EdgePositive, TrackingFidelityMode.Forward);
        public void Dispose() { }

        public bool TryGetEndLineIndex(out int endLine, out int endIndex) {
            throw new NotImplementedException();
        }

        public bool TryGetSpan(out Span span) {
            span = _span;
            return true;
        }

        public bool TryGetStartLineIndex(out int startLine, out int startIndex) {
            throw new NotImplementedException();
        }
    }
}
