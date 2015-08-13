using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Statements.Conditionals;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Statements.Loops;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Statements
{
    [DebuggerDisplay("[{Text}]")]
    public sealed class KeywordIdentifierStatement : KeywordStatement
    {
        public TokenNode OpenBrace { get; private set; }
        public TokenNode Identifier { get; private set; }
        public TokenNode CloseBrace { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (ParseKeyword(context, this))
            {
                this.OpenBrace = RParser.ParseOpenBraceSequence(context, this);
                if (this.OpenBrace != null)
                {
                    if(context.Tokens.CurrentToken.TokenType == RTokenType.Identifier)
                    {
                        this.Identifier = RParser.ParseToken(context, this);

                        this.CloseBrace = RParser.ParseCloseBraceSequence(context, this);
                        if(this.CloseBrace != null)
                        {
                            this.Parent = parent;
                            return true;
                        }
                    }
                    else
                    {
                        context.AddError(new ParseError(ParseErrorType.IndentifierExpected, ErrorLocation.Token, context.Tokens.CurrentToken));
                    }
                }
            }

            return false;
        }
    }
}