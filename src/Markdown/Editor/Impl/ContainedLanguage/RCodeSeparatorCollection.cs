// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    public sealed class RCodeSeparatorCollection: SensitiveFragmentCollection<ITextRange>, ISensitiveFragmentSeparatorsInfo {
        protected override IEnumerable<ISensitiveFragmentSeparatorsInfo> SeparatorInfos => new ISensitiveFragmentSeparatorsInfo[] { this };

        #region ISensitiveFragmentSeparatorsInfo
        public string LeftSeparator => "```{r}";
        public string RightSeparator => "```";
        #endregion
    }
}
