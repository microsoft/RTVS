using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Utility;
using Microsoft.R.Core.Parser.Definitions;

namespace Microsoft.R.Core.AST.Definitions
{
    /// <summary>
    /// Implemented by all AST nodes, Nodes that can have child nodes 
    /// such as expressions, loops, assignments and other statements
    /// also acts as composite text ranges that encompass all tokens
    /// that constitute the node. Supports visitor pattern that allows
    /// traversal of the subtree starting at this node. It is also
    /// a property owner and allows attaching arbitrary properties.
    /// Leaf nodes always have zero children and do not permit addition
    /// of child nodes.
    /// </summary>
    public interface IAstNode : ICompositeTextRange, IAstVisitorPattern, IPropertyOwner, IParseItem
    {
        /// <summary>
        /// AST root node
        /// </summary>
        AstRoot Root { get; }

        /// <summary>
        /// This node's parent
        /// </summary>
        IAstNode Parent { get; set; }

        /// <summary>
        /// Node children
        /// </summary>
        IReadOnlyTextRangeCollection<IAstNode> Children { get; }

        void AppendChild(IAstNode node);
    }
}
