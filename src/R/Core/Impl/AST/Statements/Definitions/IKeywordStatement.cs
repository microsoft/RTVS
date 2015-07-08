namespace Microsoft.R.Core.AST.Statements.Definitions
{
    /// <summary>
    /// Represents statement that is based on a keyword
    /// such as while or repeat.
    /// </summary>
    public interface IKeywordStatement: IStatement
    {
        /// <summary>
        /// Statement keyword node
        /// </summary>
        TokenNode Keyword { get; }

        /// <summary>
        /// Keyword text
        /// </summary>
        string Text { get; }
    }
}
