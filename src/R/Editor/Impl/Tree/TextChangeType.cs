
namespace Microsoft.R.Editor.Tree
{
    /// <summary>
    /// Type of text change in the editor document
    /// </summary>
    public enum TextChangeType
    {
        /// <summary>
        /// Trivial change like whitespace change
        /// </summary>
        Trivial,

        /// <summary>
        /// Change inside comment
        /// </summary>
        Comment,

        /// <summary>
        /// Change inside expandable token node
        /// such as inside a string or a comment
        /// </summary>
        Token,

        /// <summary>
        /// Structure changed such as change in curly braces
        /// </summary>
        Structure,
    }
}
