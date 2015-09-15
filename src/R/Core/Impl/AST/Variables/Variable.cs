using System;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Variables
{
    /// <summary>
    /// Represents R variable.
    /// </summary>
    [DebuggerDisplay("[{Name} : {Start}...{End}), Length = {Length}")]
    public sealed class Variable : TokenNode, IRValueNode
    {
        public string Name { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            RToken currentToken = context.Tokens.CurrentToken;

            Debug.Assert(currentToken.TokenType == RTokenType.Identifier);

            this.Name = context.TextProvider.GetText(currentToken);

            // Not calling base since expression parser will decide 
            // what parent node the variable belongs to.
            this.Token = currentToken;
            context.Tokens.MoveToNextToken();

            return true;
        }

        public RObject GetValue()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
