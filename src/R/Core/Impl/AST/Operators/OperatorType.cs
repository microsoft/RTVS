// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Operators {
    public enum OperatorType {
        Unknown,
        Add,
        Subtract,
        Multiply,
        Divide,
        Exponent, // ^
        Tilde, // ~ (formula)
        Equals, // =
        Modulo, // %%
        IntegerDivide, // %/%
        MatrixProduct, // %*%
        OuterProduct, // %o%
        KroneckerProduct, // %x%
        MatchingOperator, // %in%
        Special, // %abc%
        GreaterThan,
        GreaterThanOrEquals,
        LessThan,
        LessThanOrEquals,
        ListIndex, // $
        Sequence, // :
        Not, // !
        And, // &
        Or, // |
        DataTableAssign, // :=
        ConditionalAnd, // &&
        CondtitionalOr, // ||
        ConditionalEquals, // ==
        ConditionalNotEquals, // !=
        Namespace, // ::, :::
        Index, // [ ]
        FunctionCall, // ()
        LeftAssign, // <- <<-
        RightAssign, // -> ->>
        Help, // ? and ??
        UnaryMinus,
        UnaryPlus,
        Group, // ( ) pseudo-operator
        Sentinel // pseudo-type used in expression parsing
    }
}
