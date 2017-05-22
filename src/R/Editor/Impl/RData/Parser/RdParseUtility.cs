// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Editor.RData.Tokens;

namespace Microsoft.R.Editor.RData.Parser {
    static class RdParseUtility {
        public static bool GetKeywordArgumentBounds(TokenStream<RdToken> tokens, out int startTokenIndex, out int endTokenIndex) {
            startTokenIndex = -1;
            endTokenIndex = -1;

            TokenBraceCounter<RdToken> braceCounter = new TokenBraceCounter<RdToken>(
                new RdToken(RdTokenType.OpenCurlyBrace),
                new RdToken(RdTokenType.CloseCurlyBrace),
                new RdToken(RdTokenType.OpenSquareBracket),
                new RdToken(RdTokenType.CloseSquareBracket),
                new RdTokenTypeComparer());

            for (int pos = tokens.Position; pos < tokens.Length; pos++) {
                RdToken token = tokens[pos];

                if (braceCounter.CountBrace(token)) {
                    if (startTokenIndex < 0) {
                        startTokenIndex = pos;
                    }

                    if (braceCounter.Count == 0) {
                        endTokenIndex = pos;
                        break;
                    }
                }
            }

            return startTokenIndex >= 0 && endTokenIndex >= 0 && startTokenIndex < endTokenIndex;
        }
    }
}
