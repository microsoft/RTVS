// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Core.Parser {
    /// <summary>
    /// Implemented by objects representing parsing (syntax) errors.
    /// Parsing error is a text range so it can be used by the IDE 
    /// to squiggle the problematic text range in the editor.
    /// </summary>
    public interface IParseError : ITextRange {
        /// <summary>
        /// Error type. Actual localized message 
        /// text is provided by the IDE.
        /// </summary>
        ParseErrorType ErrorType { get; }

        /// <summary>
        /// Location of the parsing error.
        /// Gives hint to IDE what to squiggle.
        /// </summary>
        ErrorLocation Location { get; }
    }
}
