// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Parser {
    internal class CommentSeparatorInfo : ISensitiveFragmentSeparatorsInfo {
        #region ISensitiveFragmentSeparatorsInfo Members
        public string LeftSeparator {
            get { return "<!--"; }
        }

        public string RightSeparator {
            get { return "-->"; }
        }
        #endregion
    }
}
