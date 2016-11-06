// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting {
    /// <summary>
    /// Settings for formatting of { } scope.
    /// </summary>
    internal sealed class ExpressionFormattingScope : FormattingScope {
        private readonly ITextProvider _textProvider;

        /// <summary>
        /// Tracks position of expressions in the scope. Used to determine if currently
        /// formatted expression is complete at the line break. Completeness defines
        /// indentation in multiline expressions.
        /// </summary>
        public int StartExpressionTokenIndex { get; private set; }

        public bool IsComplete { get; private set; } = true;

        public ExpressionFormattingScope(ITextProvider textProvider, TextBuilder tb, TokenStream<RToken> tokens, RFormatOptions options): 
            base(tb, tokens, options) {
            _textProvider = textProvider;
            StartExpressionTokenIndex = tokens.CurrentToken.End;
        }

        /// <summary>
        /// Given starting token index parses the expression in order
        /// to determine if expression is complete and well formed.
        /// Used to determine indentation level in multiline expressions.
        /// </summary>
        public void CheckComplete() {
            IsComplete = true;

            var ast = RParser.Parse(_textProvider, 
                                    TextRange.FromBounds(Tokens[StartExpressionTokenIndex].Start, Tokens.PreviousToken.End), 
                                    Tokens, new List<RToken>(), null);
            foreach (var error in ast.Errors) {
                if (error.ErrorType == ParseErrorType.CloseCurlyBraceExpected ||
                    error.ErrorType == ParseErrorType.CloseBraceExpected ||
                    error.ErrorType == ParseErrorType.CloseSquareBracketExpected ||
                    error.ErrorType == ParseErrorType.FunctionBodyExpected ||
                    error.ErrorType == ParseErrorType.RightOperandExpected) {
                    IsComplete = false;
                    break;
                }
            }

            if(IsComplete) {
                StartExpressionTokenIndex = Tokens.PreviousToken.End;
            }
         }

        protected override void Dispose(bool disposing) { }
    }
}
