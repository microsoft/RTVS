using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Statements
{
    /// <summary>
    /// Represents statement: assignment, function definition,
    /// function call, conditional statement and so on.
    /// </summary>
    public class Statement : AstNode
    {
        /// <summary>
        /// Optional terminating semicolon
        /// </summary>
        public TokenNode Semicolon { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (context.Tokens.CurrentToken.TokenType == RTokenType.Semicolon)
            {
                this.Semicolon = RParser.ParseToken(context, this);
            }

            return base.Parse(context, parent);
        }

        /// <summary>
        /// Abstract factory creating statements depending on current
        /// token and the following token sequence
        /// </summary>
        /// <returns></returns>
        public static Statement Create(ParseContext context, IAstNode parent)
        {
            TokenStream<RToken> tokens = context.Tokens;
            RToken currentToken = tokens.CurrentToken;

            Statement statement = null;

            switch (currentToken.TokenType)
            {
                case RTokenType.Keyword:
                    // If statement starts with a keyword, it is not an assignment
                    // hence we should always try keyword based statements first.
                    // Some of the statements may be R-values like typeof() but
                    // in case of the statement appearing on its own return value
                    // will be simply ignored. IDE may choose to show a warning.
                    statement = KeywordStatement.Create(context, parent);
                    break;

                case RTokenType.Semicolon:
                    statement = new Statement();
                    break;

                default:
                    // Possible L-value in a left-hand assignment, 
                    // a function call or R-value in a right hand assignment.
                    statement = new ExpressionStatement();
                    break;
            }

            return statement;
        }

        public override string ToString()
        {
            if (this.Semicolon != null && this.Root != null)
            {
                return this.Root.TextProvider.GetText(this.Semicolon);
            }

            return string.Empty;
        }
    }
}
