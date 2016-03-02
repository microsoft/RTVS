// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.Parser {
    /// <summary>
    /// Location of the parsing error.
    /// Gives hint to IDE what to squiggle.
    /// </summary>
    public enum ErrorLocation {
        /// <summary>
        /// Whitespace or token before the provided 
        /// text range. Relatively rare case.
        /// </summary>
        BeforeToken,

        /// <summary>
        /// The range specified such as when 
        /// variable in undefined so its reference 
        /// is squiggled.
        /// </summary>
        Token,

        /// <summary>
        /// Whitespace after the provided token
        /// or end of file. Typical case when required
        /// token is missing such as missing close brace
        /// or a required operand.
        /// </summary>
        AfterToken
    }
}
