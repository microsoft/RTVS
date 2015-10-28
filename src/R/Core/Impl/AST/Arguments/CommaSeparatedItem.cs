using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments {
    /// <summary>
    /// Argument to a function or to andindexer. Generally
    /// something that is followed by an optional comma.
    /// </summary>
    public class CommaSeparatedItem : AstNode {
        /// <summary>
        /// Optional trailing comma
        /// </summary>
        public TokenNode Comma { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent) {
            if (context.Tokens.CurrentToken.TokenType == RTokenType.Comma) {
                this.Comma = RParser.ParseToken(context, this);
            }

            return base.Parse(context, parent);
        }
    }
}
