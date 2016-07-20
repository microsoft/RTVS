// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.DataTypes;

namespace Microsoft.R.Core.AST {
    /// <summary>
    /// Represents L-value: an entity that can be assigned to.
    /// This may be an identifier (variable), indexed variable such as 
    /// x[expression]. In other words an entity that can appear at the 
    /// left side of &lt;- operator or at the right side of the -&gt;
    /// operator.
    /// </summary>
    public interface ILValueNode : IRValueNode {
        void SetValue(RObject value);
    }
}
