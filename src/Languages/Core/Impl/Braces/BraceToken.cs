// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Languages.Core.Braces {
    public sealed class BraceToken : TextRange, IToken<BraceTokenType> {
        public BraceTokenType TokenType { get; }

        public BraceToken(int start, int length, BraceTokenType tokenType) : base(start, length) {
            TokenType = tokenType;
        }
    }
}
