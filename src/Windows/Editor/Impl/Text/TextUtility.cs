// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public static class TextUtility {
        /// <summary>
        /// Combines multiple changes into one larger change.
        /// </summary>
        /// <param name="e">Text buffer change event argument</param>
        /// <param name="start">Combined span start</param>
        /// <param name="oldLength">Length of the change in the original buffer state</param>
        /// <param name="newLength">Length of the change in the new buffer state</param>
        public static void CombineChanges(TextContentChangedEventArgs e, out int start, out int oldLength, out int newLength) {
            start = 0;
            oldLength = 0;
            newLength = 0;

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
        }

        /// <summary>
        /// Converts list of text changes in a text buffer to a collection of 
        /// changes that are relative one to another. 
        /// </summary>
        /// <param name="changeInfo">Sorted collection of changes</param>
        /// <returns>Collection of relative changes</returns>
        public static IReadOnlyList<TextChangeEventArgs> ConvertToRelative(this TextContentChangedEventArgs changeInfo) {
            var list = new List<TextChangeEventArgs>(changeInfo.Changes.Count);
            ITextChange previousChange = null;
            var oldText = new TextProvider(changeInfo.Before, true);
            var newText = new TextProvider(changeInfo.After, true);

            foreach (var change in changeInfo.Changes) {
                if (previousChange != null) {
                    if (previousChange.OldEnd > change.OldPosition || previousChange.NewEnd > change.NewPosition) {
                        throw new ArgumentException(@"List of changes must not overlap", nameof(changeInfo));
                    }
                }

                var textChange = new TextChangeEventArgs(change.NewPosition, change.OldPosition, change.OldLength, change.NewLength, oldText, newText);
                list.Add(textChange);

                previousChange = change;
            }
            return list;
        }
    }
}
