// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.DataTypes;

namespace Microsoft.R.Core.AST.Evaluation.Definitions {
    /// <summary>
    /// Represents object that can evaluate R statements and expressions.
    /// </summary>
    public interface ICodeEvaluator {
        RObject Evaluate(IAstNode node);
    }
}
