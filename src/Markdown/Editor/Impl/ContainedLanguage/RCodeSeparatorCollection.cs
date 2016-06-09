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

        public override bool IsDestructiveChange(int start, int oldLength, int newLength, ITextProvider oldText, ITextProvider newText) {
            if (oldText.Length > 0) {
                if (start < oldText.Length && oldText[start] == '`') {
                    // Changing anything right before the ` is destructive
                    return true;
                }

                if (start > 0 && oldText[start - 1] == '`') {
                    // Changing anything right after the ` is destructive
                    return true;
                }

                int end = start + oldLength;
                if (end < oldText.Length && oldText[end] == '`') {
                    // Changing anything right before the ` is destructive
                    return true;
                }

                if (end >= 0 && oldText[end] == '`') {
                    // Changing anything right after the ` is destructive
                    return true;
                }
            }

            // Deleting or adding backticks is destructive
            if (oldText.IndexOf('`', new TextRange(start, oldLength)) >= 0 || newText.IndexOf('`', new TextRange(start, newLength)) >= 0) {
                return true;
            }

            return base.IsDestructiveChange(start, oldLength, newLength, oldText, newText);
        }
    }
}
