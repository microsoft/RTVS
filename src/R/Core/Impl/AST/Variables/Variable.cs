// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Variables {
    /// <summary>
    /// Represents R variable.
    /// </summary>
    [DebuggerDisplay("[{Name} : {Start}...{End}), Length = {Length}")]
    public sealed class Variable : TokenNode, IVariable {
        #region IVariable
        public string Name => Root != null ? Root.TextProvider.GetText(NameRange) : "<not_ready>";
        public ITextRange NameRange => this;
        public TokenNode Identifier => this;
        public RObject Value { get; set; }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent = null) {
            RToken currentToken = context.Tokens.CurrentToken;
            Debug.Assert(currentToken.IsVariableKind());

            // Not calling base since expression parser will decide 
            // what parent node the variable belongs to.
            Token = currentToken;
            context.Tokens.MoveToNextToken();

            return true;
        }

        public override string ToString() => Name;
    }
}
