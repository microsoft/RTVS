// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public static class TrackingSpanExtensions {
        public static int GetCurrentPosition(this ITrackingPoint trackingPoint) {
            var snapshot = trackingPoint.TextBuffer.CurrentSnapshot;
            return trackingPoint.GetPosition(snapshot);
        }
    }
}

