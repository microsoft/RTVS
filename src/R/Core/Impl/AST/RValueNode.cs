// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.DataTypes;

namespace Microsoft.R.Core.AST {
    /// <summary>
    /// Base class for complex nodes representing R-values such as
    /// function calls, expressions and similar constructs.
    /// </summary>
    public abstract class RValueNode : AstNode, IRValueNode {
        #region IRValueNode
        public RObject Value { get; set; }
        #endregion
    }
}
