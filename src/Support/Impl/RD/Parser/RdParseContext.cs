using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    public sealed class RdParseContext
    {
        public ITextProvider TextProvider { get; private set; }

        public TokenStream<RdToken> Tokens { get; private set; }

        public RdParseContext(IReadOnlyTextRangeCollection<RdToken> tokens, ITextProvider textProvider)
        {
            this.TextProvider = textProvider;
            this.Tokens = new TokenStream<RdToken>(tokens, RdToken.EndOfStreamToken);
        }
    }
}
