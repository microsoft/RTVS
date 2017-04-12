// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public static class TrackingSpanExtensions {
        public static int GetCurrentPosition(this ITrackingPoint trackingPoint) {
            ITextSnapshot snapshot = trackingPoint.TextBuffer.CurrentSnapshot;
            return trackingPoint.GetPosition(snapshot);
        }

        public static int GetCurrentStart(this ITrackingSpan trackingSpan) {
            ITextSnapshot snapshot = trackingSpan.TextBuffer.CurrentSnapshot;
            return trackingSpan.GetStartPoint(snapshot).Position;
        }

        public static int GetCurrentEnd(this ITrackingSpan trackingSpan) {
            ITextSnapshot snapshot = trackingSpan.TextBuffer.CurrentSnapshot;
            return trackingSpan.GetEndPoint(snapshot).Position;
        }

        public static Span GetCurrentSpan(this ITrackingSpan trackingSpan) {
            ITextSnapshot snapshot = trackingSpan.TextBuffer.CurrentSnapshot;
            return trackingSpan.GetSpan(snapshot).Span;
        }

        public static string GetCurrentText(this ITrackingSpan trackingSpan) {
            ITextSnapshot snapshot = trackingSpan.TextBuffer.CurrentSnapshot;
            return trackingSpan.GetText(snapshot);
        }
    }
}

