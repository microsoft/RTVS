// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Parser.Tokens {
    public class CommentToken : CompositeToken // since afterfacts can appear in comments
    {
        public CommentToken(IHtmlToken[] tokens)
            : base(tokens) {
        }

        /// <summary>
        /// Comment token is well formed if last token is terminating -->
        /// </summary>
        public override bool IsWellFormed {
            get {
                if (this.Count > 0)
                    return this[Count - 1].IsWellFormed;

                return false;
            }
        }
    }
}
