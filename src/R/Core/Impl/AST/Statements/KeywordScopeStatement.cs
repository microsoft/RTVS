using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements
{
    /// <summary>
    /// Statement with keyword and scope { } such as repeat { } and else { }
    /// </summary>
    public sealed class KeywordScopeStatement : KeywordStatement
    {
        public IScope Scope { get; private set; }

        private bool allowsSimpleScope;

        public KeywordScopeStatement(bool allowsSimpleScope)
        {
            this.allowsSimpleScope = allowsSimpleScope;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (base.Parse(context, parent))
            {
                IScope scope = RParser.ParseScope(context, this, this.allowsSimpleScope, terminatingKeyword: null);
                if (scope != null)
                {
                    this.Scope = scope;
                    return true;
                }
            }

            return false;
        }
    }
}