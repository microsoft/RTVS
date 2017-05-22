// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public static class TextChangeExtensions {
        /// <summary>
        /// Combines multiple changes into one larger change.
        /// </summary>
        /// <param name="e">Text buffer change event argument</param>
        public static TextChange ToTextChange(this TextContentChangedEventArgs e) {
            int start = 0, oldLength = 0, newLength = 0;

            if (e.Changes.Count > 0) {
                // Combine multiple changes into one larger change. The problem is that
                // multiple changes map to one current snapshot and there are no
                // separate snapshots for each change which causes problems
                // in incremental parse analysis code.

                Debug.Assert(e.Changes[0].OldPosition == e.Changes[0].NewPosition);

                start = e.Changes[0].OldPosition;
                var oldEnd = e.Changes[0].OldEnd;
                var newEnd = e.Changes[0].NewEnd;

                for (var i = 1; i < e.Changes.Count; i++) {
                    start = Math.Min(start, e.Changes[i].OldPosition);
                    oldEnd = Math.Max(oldEnd, e.Changes[i].OldEnd);
                    newEnd = Math.Max(newEnd, e.Changes[i].NewEnd);
                }

                oldLength = oldEnd - start;
                newLength = newEnd - start;
            }

            return new TextChange(start, oldLength, newLength, new TextProvider(e.Before), new TextProvider(e.After));
        }
    }
}
