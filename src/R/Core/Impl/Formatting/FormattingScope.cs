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
    /// Settings for formatting of { } scope.
    /// </summary>
    internal abstract class FormattingScope : IDisposable {
        protected RFormatOptions Options { get; }
        protected TextBuilder TextBuilder { get; }
        protected TokenStream<RToken> Tokens { get; }
        protected int PreviousIndentLevel { get; }

        protected FormattingScope() { }

        protected FormattingScope(TextBuilder tb, TokenStream<RToken> tokens, RFormatOptions options) {
            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace);

            Options = options;
            Tokens = tokens;
            TextBuilder = tb;

            PreviousIndentLevel = tb.IndentBuilder.IndentLevel;
            tb.IndentBuilder.SetIndentLevelForSize(CurrentLineIndent(tb));
        }

        protected int CurrentLineIndent(TextBuilder tb) {
            for (int i = tb.Length - 1; i >= 0; i--) {
                if (CharExtensions.IsLineBreak(tb.Text[i])) {
                    return IndentBuilder.TextIndentInSpaces(tb.Text.Substring(i + 1), Options.TabSize);
                }
            }
            return 0;
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            TextBuilder.IndentBuilder.SetIndentLevel(PreviousIndentLevel);
        }
    }
}
