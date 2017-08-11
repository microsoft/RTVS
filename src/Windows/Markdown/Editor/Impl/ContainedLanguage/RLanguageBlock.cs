// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Core.Text;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    internal sealed class RLanguageBlock : TextRange {
        /// <summary>
        /// Tells that block is `inline` block (as opposed to ```fenced``` blocks)
        /// </summary>
        public bool Inline { get; }

        public RLanguageBlock(int start, int length, bool inline) : base(start, length) {
            Inline = inline;
        }

        public static RLanguageBlock FromBounds(int start, int end, bool inline)
            => new RLanguageBlock(start, end - start, inline);
    }
}
