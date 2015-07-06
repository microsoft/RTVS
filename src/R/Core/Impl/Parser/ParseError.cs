using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser.Definitions;

namespace Microsoft.R.Core.Parser
{
    /// <summary>
    /// Represents parsing (syntax) error. Parsing error is 
    /// a text range so it can be used by the IDE to squiggle 
    /// the problematic range in the editor.
    /// </summary>
    public class ParseError : TextRange, IParseError
    {
        /// <summary>
        /// Error type. Actual localized message 
        /// text is provided by the IDE.
        /// </summary>
        public ParseErrorType ErrorType { get; private set; }

        public ParseError(ParseErrorType errorType, ITextRange range) :
            base(range)
        {
            this.ErrorType = errorType;
        }

        public ParseError(ParseErrorType errorType, int start, int length) :
            base(start, length)
        {
            this.ErrorType = errorType;
        }
    }
}
