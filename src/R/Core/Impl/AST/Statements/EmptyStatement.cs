// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Represents statement that consists of a single semicolon.
    /// </summary>
    [DebuggerDisplay("[EmptyStatement]")]
    public class EmptyStatement : AstNode, IStatement {
        public TokenNode Semicolon { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent) {
            this.Semicolon = RParser.ParseToken(context, this);
            return base.Parse(context, parent);
        }
    }
}
