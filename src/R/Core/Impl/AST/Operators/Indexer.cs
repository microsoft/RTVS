using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Variables
{
    public sealed class Indexer : Operator
    {
        public TokenNode LeftBrackets { get; private set; }
        public ArgumentList Arguments { get; private set; }
        public TokenNode RightBrackets { get; private set; }

        #region IOperator
        public override bool IsUnary
        {
            get { return true; }
        }

        public override OperatorType OperatorType
        {
            get { return OperatorType.Index; }
        }

        public override int Precedence
        {
            get { return OperatorPrecedence.GetPrecedence(OperatorType.Index); }
        }

        public override Association Association
        {
            get { return Association.Right; }
        }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            TokenStream<RToken> tokens = context.Tokens;

            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.OpenSquareBracket ||
                         tokens.CurrentToken.TokenType == RTokenType.OpenDoubleSquareBracket);

            this.LeftBrackets = RParser.ParseToken(context, this);

            RTokenType terminatingTokenType = RParser.GetTerminatingTokenType(this.LeftBrackets.Token.TokenType);

            this.Arguments = new ArgumentList(terminatingTokenType);
            if (this.Arguments.Parse(context, this))
            {
                if (tokens.CurrentToken.TokenType == terminatingTokenType)
                {
                    this.RightBrackets = RParser.ParseToken(context, this);
                    return base.Parse(context, parent);
                }
                else
                {
                    context.Errors.Add(new MissingItemParseError(ParseErrorType.CloseSquareBracketExpected, tokens.PreviousToken));
                }
            }

            return false;
        }
    }
}
