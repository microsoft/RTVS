// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens {
    /// <summary>
    /// Implements <seealso cref="IToken"/>. Derives from <seealso cref="TextRange"/>
    /// </summary>
    /// <typeparam name="T">Token type (typically enum)</typeparam>
    public class Token<T> : TextRange, IToken<T> {
        /// <summary>
        /// Create token based on type and text range
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="range">Token range in the text provider</param>
        public Token(T tokenType, ITextRange range)
            : this(tokenType, range.Start, range.Length) {
        }

        /// <summary>
        /// Create token based on type and text range
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="range">Token range in the text provider</param>
        public Token(T tokenType, int start, int length)
            : base(start, length) {
            this.TokenType = tokenType;
        }

        /// <summary>
        /// Token type
        /// </summary>
        public T TokenType { get; protected set; }
    }
}
