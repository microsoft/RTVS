// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Class represents collection of text ranges that have 'sensitive' separator
    /// sequences at the beginning and at the end. For example, HTML comments &lt;!-- -->
    /// or ASP.NET blocks like &lt;% %> or R markdown code block like ```{r...code...```. 
    /// This collection has additional methods that help to detemine if change to the text
    /// buffer may have created new or invalidated existing comments or external code fragments.
    /// </summary>
    /// <typeparam name="T">
    /// Type that implements ITextRange and supplies separator information via 
    /// ISensitiveFragmentSeparatorsInfo interface
    /// </typeparam>
    public abstract class SensitiveFragmentCollection<T> : 
        TextRangeCollection<T>, 
        ISensitiveFragmentSeparatorsInfo 
        where T : ITextRange {

        public abstract string LeftSeparator { get; }
        public abstract string RightSeparator { get; }

        /// <summary>
        /// Determines if particular change to the document creates new or changes 
        /// boundaries of one of the existing sensitive fragments.
        /// </summary>
        /// <param name="oldText">Document text before the change.</param>
        /// <param name="newText">Document text after the change.</param>
        /// <param name="start">Change start position.</param>
        /// <param name="oldLength">Length of changed area before the change. Zero means new text was inserted.</param>
        /// <param name="newLength">Length of changed area after the change. Zero means text was deleted.</param>
        /// <returns>True of change is destruction and document needs to be reprocessed.</returns>
        public virtual bool IsDestructiveChange(int start, int oldLength, int newLength, ITextProvider oldText, ITextProvider newText) {
            // Get list of items overlapping the change. Note that items haven't been
            // shifted yet and hence their positions match the old text snapshot.
            var itemsInRange = ItemsInRange(new TextRange(start, oldLength));

            // Is crosses item boundaries, it is destructive
            if (itemsInRange.Count > 1 || (itemsInRange.Count == 1 && (!itemsInRange[0].Contains(start) || !itemsInRange[0].Contains(start + oldLength)))) {
                return true;
            }

            foreach (var curSeparatorInfo in SeparatorInfos) {
                if (IsDestructiveChangeForSeparator(curSeparatorInfo, itemsInRange, start, oldLength, newLength, oldText, newText)) {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool IsDestructiveChangeForSeparator(
            ISensitiveFragmentSeparatorsInfo separatorInfo, 
            IReadOnlyList<T> itemsInRange, 
            int start, int oldLength, int newLength, 
            ITextProvider oldText, ITextProvider newText) {
            if (separatorInfo == null) {
                return false;
            }

            if (separatorInfo.LeftSeparator.Length == 0 && separatorInfo.RightSeparator.Length == 0) {
                return false;
            }

            // Find out if one of the existing fragments contains position 
            // and if change damages fragment start or end separators
            var leftSeparator = separatorInfo.LeftSeparator;
            var rightSeparator = separatorInfo.RightSeparator;

            // If no items are affected, change is unsafe only if new region contains separators.
            if (itemsInRange.Count == 0) {
                // Simple optimization for whitespace insertion
                if (oldLength == 0 && string.IsNullOrWhiteSpace(newText.GetText(new TextRange(start, newLength)))) {
                    return false;
                }

                // Take into account that user could have deleted space between existing 
                // <! and -- or added - to the existing <!- so extend search range accordingly.
                var fragmentStart = Math.Max(0, start - leftSeparator.Length + 1);
                var fragmentEnd = Math.Min(newText.Length, start + newLength + leftSeparator.Length - 1);

                var fragmentStartPosition = newText.IndexOf(leftSeparator, TextRange.FromBounds(fragmentStart, fragmentEnd), true);
                if (fragmentStartPosition >= 0) {
                    return true;
                }

                fragmentStart = Math.Max(0, start - rightSeparator.Length + 1);
                fragmentEnd = Math.Min(newText.Length, start + newLength + rightSeparator.Length - 1);

                fragmentStartPosition = newText.IndexOf(rightSeparator, TextRange.FromBounds(fragmentStart, fragmentEnd), true);
                if (fragmentStartPosition >= 0) {
                    return true;
                }
                return false;
            }

            // Is change completely inside an existing item?
            if (itemsInRange.Count == 1 && (itemsInRange[0].Contains(start) && itemsInRange[0].Contains(start + oldLength))) {
                // Check that change does not affect item left separator
                if (TextRange.Contains(itemsInRange[0].Start, leftSeparator.Length, start)) {
                    return true;
                }

                // Check that change does not affect item right separator. Note that we should not be using 
                // TextRange.Intersect since in case oldLength is zero (like when user is typing right before %> or ?>)
                // TextRange.Intersect will determine that zero-length range intersects with the right separator
                // which is incorrect. Typing at position 10 does not change separator at position 10. Similarly,
                // deleting text right before %> or ?> does not make change destructive.
                var rightSeparatorStart = itemsInRange[0].End - rightSeparator.Length;
                if (start + oldLength > rightSeparatorStart) {
                    if (TextRange.Intersect(rightSeparatorStart, rightSeparator.Length, start, oldLength)) {
                        return true;
                    }
                }

                // Touching left separator is destructive too, like when changing <% to <%@
                // Check that change does not affect item left separator (whitespace is fine)
                if (itemsInRange[0].Start + leftSeparator.Length == start) {
                    if (oldLength == 0) {
                        var text = newText.GetText(new TextRange(start, newLength));
                        if (string.IsNullOrWhiteSpace(text)) {
                            return false;
                        }
                    }
                    return true;
                }

                var fragmentStart = itemsInRange[0].Start + separatorInfo.LeftSeparator.Length;
                fragmentStart = Math.Max(fragmentStart, start - separatorInfo.RightSeparator.Length + 1);
                var changeLength = newLength - oldLength;
                var fragmentEnd = itemsInRange[0].End + changeLength;
                fragmentEnd = Math.Min(fragmentEnd, start + newLength + separatorInfo.RightSeparator.Length - 1);

                if (newText.IndexOf(separatorInfo.RightSeparator, TextRange.FromBounds(fragmentStart, fragmentEnd), true) >= 0) {
                    return true;
                }

                return false;
            }

            return true;
        }

        protected abstract IEnumerable<ISensitiveFragmentSeparatorsInfo> SeparatorInfos { get; }
    }
}
