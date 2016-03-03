// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Parser {
    /// <summary>
    /// Represents parsing error when expected item is missing.
    /// </summary>
    [DebuggerDisplay("[ErrorType]")]
    public class MissingItemParseError : ParseError {
        public MissingItemParseError(ParseErrorType errorType, RToken token) :
            base(errorType, ErrorLocation.AfterToken, token) {
        }
    }
}
