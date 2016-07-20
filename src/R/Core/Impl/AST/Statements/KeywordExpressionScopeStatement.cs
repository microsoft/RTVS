// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Statement that is based on a keyword and condition 
    /// followed by a scope typically in a form of 
    /// 'keyword ( expression ) { }'.
    /// </summary>
    [DebuggerDisplay("[KeywordExpressionScopeStatement: {Text}]")]
    public class KeywordExpressionScopeStatement : KeywordExpressionStatement, IKeywordExpressionScope {
        private string _terminatingKeyword;

        public IScope Scope { get; private set; }

        public KeywordExpressionScopeStatement() :
            this(null) {
        }

        public KeywordExpressionScopeStatement(string terminatingKeyword) {
            _terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            if (base.Parse(context, parent)) {
                IScope scope = RParser.ParseScope(context, this, allowsSimpleScope: true,
                                                  terminatingKeyword: _terminatingKeyword);
                if (scope != null) {
                    this.Scope = scope;
                    return true;
                }
            }

            return false;
        }
    }
}