// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Core.Formatting {
    /// <summary>
    /// Represents stack of formatting scopes. Formatting scope defines
    /// indentation level which is typically based upon { } blocks.
    /// </summary>
    internal sealed class FormattingScopeStack {
        private readonly Stack<FormattingScope> _formattingScopes = new Stack<FormattingScope>();

        public int SuppressLineBreakCount {
            get => _formattingScopes.Peek().SuppressLineBreakCount;
            set => _formattingScopes.Peek().SuppressLineBreakCount = value;
        }

        public FormattingScopeStack() {
            // Push global scope
            _formattingScopes.Push(new FormattingScope());
        }

        public void OpenScope(FormattingScope scope) => _formattingScopes.Push(scope);

        public void TryCloseScope(int tokenIndex) {
            if (_formattingScopes.Count > 1) {
                if (_formattingScopes.Peek().CloseCurlyBraceTokenIndex == tokenIndex) {
                    var scope = _formattingScopes.Pop();
                    scope.Dispose();
                }
            }
        }
    }
}
