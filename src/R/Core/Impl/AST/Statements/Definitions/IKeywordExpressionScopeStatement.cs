namespace Microsoft.R.Core.AST.Statements.Definitions
{
    /// <summary>
    /// Represents statement that is based on a keyword
    /// and has conditional or enumerable expression such as 
    /// while(...) or for(...) followed by { } scope.
    /// </summary>
    public interface IKeywordExpressionScopeStatement: IKeywordExpressionStatement, IKeywordScopeStatement
    {
    }
}
