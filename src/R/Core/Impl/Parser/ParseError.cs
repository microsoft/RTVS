using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser.Definitions;

namespace Microsoft.R.Core.Parser
{
    /// <summary>
    /// Represents parsing (syntax) error. Parsing error is 
    /// a text range so it can be used by the IDE to squiggle 
    /// the problematic range in the editor.
    /// </summary>
    [DebuggerDisplay("[ErrorType]")]
    public class ParseError : TextRange, IParseError
    {
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
        public ParseErrorLocation Location { get; private set; }
        #endregion

        public ParseError(ParseErrorType errorType, ParseErrorLocation location, ITextRange range) :
            this(errorType, location, range.Start, range.Length)
        {
        }

        public ParseError(ParseErrorType errorType, ParseErrorLocation location, int start, int length) :
            base(start, length)
        {
            ErrorType = errorType;
            Location = location;
        }
    }
}
