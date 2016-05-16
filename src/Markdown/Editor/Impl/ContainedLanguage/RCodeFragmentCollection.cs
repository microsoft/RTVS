// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Markdown.Editor.Tokens;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    public sealed class RCodeFragmentCollection: SensitiveFragmentCollection<MarkdownToken>, ISensitiveFragmentSeparatorsInfo {
        protected override IEnumerable<ISensitiveFragmentSeparatorsInfo> SeparatorInfos => new ISensitiveFragmentSeparatorsInfo[] { this };

        #region ISensitiveFragmentSeparatorsInfo
        public string LeftSeparator => "```";
        public string RightSeparator => "```";
        #endregion
    }
}
