namespace Microsoft.R.Core.AST.Operators
{
    public static class OperatorPrecedence
    {
        public static int GetPrecedence(OperatorType operatorType)
        {
            switch (operatorType)
            {
                case OperatorType.Equals:
                    return 1;

                case OperatorType.LeftAssign:
                case OperatorType.RightAssign:
                    return 2;

                case OperatorType.Tilde:
                    return 3;

                case OperatorType.Or:
                case OperatorType.CondtitionalOr:
                    return 4;

                case OperatorType.And:
                case OperatorType.ConditionalAnd:
                    return 5;

                case OperatorType.Not:
                    return 6;

                case OperatorType.GreaterThan:
                case OperatorType.GreaterThanOrEquals:
                case OperatorType.LessThan:
                case OperatorType.LessThanOrEquals:
                case OperatorType.ConditionalEquals:
                case OperatorType.ConditionalNotEquals:
                    return 7;

                case OperatorType.Add:
                case OperatorType.Subtract:
                    return 8;

                case OperatorType.Multiply:
                case OperatorType.Divide:
                    return 9;

                case OperatorType.Modulo: // %%
                case OperatorType.IntegerDivide: // %/%
                case OperatorType.MatrixProduct: // %*%
                case OperatorType.OuterProduct: // %o%
                case OperatorType.KroneckerProduct: // %x%
                case OperatorType.MatchingPperator: // %in%
                case OperatorType.Special: // %abc%
                    return 10;

                case OperatorType.Sequence: // :
                    return 11;

                case OperatorType.Exponent: // ^
                    return 12;

                case OperatorType.Unary: // +, =, !
                    return 13;

                case OperatorType.FunctionCall: // (...)
                case OperatorType.Index: // [] [[]]
                    return 14;

                case OperatorType.ListIndex: // $ or @
                    return 15;

                case OperatorType.Namespace: // :: or :::
                    return 16;
            }

            return 0;
        }
    }
}
