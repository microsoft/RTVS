using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Statements
{
    [DebuggerDisplay("[Function: {Keyword}]")]
    public sealed class KeywordFunctionStatement : FunctionCall, IKeywordFunction, IStatement
    {
        #region IKeyword
        public TokenNode Keyword { get; private set; }
        public string Text { get; private set; }
        #endregion

        #region IStatement
        public TokenNode Semicolon { get; private set; }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            Debug.Assert(context.Tokens.CurrentToken.TokenType == RTokenType.Keyword);

            this.Keyword = RParser.ParseKeyword(context, this);
            this.Text = context.TextProvider.GetText(this.Keyword);

            bool result = base.Parse(context, parent);

            if(context.Tokens.CurrentToken.TokenType == RTokenType.Semicolon)
            {
                this.Semicolon = RParser.ParseToken(context, this);
            }

            return result;
        }
    }
}