using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Parser
{
    public sealed partial class RParser
    {
        public AstRoot Parse(string text)
        {
            return this.Parse(new TextStream(text));
        }

        public AstRoot Parse(ITextProvider textProvider)
        {
            return this.Parse(textProvider, new TextRange(0, textProvider.Length));
        }

        /// <summary>
        /// Parse text from a text provider within a given range
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="range">Range to parse</param>
        public AstRoot Parse(ITextProvider textProvider, ITextRange range)
        {
            var tokenizer = new Tokenizer();

            IReadOnlyTextRangeCollection<RToken> tokens = tokenizer.Tokenize(textProvider, range.Start, range.Length);
            TokenStream<RToken>  tokenStream = new TokenStream<RToken>(tokens, new RToken(RTokenType.EndOfStream, TextRange.EmptyRange));
            ParseContext context = new ParseContext(textProvider, range, tokenStream);

            AstRoot tree = new AstRoot(textProvider);
            tree.Parse(context, tree);
            tree.Errors = context.Errors;

            return tree;
        }
    }
}
