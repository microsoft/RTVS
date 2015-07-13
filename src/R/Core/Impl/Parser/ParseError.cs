using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser.Definitions;
using Microsoft.R.Core.Tokens;

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

        /// <summary>
        /// Node the error applies to. Some errors may not have
        /// AST node associated with them since due to errors
        /// the node was not actually created.
        /// </summary>
        public IAstNode Node { get; private set; }

        /// <summary>
        /// Token the error applies to. Some errors may not have
        /// AST node associated with them since due to errors
        /// the node was not actually created.
        /// </summary>
        public RToken Token { get; private set; }
        #endregion

        public ParseError(ParseErrorType errorType, ParseErrorLocation location, IAstNode node):
            base(node)
        {
            this.Node = node;
            this.ErrorType = errorType;
            this.Location = location;
        }

        public ParseError(ParseErrorType errorType, ParseErrorLocation location, RToken token) :
            base(token)
        {
            this.Token = token;
            this.ErrorType = errorType;
            this.Location = location;
        }
    }
}
