using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Operators
{
    [DebuggerDisplay("[{OperatorType} Precedence={Precedence} Unary={IsUnary}]")]
    public sealed class TokenOperator : Operator
    {
        private OperatorType operatorType;
        private int precedence;
        private bool isUnary;

        public TokenNode OperatorToken { get; private set; }

        #region IOperator
        public override OperatorType OperatorType
        {
            get { return this.operatorType; }
        }

        public override int Precedence
        {
            get { return this.precedence; }
        }
        public override bool IsUnary
        {
            get { return this.isUnary; }
        }
        #endregion

        public TokenOperator(bool unary)
        {
            this.isUnary = unary;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            Debug.Assert(context.Tokens.CurrentToken.TokenType == RTokenType.Operator);

            this.operatorType = TokenOperator.GetOperatorType(context);
            this.OperatorToken = RParser.ParseToken(context, this);

            bool isUnary;
            this.precedence = this.GetCurrentOperatorPrecedence(context, this.OperatorType, out isUnary);

            if (!this.isUnary)
            {
                this.isUnary = isUnary;
            }


            return base.Parse(context, parent);
        }

        public override string ToString()
        {
            return this.OperatorToken.ToString();
        }

        private bool IsUnaryOperator(ParseContext context, OperatorType operatorType)
        {
            bool possibleUnary = Operator.IsPossibleUnary(operatorType);

            // We need to lock back two tokens since operator
            // parsing already consumed its token.
            TokenStream<RToken> tokens = context.Tokens;
            if (tokens.LookAhead(-2).TokenType == RTokenType.Operator)
            {
                return true;
            }

            return false;
        }
        private int GetCurrentOperatorPrecedence(ParseContext context, OperatorType operatorType, out bool isUnary)
        {
            isUnary = false;

            if (this.IsUnaryOperator(context, operatorType))
            {
                operatorType = OperatorType.Unary;
                isUnary = true;
            }

            return OperatorPrecedence.GetPrecedence(operatorType);
        }

        private static OperatorType GetOperatorType(ParseContext context)
        {
            RToken token = context.Tokens.CurrentToken;

            string text = context.TextProvider.GetText(token);
            switch (text)
            {
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
                    return OperatorType.MatchingPperator; // %in%

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

                default:
                    if (text.Length > 2 && text[0] == '%' && text[text.Length - 1] == '%')
                    {
                        for (int i = 1; i < text.Length - 1; i++)
                        {
                            if (!char.IsLetter(text[i]))
                                return OperatorType.Unknown;
                        }

                        return OperatorType.Special; // %abc%
                    }
                    break;
            }

            return OperatorType.Unknown;
        }
    }
}
