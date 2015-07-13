using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser.Definitions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Parser
{
    public sealed partial class RParser
    {
        public static AstRoot Parse(string text)
        {
            return RParser.Parse(new TextStream(text));
        }

        public static AstRoot Parse(ITextProvider textProvider)
        {
            return RParser.Parse(textProvider, new TextRange(0, textProvider.Length));
        }

        /// <summary>
        /// Parse text from a text provider within a given range
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="range">Range to parse</param>
        public static AstRoot Parse(ITextProvider textProvider, ITextRange range)
        {
            var tokenizer = new Tokenizer();

            IReadOnlyTextRangeCollection<RToken> tokens = tokenizer.Tokenize(textProvider, range.Start, range.Length);
            TokenStream<RToken>  tokenStream = new TokenStream<RToken>(tokens, new RToken(RTokenType.EndOfStream, TextRange.EmptyRange));

            AstRoot astRoot = new AstRoot(textProvider);
            ParseContext context = new ParseContext(astRoot, range, tokenStream);

            astRoot.Parse(context, astRoot);
            astRoot.Errors = new TextRangeCollection<IParseError>(context.Errors);

            return astRoot;
        }
    }
}
