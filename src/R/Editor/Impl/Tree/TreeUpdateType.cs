
namespace Microsoft.R.Editor.Tree
{
    /// <summary>
    /// Describes changes in the AST
    /// </summary>
    public enum TreeUpdateType
    {
        PositionsOnly,
        NodesRemoved,
        ScopeChanged,
        NewTree
    }
}
