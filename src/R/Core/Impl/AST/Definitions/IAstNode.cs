// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Utility;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST {
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
    public interface IAstNode : ICompositeTextRange, IAstVisitorPattern, IPropertyOwner, IParseItem {
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

        /// <summary>
        /// Adds child node. Node is added in sorted order according to 
        /// its text range. Child node text ranges should not intersect.
        /// </summary>
        /// <param name="node"></param>
        void AppendChild(IAstNode node);

        /// <summary>
        /// Removes one or more child nodes
        /// </summary>
        void RemoveChildren(int start, int count);

        /// <summary>
        /// Finds deepest node that contains given position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Node or null if not found</returns>
        IAstNode NodeFromPosition(int position);

        /// <summary>
        /// Finds deepest element node that fully encloses given range
        /// </summary>
        IAstNode NodeFromRange(ITextRange range, bool inclusiveEnd = false);

        /// <summary>
        /// Determines position type and the enclosing node for 
        /// a given position in the document text.
        /// </summary>
        /// <param name="position">Position in the document text</param>
        /// <param name="node">Node that contains position</param>
        /// <returns>Position type</returns>
        PositionType GetPositionNode(int position, out IAstNode node);

        /// <summary>
        /// Finds two nodes that surround given text range
        /// </summary>
        /// <param name="start">Range start</param>
        /// <param name="length">Range length</param>
        /// <param name="startNode">Node that precedes the range or null if there is none</param>
        /// <param name="startPositionType">Type of position in the start node</param>
        /// <param name="endNode">Node that follows the range or null if there is none</param>
        /// <param name="endPositionType">Type of position in the end node</param>
        /// <returns>Node that encloses the range or root node</returns>
        IAstNode GetElementsEnclosingRange(
                                int start, int length,
                                out IAstNode startNode, out PositionType startPositionType,
                                out IAstNode endNode, out PositionType endPositionType);
    }
}
