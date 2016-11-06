// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Parser {
    public sealed partial class RParser {
        public static AstRoot Parse(string text) {
            return RParser.Parse(new TextStream(text), null);
        }

        public static AstRoot Parse(ITextProvider textProvider, IExpressionTermFilter filter = null) {
            return RParser.Parse(textProvider, new TextRange(0, textProvider.Length), filter);
        }

        public static AstRoot Parse(ITextProvider textProvider, ITextRange range, IExpressionTermFilter filter) {
            var tokenizer = new RTokenizer(separateComments: true);

            IReadOnlyTextRangeCollection<RToken> tokens = tokenizer.Tokenize(textProvider, range.Start, range.Length);
            TokenStream<RToken> tokenStream = new TokenStream<RToken>(tokens, new RToken(RTokenType.EndOfStream, TextRange.EmptyRange));

            return Parse(textProvider, range, tokenStream, tokenizer.CommentTokens, filter);
        }

        /// <summary>
        /// Parse text from a text provider within a given range
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="range">Range to parse</param>
        internal static AstRoot Parse(ITextProvider textProvider, ITextRange range, TokenStream<RToken> tokenStream, IReadOnlyList<RToken> commentTokens, IExpressionTermFilter filter) {
            ParseContext context = new ParseContext(textProvider, range, tokenStream, commentTokens, filter);
            return Parse(context);
        }

        /// <summary>
        /// Parse text from a text provider within a given range
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="range">Range to parse</param>
        internal static AstRoot Parse(ParseContext context) {
            context.AstRoot.Parse(context, context.AstRoot);
            context.AstRoot.Errors = new TextRangeCollection<IParseError>(context.Errors);
            return context.AstRoot;
        }
    }
}
