using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Statements.Conditionals;
using Microsoft.R.Core.AST.Statements.Loops;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Statements
{
    [DebuggerDisplay("[{Text}]")]
    public class KeywordStatement : Statement
    {
        public TokenNode Keyword { get; private set; }

        public string Text { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            this.Keyword = RParser.ParseKeyword(context, this);
            this.Text = context.TextProvider.GetText(this.Keyword);

            return base.Parse(context, parent);
        }

        /// <summary>
        /// Abstract factory
        /// </summary>
        public static Statement CreateStatement(ParseContext context, IAstNode parent)
        {
            RToken currentToken = context.Tokens.CurrentToken;
            string keyword = context.TextProvider.GetText(currentToken);
            Statement statement = null;

            switch (keyword)
            {
                case "if":
                    statement = new If();
                    break;

                case "for":
                    statement = new For();
                    break;

                case "while":
                    statement = new KeywordExpressionScopeStatement();
                    break;

                case "repeat":
                    statement = new KeywordScopeStatement(allowsSimpleScope: false);
                    break;

                case "break":
                case "next":
                    statement = new KeywordStatement();
                    break;

                case "return":
                case "typeof":
                case "library":
                    statement = new KeywordBracesStatement();
                    break;
            }

            return statement;
        }
    }
}