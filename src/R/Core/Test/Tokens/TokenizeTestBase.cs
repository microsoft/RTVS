using Microsoft.Languages.Core.Diagnostics;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Test.Tokens
{
    public class TokenizeTestBase: UnitTestBase
    {
        protected string TokenizeToString(string text)
        {
            var tokens = this.Tokenize(text);
            string result = DebugWriter.WriteTokens<RToken, RTokenType>(tokens);

            return result;
        }

        protected IReadOnlyTextRangeCollection<RToken> Tokenize(string text)
        {
            ITextProvider textProvider = new TextStream(text);
            var tokenizer = new Tokenizer();

            return tokenizer.Tokenize(textProvider, 0, textProvider.Length);
        }
    }
}
