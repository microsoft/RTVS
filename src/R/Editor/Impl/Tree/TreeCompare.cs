using System;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Editor.Tree
{
    internal static class TreeCompare
    {
        /// <summary>
        /// Compares two nodes in the tree and calculates which child elements
        /// are present in both nodes and which have been changed. Fills two arrays:
        /// 'children of old node that exist under new node' and 'children of new
        /// node that existed in old node'. Array contains indexes into collection
        /// of child elements. For example, if third child node of a new element
        /// existed in the old node at position 5, then newChildrenExistsInOld[3] 
        /// will be set to 5.
        /// </summary>
        /// <param name="oldNode">Element in earlier tree state (typically before reparse)</param>
        /// <param name="newNode">Element in the new tree</param>
        /// <param name="isCanceled">Callback to determine if comparison should stop. 
        /// Typically used when code is invoked from a background thread.</param>
        /// <param name="newChildrenExistsInOld">Array of child states. Initialized to -1</param>
        /// <param name="oldChildrenExistsInNew">Array of child states. Initialized to -1</param>
        public static void CompareNodes
            (
            IAstNode oldNode,
            IAstNode newNode,
            Func<bool> isCanceled,
            int[] newChildrenExistsInOld,
            int[] oldChildrenExistsInNew
            )
        {
            //if (oldNode.ChildrenInvalidated)
            //    return;

            for (int i = 0; i < oldNode.Children.Count; i++)
            {
                if (oldChildrenExistsInNew[i] < 0)
                {
                    IAstNode existingChild = oldNode.Children[i];

                    for (int j = 0; j < newNode.Children.Count; j++)
                    {
                        IAstNode newChild = newNode.Children[j];

                        if (newChildrenExistsInOld[j] < 0)
                        {
                            if (isCanceled != null && isCanceled())
                                break;

                            if (CompareElements(existingChild, newChild))
                            {
                                oldChildrenExistsInNew[i] = j;
                                newChildrenExistsInOld[j] = i;
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compares two element nodes for equality. Element nodes are considered 
        /// to be equal if the have same number of children, their text ranges
        /// are the same. Method recurses into child nodes.
        /// </summary>
        /// <param name="node1">First node</param>
        /// <param name="node2">Second node</param>
        /// <returns>True if nodes are equal</returns>
        public static bool CompareElements(IAstNode node1, IAstNode node2)
        {
            if (node1.IsDirty || node2.IsDirty)
                return false;

            if (node1.Children.Count != node2.Children.Count)
                return false;

            if (!TextRange.AreEqual(node1, node2))
                return false;

            if (node1.GetType() != node2.GetType())
                return false;

            // OK, now time to dive into children...
            for (int i = 0; i < node1.Children.Count; i++)
            {
                if (!CompareElements(node1.Children[i], node2.Children[i]))
                    return false;
            }

            return true;
        }
    }
}
