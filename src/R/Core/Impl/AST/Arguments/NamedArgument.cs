using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments
{
    [DebuggerDisplay("[{Name}]")]
    public class NamedArgument : ExpressionArgument
    {
        public ITextRange NameRange
        {
            get { return this.Identifier; }
        }
        public string Name
        {
            get { return this.Root.TextProvider.GetText(this.NameRange); }
        }

        public TokenNode Identifier { get; private set; }

        public TokenNode EqualsSign { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            TokenStream<RToken> tokens = context.Tokens;

            if (tokens.CurrentToken.TokenType == RTokenType.Identifier &&
                tokens.NextToken.TokenType == RTokenType.Operator &&
                context.TextProvider.GetText(tokens.NextToken) == "=")
            {
                this.Identifier = RParser.ParseToken(context, this);
                this.EqualsSign = RParser.ParseToken(context, this);
            }

            return base.Parse(context, parent);
        }
    }
}
