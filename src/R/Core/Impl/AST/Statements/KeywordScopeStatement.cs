// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Statement with keyword and scope { } such as repeat { } and else { }
    /// </summary>
    [DebuggerDisplay("[KeywordScopeStatement: {Text}]")]
    public sealed class KeywordScopeStatement : KeywordStatement, IKeywordScopeStatement {
        public IScope Scope { get; private set; }

        private bool _allowsSimpleScope;

        public KeywordScopeStatement(bool allowsSimpleScope) {
            _allowsSimpleScope = allowsSimpleScope;
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            if (ParseKeyword(context, parent)) {
                IScope scope = RParser.ParseScope(context, this, _allowsSimpleScope, terminatingKeyword: null);
                if (scope != null) {
                    this.Scope = scope;
                }

                this.Parent = parent;
                return true;
            }

            return false;
        }
    }
}