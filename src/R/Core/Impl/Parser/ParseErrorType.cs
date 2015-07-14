namespace Microsoft.R.Core.Parser
{
    /// <summary>
    /// Type of parsing error
    /// </summary>
    public enum ParseErrorType
    {
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
        /// Identifier appears to be missing. For example, 
        /// two binary operators without anything between them.
        /// </summary>
        OperandExpected,

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
