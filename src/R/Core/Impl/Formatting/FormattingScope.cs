// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting {
    /// <summary>
    /// Settings for formatting of { } scope. Scope is created when formatter
    /// encounters one of the scope-defining statements such as if() or function()
    /// with or without explicit scope or { }.
    /// </summary>
    internal sealed class FormattingScope : IDisposable {
        private readonly RFormatOptions _options;
        private readonly TextBuilder _tb;
        private readonly int _previousIndentLevel;

        public int CloseBracePosition { get; private set; } = -1;

        /// <summary>
        /// Controls suppression of line break insertion. Used in formatting
        /// constructs like single-line 'if(...) { } else { }`
        /// <seealso cref="RFormatter.CloseFormattingScope"/>
        /// </summary>
        public int SuppressLineBreakCount { get; set; }

        /// <summary>
        /// Defines if indentation should be based on user supplied indent
        /// or if it should be calculated automatically by scope nesting level.
        /// Set by formatter when it encounters construct that should be indented
        /// as user specified such as scope-less functions: 
        ///     x &lt;- function()
        ///         return 1;
        /// or
        ///     x &lt;- 
        ///         if(...)
        ///             return 1;
        /// </summary>
        public int UserIndent { get; set; }

        public FormattingScope() { }

        public FormattingScope(TextBuilder tb, TokenStream<RToken> tokens, RFormatOptions options) {
            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace);

            _options = options;
            _tb = tb;

            CloseBracePosition = TokenBraceCounter<RToken>.GetMatchingBrace(tokens,
                new RToken(RTokenType.OpenCurlyBrace), new RToken(RTokenType.CloseCurlyBrace),
                new RTokenTypeComparer());

            _previousIndentLevel = tb.IndentBuilder.IndentLevel;
            tb.IndentBuilder.SetIndentLevelForSize(CurrentLineIndent(tb));
        }

        private int CurrentLineIndent(TextBuilder tb) {
            for (int i = tb.Length - 1; i >= 0; i--) {
                if (CharExtensions.IsLineBreak(tb.Text[i])) {
                    return IndentBuilder.TextIndentInSpaces(tb.Text.Substring(i + 1), _options.TabSize);
                }
            }
            return 0;
        }

        public void Dispose() {
            _tb.IndentBuilder.SetIndentLevel(_previousIndentLevel);
        }
    }
}
