// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.Parser {
    /// <summary>
    /// Type of parsing error
    /// </summary>
    public enum ParseErrorType {
        /// <summary>
        /// No errors
        /// </summary>
        None,

        /// <summary>
        /// Unknown or general syntax error
        /// </summary>
        Unknown,

        /// <summary>
        /// Unexpected token such as curly braces
        /// inside function parameter list
        /// </summary>
        UnexpectedToken,

        IndentifierExpected,

        /// <summary>
        /// Numerical value is expected
        /// </summary>
        NumberExpected,

        /// <summary>
        /// Logical value is expected
        /// </summary>
        LogicalExpected,

        /// <summary>
        /// String value is expected
        /// </summary>
        StringExpected,

        /// <summary>
        /// Expression expected
        /// </summary>
        ExpressionExpected,

        /// <summary>
        /// Identifier or complete expression to the right of the operator 
        /// appears to be missing. For example, two binary operators without 
        /// anything between them or expression like x <- y + '.
        /// </summary>
        RightOperandExpected,

        /// <summary>
        /// Identifier or complete expression to the left of the operator 
        /// appears to be missing. For example, [] without indetifier or
        /// expression to apply the indexer to.
        /// </summary>
        LeftOperandExpected,

        /// <summary>
        /// function(a) without anything after it
        /// </summary>
        FunctionBodyExpected,

        OpenCurlyBraceExpected,
        CloseCurlyBraceExpected,
        OpenBraceExpected,
        CloseBraceExpected,
        OpenSquareBracketExpected,
        CloseSquareBracketExpected,
        OperatorExpected,
        FunctionExpected,
        InKeywordExpected,
        UnexpectedEndOfFile,
    }
}
