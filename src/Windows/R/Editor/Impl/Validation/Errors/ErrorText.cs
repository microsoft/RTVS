// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Validation.Errors
{
    internal static class ErrorText
    {
        public static string GetText(ParseErrorType errorType)
        {
            switch (errorType)
            {
                default:
                    return Resources.ParseError_General;

                case ParseErrorType.UnexpectedToken:
                    return Resources.ParseError_UnexpectedToken;

                case ParseErrorType.IndentifierExpected:
                    return Resources.ParseError_IndentifierExpected;

                case ParseErrorType.NumberExpected:
                    return Resources.ParseError_NumberExpected;

                case ParseErrorType.LogicalExpected:
                    return Resources.ParseError_LogicalExpected;

                case ParseErrorType.StringExpected:
                    return Resources.ParseError_StringExpected;

                case ParseErrorType.ExpressionExpected:
                    return Resources.ParseError_ExpressionExpected;

                case ParseErrorType.LeftOperandExpected:
                    return Resources.ParseError_LeftOperandExpected;

                case ParseErrorType.RightOperandExpected:
                    return Resources.ParseError_RightOperandExpected;

                case ParseErrorType.OpenCurlyBraceExpected:
                    return Resources.ParseError_OpenCurlyBraceExpected;

                case ParseErrorType.CloseCurlyBraceExpected:
                    return Resources.ParseError_CloseCurlyBraceExpected;

                case ParseErrorType.OpenBraceExpected:
                    return Resources.ParseError_OpenBraceExpected;

                case ParseErrorType.CloseBraceExpected:
                    return Resources.ParseError_CloseBraceExpected;

                case ParseErrorType.OpenSquareBracketExpected:
                    return Resources.ParseError_OpenSquareBracketExpected;

                case ParseErrorType.CloseSquareBracketExpected:
                    return Resources.ParseError_CloseSquareBracketExpected;

                case ParseErrorType.OperatorExpected:
                    return Resources.ParseError_OperatorExpected;

                case ParseErrorType.FunctionExpected:
                    return Resources.ParseError_FunctionExpected;

                case ParseErrorType.InKeywordExpected:
                    return Resources.ParseError_InKeywordExpected;

                case ParseErrorType.UnexpectedEndOfFile:
                    return Resources.ParseError_UnexpectedEndOfFile;

                case ParseErrorType.FunctionBodyExpected:
                    return Resources.ParseError_FunctionBodyExpected;
            }
        }
    }
}
