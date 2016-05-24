// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Core.Text {
    public interface ISensitiveFragmentSeparatorsInfo {
        /// <summary>
        /// Sequence that begins the fragment. For example, 
        /// &lt;!-- in HTML comment or ```{r in R markdown.
        /// </summary>
        string LeftSeparator { get; }

        /// <summary>
        /// Sequence that terminates the fragment. For example, 
        /// --&gt; in HTML comment or ```{r in R markdown.
        /// </summary>
        string RightSeparator { get; }
    }
}
