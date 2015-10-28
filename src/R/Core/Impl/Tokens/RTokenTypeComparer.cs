using System.Collections.Generic;

namespace Microsoft.R.Core.Tokens {
    public class RTokenTypeComparer : IComparer<RToken> {
        public int Compare(RToken one, RToken another) {
            if (one == null && another == null)
                return 0;

            if (one == null && another != null)
                return -1;

            if (one != null && another == null)
                return 1;

            if (one.TokenType == another.TokenType)
                return 0;

            if ((int)one.TokenType < (int)another.TokenType)
                return -1;

            return 1;
        }
    }
}
