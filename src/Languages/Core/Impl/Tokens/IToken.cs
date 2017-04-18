// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens {
    /// <summary>
    /// Describes a token. Parse token is a text range
    /// with a type that describes nature of the range.
    /// Derives from <seealso cref="ITextRange"/>
    /// </summary>
    /// <typeparam name="T">Token type (typically enum)</typeparam>
    public interface IToken<T> : ITextRange {
        /// <summary>
        /// Type of the token
        /// </summary>
        T TokenType { get; }
    }
}
