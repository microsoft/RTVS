using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Core.Parser.Definitions
{
    /// <summary>
    /// Implemented by objects representing parsing (syntax) errors.
    /// Parsing error is a text range so it can be used by the IDE 
    /// to squiggle the problematic text range in the editor.
    /// </summary>
    public interface IParseError : ITextRange
    {
        /// <summary>
        /// Error type. Actual localized message 
        /// text is provided by the IDE.
        /// </summary>
        ParseErrorType ErrorType { get; }

        /// <summary>
        /// Location of the parsing error.
        /// Gives hint to IDE what to squiggle.
        /// </summary>
        ParseErrorLocation Location { get; }
    }
}
