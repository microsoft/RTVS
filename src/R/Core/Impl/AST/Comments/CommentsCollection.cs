// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Comments {
    /// <summary>
    /// A collection of R comments. 
    /// </summary>
    [DebuggerDisplay("Count={Count}")]
    public class CommentsCollection : DisjointTextRangeCollection<RToken> {

        #region Construction
        public CommentsCollection() { }

        public CommentsCollection(IEnumerable<RToken> ranges) :
            base(ranges) {
        }
        #endregion

        #region ITextRange
        public override bool Contains(int position) {
            if (this.Count == 0 || position == this.Start) {
                return false;
            }

            foreach (ITextRange range in this) {
                if (range.Contains(position) || range.End == position) {
                    return true;
                }
            }

            return false;
        }
        #endregion

        public override int GetItemContaining(int position) {
            // Comments contain end position
            IReadOnlyList<int> items = GetItemsContainingInclusiveEnd(position);
            Debug.Assert(items.Count <= 1);

            return items.Count > 0 ? items[0] : -1;
        }
    }
}
