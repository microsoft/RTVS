// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Core.Parser {
    /// <summary>
    /// Represents parsing (syntax) error. Parsing error is 
    /// a text range so it can be used by the IDE to squiggle 
    /// the problematic range in the editor.
    /// </summary>
    [DebuggerDisplay("[ErrorType]")]
    public class ParseError : TextRange, IParseError {
        #region IParseError
        /// <summary>
        /// Error type. Actual localized message 
        /// text is provided by the IDE.
        /// </summary>
        public ParseErrorType ErrorType { get; private set; }

        /// <summary>
        /// Location of the parsing error.
        /// Gives hint to IDE what to squiggle.
        /// </summary>
        public ErrorLocation Location { get; private set; }
        #endregion

        public ParseError(ParseErrorType errorType, ErrorLocation location, ITextRange range) :
            base(range) {
            this.ErrorType = errorType;
            this.Location = location;
        }
    }
}
