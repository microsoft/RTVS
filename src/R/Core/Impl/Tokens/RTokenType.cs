// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.Core.Tokens {
    public enum RTokenType {
        /// <summary>
        /// Unrecognized token
        /// </summary>
        Unknown,

        /// <summary>
        /// Any non-whitespace sequence not recognized as
        /// anything else. Leaves to the parser to handle this.
        /// </summary>
        Identifier,

        /// <summary>
        /// # comment, lasts to the end of the line
        /// </summary>
        Comment,

        /// <summary>
        /// "..." sequence
        /// </summary>
        String,

        /// <summary>
        /// Known language keyword like 'if', 'while', 'for', ...
        /// </summary>
        Keyword,

        /// <summary>
        /// Integer or floating point number
        /// </summary>
        Number,

        /// <summary>
        /// Complex number like 1+2i
        /// </summary>
        Complex,

        /// <summary>
        /// Logical constant (TRUE or FALSE)
        /// </summary>
        Logical,

        /// <summary>
        /// NULL constant
        /// </summary>
        Null,

        // NA constant
        Missing,

        // Inf constant
        Infinity,

        // NaN constant
        NaN,

        /// <summary>
        /// Language operator like +, -, *, %%, ...
        /// </summary>
        Operator,

        OpenCurlyBrace,
        CloseCurlyBrace,
        OpenSquareBracket,
        CloseSquareBracket,
        OpenDoubleSquareBracket,
        CloseDoubleSquareBracket,
        OpenBrace,
        CloseBrace,
        Comma,
        Semicolon,

        /// <summary>
        /// '...' sequence, typically argument indicating
        /// any number of parameters in the function definitions
        /// </summary>
        Ellipsis,

        /// <summary>
        /// Preudo-token indicating end of stream
        /// </summary>
        EndOfStream
    }
}
