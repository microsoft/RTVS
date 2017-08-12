// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    internal sealed class RLanguageBlock : TextRange, IRLanguageBlock {
        /// <summary>
        /// Tells that block is `inline` block (as opposed to ```fenced``` blocks)
        /// </summary>
        public bool Inline { get; }

        public RLanguageBlock(ITextRange textRange, bool inline) : base(textRange) {
            Inline = inline;
        }
    }
}
