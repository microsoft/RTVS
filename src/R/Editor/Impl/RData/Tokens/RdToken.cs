// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.R.Editor.RData.Tokens {
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public class RdToken : Token<RdTokenType>, IComparable<RdToken> {
        public static RdToken EndOfStreamToken = new RdToken(RdTokenType.EndOfStream);

        public RdToken(RdTokenType tokenType)
            : this(tokenType, TextRange.EmptyRange) {
        }

        public RdToken(RdTokenType tokenType, ITextRange range)
            : base(tokenType, range) {
        }

        public bool ContentTypeChange { get; set; }

        public bool IsKeywordText(ITextProvider textProvider, string keywordText) {
            if (this.TokenType == RdTokenType.Keyword) {
                return textProvider.CompareTo(this.Start, this.Length, keywordText, ignoreCase: false);
            }

            return false;
        }

        public int CompareTo(RdToken other) {
            if (other == null) {
                return -1;
            }

            if (this.TokenType == other.TokenType) {
                return 0;
            }

            if ((int)this.TokenType < (int)other.TokenType) {
                return -1;
            }

            return 1;
        }
    }
}
