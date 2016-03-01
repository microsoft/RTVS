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
