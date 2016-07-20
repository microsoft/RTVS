// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Scopes;

namespace Microsoft.R.Core.AST.Functions {
    /// <summary>
    /// Represents anonymous function call as in
    /// tryCatch({ code }, ...)
    /// </summary>
    public sealed class Lambda : Scope, IRValueNode {
        #region IRValueNode
        public RObject Value { get; set; }
        #endregion
    }
}
