using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Core.Parser
{
    /// <summary>
    /// Represents parsing error when expected item is missing.
    /// </summary>
    [DebuggerDisplay("[ErrorType]")]
    public class MissingItemParseError : ParseError
    {
        public MissingItemParseError(ParseErrorType errorType, ITextRange range) :
            base(errorType, ParseErrorLocation.AfterToken, range)
        {
        }
    }
}
