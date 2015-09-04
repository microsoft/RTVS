using System.Diagnostics;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Statements
{
    [DebuggerDisplay("[Library: {LibraryName}]")]
    public sealed class LibraryStatement : KeywordIdentifierStatement
    {
        public string LibraryName
        {
            get
            {
                string name = string.Empty;

                if(Identifier != null)
                {
                    name = Root.TextProvider.GetText(Identifier);
                    if(Identifier.Token.TokenType == RTokenType.String)
                    {
                        if (name.Length > 0)
                            name = name.Substring(1);

                        if (name.Length > 0 && (name[name.Length-1] == '\"' || name[name.Length - 1] == '\''))
                            name = name.Substring(0, name.Length-1);
                    }
                }

                return name;
            }
        }
   }
}