// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Operators {
    public static class OperatorAssociativity {
        public static Associativity GetAssociativity(OperatorType operatorType) {
            return operatorType == OperatorType.Exponent ? Associativity.Right : Associativity.Left;
        }
    }
}
