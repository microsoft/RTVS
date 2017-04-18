// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Markdown.Editor.Tokens {
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public sealed class MarkdownCodeToken : MarkdownToken {

        public MarkdownCodeToken(): base(MarkdownTokenType.Code) { }

        public MarkdownCodeToken(ITextRange range, int leadingSeparatorLength, int trailingSeparatorLength)
            : base(MarkdownTokenType.Code, range) {
            LeadingSeparatorLength = leadingSeparatorLength;
            TrailingSeparatorLength = trailingSeparatorLength;
        }

        /// <summary>
        /// Code fragment is well formed: it begins and ends
        /// with the proper backtick sequence.
        /// </summary>
        public bool IsWellFormed => TrailingSeparatorLength > 0;

        /// <summary>
        /// Length of the leading block separator. Typically two 
        /// two in '`r' or five in '```{r.
        /// </summary>
        public int LeadingSeparatorLength { get; }

        /// <summary>
        /// Length of the trailing block separator. Typically  
        /// one in '`' or three in '```'. Can be zero
        /// if block is not closed and ends ad the end of the file.
        /// </summary>
        public int TrailingSeparatorLength { get; }
    }
}
