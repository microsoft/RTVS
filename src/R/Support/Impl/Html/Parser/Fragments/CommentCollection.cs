// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser {
    /// <summary>
    /// Collection of HTML comments in the document.
    /// </summary>
    public class CommentCollection : HtmlSensitiveFragmentCollection<CommentToken>, ISensitiveFragmentSeparatorsInfo {
        protected override IEnumerable<ISensitiveFragmentSeparatorsInfo> SeparatorInfos {
            get { return new ISensitiveFragmentSeparatorsInfo[] { this }; }
        }

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
