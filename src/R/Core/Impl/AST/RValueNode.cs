// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST {
    /// <summary>
    /// Base class for complex nodes representing R-values such as
    /// function calls, expressions and similar constructs.
    /// </summary>
    public abstract class RValueNode<T> : AstNode, IRValueNode where T : RObject {
        protected T NodeValue { get; set; }

        #region IRValueNode
        public virtual RObject GetValue() {
            if (NodeValue == null) {
                NodeValue = (T)Root.CodeEvaluator.Evaluate(this);
            }

            return NodeValue;
        }
        #endregion
    }
}
