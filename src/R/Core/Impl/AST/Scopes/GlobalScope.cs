using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST.Scopes
{
    public sealed class GlobalScope : Scope
    {
        public GlobalScope() :
            base("Global")
        {
        }
    }
}
