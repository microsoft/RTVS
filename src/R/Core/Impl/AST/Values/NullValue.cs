// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Values {
    /// <summary>
    /// Represents NULL value
    /// </summary>
    public sealed class NullValue : RValueTokenNode<RNull>, ILiteralNode {
        public override bool Parse(ParseContext context, IAstNode parent) {
            Value = new RNull();
            return base.Parse(context, parent);
        }
    }
}
