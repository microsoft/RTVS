// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Operators {
    [DebuggerDisplay("[{OperatorType} Precedence={Precedence} Unary={IsUnary}]")]
    public sealed class TokenOperator : Operator {
        public TokenNode OperatorToken { get; private set; }

        public TokenOperator(bool firstInExpression) {
            IsUnary = firstInExpression;
            if (IsUnary) {
                Associativity = Associativity.Right;
            }
        }

        public TokenOperator(OperatorType operatorType, bool unary) :
            this(unary) {
            OperatorType = operatorType;
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            Debug.Assert(context.Tokens.CurrentToken.TokenType == RTokenType.Operator);

            OperatorType = TokenOperator.GetOperatorType(context.TextProvider.GetText(context.Tokens.CurrentToken));
            OperatorToken = RParser.ParseToken(context, this);
            Associativity = OperatorAssociativity.GetAssociativity(OperatorType);

            // If operator is preceded by an operator, it is then unary
            // Look back two tokens since operator parsing already consumed its token.
            if (IsUnary || IsUnaryOperator(context.Tokens, context.TextProvider, OperatorType, -2)) {
                OperatorType = Operator.GetUnaryForm(OperatorType);
                IsUnary = true;
                Associativity = Associativity.Right;
            }
            return base.Parse(context, parent);
        }

        public override string ToString() => OperatorToken.ToString();

        public static OperatorType GetOperatorType(string text) {
            switch (text) {
                case "+":
                    return OperatorType.Add;

                case "-":
                    return OperatorType.Subtract;

                case "*":
                    return OperatorType.Multiply;

                case "/":
                    return OperatorType.Divide;

                case "^":
                case "**":
                    return OperatorType.Exponent;

                case "%%":
                    return OperatorType.Modulo; // %%

                case "%/%":
                    return OperatorType.IntegerDivide; // %/%

                case "%*%":
                    return OperatorType.MatrixProduct; // %*%

                case "%o%":
                    return OperatorType.OuterProduct; // %o%

                case "%x%":
                    return OperatorType.KroneckerProduct; // %x%

                case "%in%":
                    return OperatorType.MatchingOperator; // %in%

                case ">":
                    return OperatorType.GreaterThan;

                case ">=":
                    return OperatorType.GreaterThanOrEquals;

                case "<":
                    return OperatorType.LessThan;

                case "<=":
                    return OperatorType.LessThanOrEquals;

                case "$":
                case "@":
                    return OperatorType.ListIndex; // $

                case ":":
                    return OperatorType.Sequence; // :

                case "!":
                    return OperatorType.Not; // !

                case "&":
                    return OperatorType.And; // &

                case "|":
                    return OperatorType.Or; // |

                case "&&":
                    return OperatorType.ConditionalAnd; // &&

                case "||":
                    return OperatorType.CondtitionalOr; // ||

                case "==":
                    return OperatorType.ConditionalEquals; // ==

                case "!=":
                    return OperatorType.ConditionalNotEquals; // !=

                case ":=":
                    return OperatorType.DataTableAssign; // :=

                case "::":
                case ":::":
                    return OperatorType.Namespace;

                case "~":
                    return OperatorType.Tilde;

                case "<-":
                case "<<-":
                    return OperatorType.LeftAssign;

                case "->":
                case "->>":
                    return OperatorType.RightAssign;

                case "=":
                    return OperatorType.Equals;

                case "?":
                case "??":
                    return OperatorType.Help;

                default:
                    if (text.Length > 2 && text[0] == '%' && text[text.Length - 1] == '%') {
                        return OperatorType.Special; // %abc%
                    }
                    break;
            }

            return OperatorType.Unknown;
        }
    }
}
