// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// A collection of text ranges that are not next to another. 
    /// Ranges must not overlap. Can be sorted by range start positions. 
    /// Can be searched in order to locate range that contains given 
    /// position or range that starts at a given position.
    /// The search is a binary search. Collection derives from 
    /// <seealso cref="TextRangeCollection"/>
    /// </summary>
    /// <typeparam name="T">A class or an interface that derives from <seealso cref="ITextRange"/></typeparam>
    [DebuggerDisplay("Count={Count}")]
    public class DisjointTextRangeCollection<T> : TextRangeCollection<T> where T : ITextRange {

        #region Construction
        public DisjointTextRangeCollection() {
        }

        public DisjointTextRangeCollection(IEnumerable<T> ranges) : base(ranges) {
        }
        #endregion

        #region ITextRange
        public override bool Contains(int position) {
            if (this.Count == 0) {
                return false;
            }

            foreach (ITextRange range in this) {
                if (range.Contains(position)) {
                    return true;
                }
            }

            return false;
        }
        #endregion

        public bool Contains(int position, bool inclusiveEnd) {
            if (this.Count == 0) {
                return false;
            }

            foreach (ITextRange range in this) {
                if (range.Contains(position) || (inclusiveEnd && range.End == position)) {
                    return true;
                }
            }

            return false;
        }
    }
}
