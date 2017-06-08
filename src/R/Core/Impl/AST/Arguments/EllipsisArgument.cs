// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Arguments {
    /// <summary>
    /// Represents '...' argument. Normally it is the last 
    /// one in the function definition.
    /// </summary>
    [DebuggerDisplay("Ellipsis [{Start}...{End})")]
    public sealed class EllipsisArgument : CommaSeparatedItem {
        public TokenNode EllipsisToken { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent) {
            EllipsisToken = RParser.ParseToken(context, this);
            return base.Parse(context, parent);
        }

        public override string ToString() => "...";
    }
}
