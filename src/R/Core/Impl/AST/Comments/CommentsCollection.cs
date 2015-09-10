using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Comments
{
    /// <summary>
    /// A collection of R comments. 
    /// </summary>
    [DebuggerDisplay("Count={Count}")]
    public class CommentsCollection: DisjointTextRangeCollection<RToken>
    {

        #region Construction
        public CommentsCollection():
            base()
        {
        }

        public CommentsCollection(IEnumerable<RToken> ranges) :
            base(ranges)
        {
        }
        #endregion

        #region ITextRange
        public override bool Contains(int position)
        {
            if (this.Count == 0 || position == this.Start)
                return false;

            foreach (ITextRange range in this)
            {
                if (range.Contains(position) || range.End == position)
                    return true;
            }

            return false;
        }
        #endregion
    }
}
