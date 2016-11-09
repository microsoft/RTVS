// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Core.Formatting {
    internal sealed class FormattingScopeStack {
        private readonly Stack<FormattingScope> _formattingScopes = new Stack<FormattingScope>();

        public int SuppressLineBreakCount {
            get { return _formattingScopes.Peek().SuppressLineBreakCount; }
            set { _formattingScopes.Peek().SuppressLineBreakCount = value; }
        }

        public FormattingScopeStack() {
            _formattingScopes.Push(new FormattingScope());
        }

        public void Push(FormattingScope scope) => _formattingScopes.Push(scope);

        public void Close(int tokenIndex) {
            if (_formattingScopes.Count > 1) {
                if (_formattingScopes.Peek().CloseBraceTokenIndex == tokenIndex) {
                    FormattingScope scope = _formattingScopes.Pop();
                    scope.Dispose();
                }
            }
        }
    }
}
