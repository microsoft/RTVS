// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.Extensions {
    public static class TextBufferExtensions {
        public static bool IsContentEqualsOrdinal(this ITextBuffer textBuffer, ITrackingSpan span1, ITrackingSpan span2) {
            var snapshot = textBuffer.CurrentSnapshot;
            var snapshotSpan1 = span1.GetSpan(snapshot);
            var snapshotSpan2 = span2.GetSpan(snapshot);

            if (snapshotSpan1 == snapshotSpan2) {
                return true;
            }

            if (snapshotSpan1.Length != snapshotSpan2.Length) {
                return false;
            }

            var start1 = snapshotSpan1.Start;
            var start2 = snapshotSpan2.Start;
            for (int i = 0; i < snapshotSpan1.Length; i++) {
                if (snapshot[start1 + i] != snapshot[start2 + i]) {
                    return false;
                }
            }

            return true;
        }

    }
}
