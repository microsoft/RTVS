using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST.Scopes
{
    [DebuggerDisplay("Global Scope, Children: {Children.Count} [{Start}...{End})")]
    public sealed class GlobalScope : Scope
    {
        public GlobalScope() :
            base("Global")
        {
        }

        #region ITextRange
        public override int Start
        {
            get { return 0; }
        }

        public override int End
        {
            get
            {
                if (Root != null && Root.TextProvider != null)
                {
                    return Root.TextProvider.Length;
                }

                return base.End;
            }
        }

        public override bool Contains(int position)
        {
            return position >= Start && position <= End;
        }
        #endregion
    }
}
