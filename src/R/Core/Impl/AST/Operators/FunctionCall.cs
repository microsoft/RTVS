using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Operators
{
    public sealed class FunctionCall : Operator, IFunction
    {
        #region IFunction
        public TokenNode OpenBrace { get; private set; }

        public ArgumentList Arguments { get; private set; }

        public TokenNode CloseBrace { get; private set; }
        #endregion

        #region IOperator
        public override OperatorType OperatorType
        {
            get { return OperatorType.FunctionCall; }
        }

        public override int Precedence
        {
            get { return OperatorPrecedence.GetPrecedence(OperatorType.FunctionCall); }
        }

        public override bool IsUnary
        {
            get { return true; }
        }

        public override Association Association
        {
            get { return Association.Right; }
        }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            TokenStream<RToken> tokens = context.Tokens;

            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.OpenBrace);
            this.OpenBrace = RParser.ParseToken(context, this);

            this.Arguments = new ArgumentList(RTokenType.CloseBrace);
            if (this.Arguments.Parse(context, this))
            {
                if (tokens.CurrentToken.TokenType == RTokenType.CloseBrace)
                {
                    this.CloseBrace = RParser.ParseToken(context, this);
                    return base.Parse(context, parent);
                }

                context.AddError(new MissingItemParseError(ParseErrorType.CloseBraceExpected, tokens.PreviousToken));
            }

            return false;
        }
    }
}
