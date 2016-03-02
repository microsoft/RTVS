// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Operators {

    public static class OperatorPrecedence {

        public static int GetPrecedence(OperatorType operatorType) {
            switch (operatorType) {
                case OperatorType.Sentinel:
                    return 0;

                case OperatorType.Equals:
                    return 10;

                case OperatorType.LeftAssign:
                case OperatorType.RightAssign:
                    return 20;

                case OperatorType.DataTableAssign:
                    return 25;

                case OperatorType.Tilde:
                    return 30;

                case OperatorType.Or:
                case OperatorType.CondtitionalOr:
                    return 40;

                case OperatorType.And:
                case OperatorType.ConditionalAnd:
                    return 50;

                case OperatorType.Not:
                    return 60;

                case OperatorType.GreaterThan:
                case OperatorType.GreaterThanOrEquals:
                case OperatorType.LessThan:
                case OperatorType.LessThanOrEquals:
                case OperatorType.ConditionalEquals:
                case OperatorType.ConditionalNotEquals:
                    return 70;

                case OperatorType.Add:
                case OperatorType.Subtract:
                    return 80;

                case OperatorType.Multiply:
                case OperatorType.Divide:
                    return 90;

                case OperatorType.Modulo: // %%
                case OperatorType.IntegerDivide: // %/%
                case OperatorType.MatrixProduct: // %*%
                case OperatorType.OuterProduct: // %o%
                case OperatorType.KroneckerProduct: // %x%
                case OperatorType.MatchingOperator: // %in%
                case OperatorType.Special: // %abc%
                    return 100;

                case OperatorType.Sequence: // :
                    return 110;

                case OperatorType.Exponent: // ^
                    return 120;

                case OperatorType.Unary: // +, =, !
                    return 130;

                case OperatorType.FunctionCall: // (...)
                case OperatorType.Index: // [] [[]]
                    return 140;

                case OperatorType.ListIndex: // $ or @
                    return 150;

                case OperatorType.Namespace: // :: or :::
                    return 160;

                case OperatorType.Group: // ( ) around expression
                    return 200;
            }

            return 1000;
        }
    }
}
