// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public sealed class TrackingTextRange : ITrackingTextRange {
        private readonly ITrackingSpan _span;

        public TrackingTextRange(ITrackingSpan span) {
            _span = span;
        }

        public T As<T>() where T : class => _span as T;

        public int GetEndPoint(IEditorBufferSnapshot snapshot) => _span.GetEndPoint(snapshot.As<ITextSnapshot>());
        public int GetStartPoint(IEditorBufferSnapshot snapshot) => _span.GetStartPoint(snapshot.As<ITextSnapshot>());
    }
}
