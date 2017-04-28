// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Markdown.Editor.Tokens {
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public class MarkdownToken : Token<MarkdownTokenType>, IComparable<MarkdownToken> {
        public static MarkdownToken EndOfStreamToken = new MarkdownToken(MarkdownTokenType.EndOfStream);

        public MarkdownToken(MarkdownTokenType tokenType)
            : this(tokenType, TextRange.EmptyRange) {
        }

        public MarkdownToken(MarkdownTokenType tokenType, ITextRange range)
            : base(tokenType, range) {
        }

        public int CompareTo(MarkdownToken other) {
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
