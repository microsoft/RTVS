using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Statements
{
    public class KeywordBracesStatement : KeywordStatement
    {
        public TokenNode OpenBrace { get; private set; }
        public Expression Expression { get; private set; }
        public TokenNode CloseBrace { get; private set; }

        protected override bool ParseKeywordSequence(ParseContext context)
        {
            TokenStream<RToken> tokens = context.Tokens;

            base.ParseKeywordSequence(context);

            this.OpenBrace = RParser.ParseOpenBraceSequence(context, this);
            if (this.OpenBrace != null)
            {
                if (this.ParseExpression(context, this))
                {
                    this.CloseBrace = RParser.ParseCloseBraceSequence(context, this);
                    if (this.CloseBrace != null)
                    {
                        return base.Parse(context, this);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Allows derived classes to parse expression inside braces
        /// </summary>
        protected virtual bool ParseExpression(ParseContext context, IAstNode parent)
        {
            this.Expression = new Expression();
            if (this.Expression.Parse(context, this))
            {
                return base.Parse(context, parent);
            }

            return false;
        }
    }
}