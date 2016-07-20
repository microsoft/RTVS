// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Operators {
    public interface IOperator : IRValueNode {
        OperatorType OperatorType { get; }

        IRValueNode LeftOperand { get; set; }

        IRValueNode RightOperand { get; set; }

        int Precedence { get; }

        bool IsUnary { get; }

        Associativity Associativity { get; }
    }
}
