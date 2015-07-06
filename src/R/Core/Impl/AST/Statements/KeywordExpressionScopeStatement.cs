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

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (this.ParseKeywordSequence(context))
            {
                IScope scope = RParser.ParseScope(context, this, allowsSimpleScope: true);
                if(scope != null)
                {
                    this.Scope = scope;
                    return base.Parse(context, parent);
                }
            }

            return false;
        }
    }
}