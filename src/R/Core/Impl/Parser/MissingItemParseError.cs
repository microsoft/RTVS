using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Parser
{
    /// <summary>
    /// Represents parsing error when expected item is missing.
    /// </summary>
    [DebuggerDisplay("[ErrorType]")]
    public class MissingItemParseError : ParseError
    {
        public MissingItemParseError(ParseErrorType errorType, RToken token) :
            base(errorType, ParseErrorLocation.AfterToken, token)
        {
        }
    }
}
