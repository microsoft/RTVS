// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting {
    /// <summary>
    /// Settings for formatting of { } scope.
    /// </summary>
    internal sealed class BlockFormattingScope : FormattingScope {
        public int CloseBracePosition { get; private set; } = -1;

        /// <summary>
        /// Counts line break suppression requests. Line breaks are suppressed
        /// when formatting nested conditional expression with simple scopes
        /// (i.e. scopes without { } blocks).
        /// </summary>
        public int SuppressLineBreakCounter { get; set; }

        public BlockFormattingScope() { }

        public BlockFormattingScope(TextBuilder tb, TokenStream<RToken> tokens, RFormatOptions options): 
            base(tb, tokens, options) {
            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace);

            CloseBracePosition = TokenBraceCounter<RToken>.GetMatchingBrace(tokens,
                new RToken(RTokenType.OpenCurlyBrace), new RToken(RTokenType.CloseCurlyBrace),
                new RTokenTypeComparer());
        }
    }
}
