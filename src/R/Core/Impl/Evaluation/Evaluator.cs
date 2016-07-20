// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Evaluation.Definitions;

namespace Microsoft.R.Core.Evaluation {
    public sealed class CodeEvaluator : ICodeEvaluator {
        public RObject Evaluate(IAstNode node) {
            IRValueNode rValue = node as IRValueNode;
            if (rValue == null) {
                return RNull.Null;
            }

            return RNull.Null;
        }
    }
}
