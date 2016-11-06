// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting {
    /// <summary>
    /// Stack of formatting scopes. Scope defined indentation level and 
    /// may be opened by curly braces (<see cref="BlockFormattingScope"/>) 
    /// or by an incomplete multiline expression (<see cref="ExpressionFormattingScope"/>).
    /// </summary>
    internal sealed class FormattingScopeStack {
        private readonly List<FormattingScope> _scopes = new List<FormattingScope>();
        private readonly ITextProvider _textProvider;
        private readonly TextBuilder _tb;
        private readonly TokenStream<RToken> _tokens;
        private readonly RFormatOptions _options;

        private FormattingScope Top => _scopes[_scopes.Count - 1];

        public FormattingScopeStack(ITextProvider textProvider, TextBuilder tb, TokenStream<RToken> tokens, RFormatOptions options) {
            // Global scope is a block scope. There is always block scope
            // and an expression scope on top of it.
            _textProvider = textProvider;
            _tb = tb;
            _tokens = tokens;
            _options = options;

            OpenScope(new BlockFormattingScope());
            OpenScope(new ExpressionFormattingScope(textProvider, tb, tokens, options));
        }
        public void OpenScope(FormattingScope scope) {
            // Expression scope can only be pushed on top of the block scope.
            if (scope is ExpressionFormattingScope && Top is ExpressionFormattingScope) {
                Debug.Fail("Expression formatting scope is already opened");
                return;
            }
            _scopes.Add(scope);
        }

        public void CloseScope() {
            // If expression is complete, expression scope is removed and block scope
            // is placed on the stack followed by a new expression scope. If expression
            // is incomplete, its scope remains on the stack and block scope is placed
            // on top of the incomplete expression scope.
            Debug.Assert(_scopes.Count > 1);
            if (ExpressionScope.IsComplete) {
                PopScope();
            }

            PopScope();

            var efs = Top as ExpressionFormattingScope;
            if (efs == null) {
                OpenScope(new ExpressionFormattingScope(_textProvider, _tb, _tokens, _options));
            }
        }

        public BlockFormattingScope BlockScope {
            get {
                var bfs = Top as BlockFormattingScope;
                return bfs ?? _scopes[_scopes.Count - 2] as BlockFormattingScope;
            }
        }

        public ExpressionFormattingScope ExpressionScope {
            get {
                var scope = Top as ExpressionFormattingScope;
                Debug.Assert(scope != null);
                return scope;
            }
        }

        public void CloseExpression() {
            if (Top is ExpressionFormattingScope) {
                CloseScope();
            }
        }

        private void PopScope() {
            var scope = Top;
            _scopes.RemoveAt(_scopes.Count - 1);
            scope.Dispose();
        }
    }
}
