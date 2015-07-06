using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.Parser.Definitions
{
    /// <summary>
    /// Represents an item that can be parsed. Used in recursive
    /// R language parser to construct syntax tree. All items
    /// in the AST implement this interface.
    /// </summary>
    public interface IParseItem
    {
        /// <summary>
        /// Parses the item.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>True if parsing is successfull, false otherwise</returns>
        bool Parse(ParseContext context, IAstNode parent = null);
    }
}
