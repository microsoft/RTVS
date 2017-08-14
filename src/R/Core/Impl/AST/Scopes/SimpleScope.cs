// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Scopes {
    /// <summary>
    /// Represents scope that only holds a single statement. The node may
    /// actually have multiple children since single line statement
    /// can be followed by a comment as in 'for(...) statement # comment'
    /// </summary>
    [DebuggerDisplay("Simple Scope, Children: {Children.Count} [{Start}...{End})")]
    public sealed class SimpleScope : AstNode, IScope {
        private IStatement _statement;
        private readonly string _terminatingKeyword;

        #region IScope
        public string Name => string.Empty;
        public TokenNode OpenCurlyBrace => null;
        public TokenNode CloseCurlyBrace => null;
        public bool KnitrOptions => false;

        public IReadOnlyDictionary<string, int> Functions => StaticDictionary<string, int>.Empty;
        public IReadOnlyDictionary<string, int> Variables => StaticDictionary<string, int>.Empty;
        public IReadOnlyTextRangeCollection<IStatement> Statements => new TextRangeCollection<IStatement> { _statement };
        #endregion

        public SimpleScope(string terminatingKeyword) {
            _terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent = null) {
            _statement = Statement.Create(context, this, _terminatingKeyword);
            if (_statement != null) {
                if (_statement.Parse(context, this)) {
                    return base.Parse(context, parent);
                }
            }

            return false;
        }
    }
}
