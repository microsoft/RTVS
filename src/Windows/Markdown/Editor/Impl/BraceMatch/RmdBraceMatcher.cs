// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Core.Braces;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.BraceMatch {
    internal sealed class RmdBraceMatcher : BraceMatcher<BraceToken, BraceTokenType> {
        static RmdBraceMatcher() {
            BraceTypeToTokenTypeMap.Add(BraceType.Curly, new BraceTokenPair<BraceTokenType>(BraceTokenType.OpenCurly, BraceTokenType.CloseCurly));
            BraceTypeToTokenTypeMap.Add(BraceType.Parenthesis, new BraceTokenPair<BraceTokenType>(BraceTokenType.OpenBrace, BraceTokenType.CloseBrace));
            BraceTypeToTokenTypeMap.Add(BraceType.Square, new BraceTokenPair<BraceTokenType>(BraceTokenType.OpenBracket, BraceTokenType.CloseBracket));
        }

        public RmdBraceMatcher(ITextView textView, ITextBuffer textBuffer) : 
            base(textView, textBuffer, new TokenComparer()) {
        }

        protected override IReadOnlyTextRangeCollection<BraceToken> GetTokens(int start, int length) {
            var tokenizer = new BraceTokenizer();
            return tokenizer.Tokenize(TextBuffer.CurrentSnapshot.GetText());
        }

        private class TokenComparer : IComparer<BraceTokenType> {
            public int Compare(BraceTokenType x, BraceTokenType y) => x.CompareTo(y);
        }
    }
}
