// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Operators {

    public static class OperatorAssociation {

        public static Association GetAssociation(OperatorType operatorType) {
            switch (operatorType) {
                case OperatorType.Exponent:     // ^
                case OperatorType.Equals:       // =
                case OperatorType.LeftAssign:   // <- or <<-
                case OperatorType.FunctionCall: // ()
                case OperatorType.Index:        // [] [[]]
                    return Association.Right;
            }

            return Association.Left;
        }
    }
}
