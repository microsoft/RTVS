// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Operators {

    public static class OperatorPrecedence {
        // https://stat.ethz.ch/R-manual/R-devel/library/base/html/Syntax.html
        public static int GetPrecedence(OperatorType operatorType) {
            // Lower number means lower priority. Lowest priority operators 
            // appear higher in the tree so they are evaluated last.
            switch (operatorType) {
                case OperatorType.Sentinel:
                    return 0;

                case OperatorType.Help:
                    return 10;

                case OperatorType.Equals:
                    return 20;

                case OperatorType.LeftAssign:
                    return 30;

                case OperatorType.RightAssign:
                    return 40;

                case OperatorType.DataTableAssign:
                    return 50;

                case OperatorType.Tilde:
                    return 60;

                case OperatorType.Or:
                case OperatorType.CondtitionalOr:
                    return 70;

                case OperatorType.And:
                case OperatorType.ConditionalAnd:
                    return 80;

                case OperatorType.Not:
                    return 90;

                case OperatorType.GreaterThan:
                case OperatorType.GreaterThanOrEquals:
                case OperatorType.LessThan:
                case OperatorType.LessThanOrEquals:
                case OperatorType.ConditionalEquals:
                case OperatorType.ConditionalNotEquals:
                    return 100;

                case OperatorType.Add:
                case OperatorType.Subtract:
                    return 110;

                case OperatorType.Multiply:
                case OperatorType.Divide:
                    return 120;

                case OperatorType.Modulo: // %%
                case OperatorType.IntegerDivide: // %/%
                case OperatorType.MatrixProduct: // %*%
                case OperatorType.OuterProduct: // %o%
                case OperatorType.KroneckerProduct: // %x%
                case OperatorType.MatchingOperator: // %in%
                case OperatorType.Special: // %abc%
                    return 130;

                case OperatorType.Sequence: // :
                    return 140;

                case OperatorType.UnaryMinus: // -
                case OperatorType.UnaryPlus: // +
                    return 150;

                case OperatorType.Exponent: // ^
                    return 160;

                case OperatorType.FunctionCall: // (...)
                case OperatorType.Index: // [] [[]]
                    return 170;

                case OperatorType.ListIndex: // $ or @
                    return 180;

                case OperatorType.Namespace: // :: or :::
                    return 190;

                case OperatorType.Group: // ( ) around expression
                    return 200;
            }
            return 1000;
        }
    }
}
