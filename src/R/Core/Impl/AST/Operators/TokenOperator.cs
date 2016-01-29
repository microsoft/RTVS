using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Operators {
    [DebuggerDisplay("[{OperatorType} Precedence={Precedence} Unary={IsUnary}]")]
    public sealed class TokenOperator : Operator {
        private OperatorType _operatorType;
        private int _precedence;
        private bool _isUnary;

        public TokenNode OperatorToken { get; private set; }

        #region IOperator
        public override OperatorType OperatorType {
            get { return _operatorType; }
        }

        public override int Precedence {
            get { return _precedence; }
        }
        public override bool IsUnary {
            get { return _isUnary; }
        }
        #endregion

        public TokenOperator(bool unary) {
            _isUnary = unary;
        }
        public TokenOperator(OperatorType operatorType, bool unary) :
            this(unary) {
            _operatorType = operatorType;
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            Debug.Assert(context.Tokens.CurrentToken.TokenType == RTokenType.Operator);

            _operatorType = TokenOperator.GetOperatorType(context.TextProvider.GetText(context.Tokens.CurrentToken));
            this.OperatorToken = RParser.ParseToken(context, this);
            this.Association = OperatorAssociation.GetAssociation(_operatorType);

            bool isUnary;
            _precedence = this.GetCurrentOperatorPrecedence(context, this.OperatorType, out isUnary);

            if (!_isUnary) {
                _isUnary = isUnary;
            }

            // http://www.inside-r.org/r-doc/stats/formula
            // Handle multiple ~ by simply skipping over the remaining ones.
            // This will change when actual formula object will need to be built.
            while(!context.Tokens.IsEndOfStream() && context.Tokens.CurrentToken.TokenType == RTokenType.Operator) {
                string operatorText = context.TextProvider.GetText(context.Tokens.CurrentToken);
                if(operatorText != "~") {
                    break;
                }
                context.Tokens.MoveToNextToken();
            }

            return base.Parse(context, parent);
        }

        public override string ToString() {
            return this.OperatorToken.ToString();
        }

        private int GetCurrentOperatorPrecedence(ParseContext context, OperatorType operatorType, out bool isUnary) {
            isUnary = false;

            if (IsUnaryOperator(context.Tokens, operatorType, -1)) {
                operatorType = OperatorType.Unary;
                isUnary = true;
            }

            return OperatorPrecedence.GetPrecedence(operatorType);
        }

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
                        for (int i = 1; i < text.Length - 1; i++) {
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
