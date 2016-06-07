// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    public sealed class RCodeSeparatorCollection : SensitiveFragmentCollection<ITextRange> {
        protected override IEnumerable<ISensitiveFragmentSeparatorsInfo> SeparatorInfos => new ISensitiveFragmentSeparatorsInfo[] { this };

        #region ISensitiveFragmentSeparatorsInfo
        public override string LeftSeparator => "```{r";
        public override string RightSeparator => "```";
        #endregion

        protected override bool IsDestructiveChangeForSeparator(
            ISensitiveFragmentSeparatorsInfo separatorInfo,
            IReadOnlyList<ITextRange> itemsInRange,
            int start, int oldLength, int newLength,
            ITextProvider oldText, ITextProvider newText) {

            var index = GetItemAtPosition(start);
            if (index >= 0) {
                // Changing anything right before the ``` is destructive
                return true;
            }

            index = GetFirstItemBeforePosition(start);
            if (index >= 0 && Items[index].End == start && newLength > 0 && newText[start] == '`') {
                // Typing ` right after the ``` is destructive
                return true;
            }

            return base.IsDestructiveChangeForSeparator(separatorInfo, itemsInRange, start, oldLength, newLength, oldText, newText);
        }
    }
}
