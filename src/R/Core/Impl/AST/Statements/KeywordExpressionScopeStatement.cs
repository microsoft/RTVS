using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements
{
    /// <summary>
    /// Statement that is based on a keyword and condition typically
    /// in a form of 'keyword ( expression )'. It may or may not
    /// have scope.
    /// </summary>
    public class KeywordExpressionScopeStatement : KeywordBracesStatement
    {
        public IScope Scope { get; private set; }

        private string terminatingKeyword;

        public KeywordExpressionScopeStatement():
            this(null)
        {
        }

        public KeywordExpressionScopeStatement(string terminatingKeyword)
        {
            this.terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (base.Parse(context, parent))
            {
                IScope scope = RParser.ParseScope(context, this, allowsSimpleScope: true, 
                                                  terminatingKeyword: this.terminatingKeyword);
                if(scope != null)
                {
                    this.Scope = scope;
                    return true;
                }
            }

            return false;
        }
    }
}