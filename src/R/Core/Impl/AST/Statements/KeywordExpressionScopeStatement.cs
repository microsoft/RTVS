using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements
{
    /// <summary>
    /// Statement that is based on a keyword and condition 
    /// followed by a scope typically in a form of 
    /// 'keyword ( expression ) { }'.
    /// </summary>
    public class KeywordExpressionScopeStatement : KeywordExpressionStatement, IKeywordExpressionScopeStatement
    {
        public IScope Scope { get; private set; }

        private string _terminatingKeyword;

        public KeywordExpressionScopeStatement():
            this(null)
        {
        }

        public KeywordExpressionScopeStatement(string terminatingKeyword)
        {
            _terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (base.Parse(context, parent))
            {
                IScope scope = RParser.ParseScope(context, this, allowsSimpleScope: true, 
                                                  terminatingKeyword: _terminatingKeyword);
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