using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Values {
    /// <summary>
    /// Represents string constant
    /// </summary>
    public sealed class StringValue : RValueTokenNode<RString> {
        public override bool Parse(ParseContext context, IAstNode parent) {
            RToken currentToken = context.Tokens.CurrentToken;
            string text = context.TextProvider.GetText(currentToken);

            Debug.Assert(currentToken.TokenType == RTokenType.String);

            NodeValue = new RString(text);
            return base.Parse(context, parent);
        }
    }
}
