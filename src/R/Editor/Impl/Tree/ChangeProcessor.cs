using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Tree
{
    /// <summary>
    /// Class that handles processing of changes happened in the text buffer.
    /// </summary>
    internal sealed class TextChangeProcessor
    {
        private static BooleanSwitch _tracePartialParse =
            new BooleanSwitch("tracePartialParse", "Trace R partial parse events in debug window.");

        /// <summary>
        /// Editor tree
        /// </summary>
        private EditorTree _editorTree;

        /// <summary>
        /// Tree root node
        /// </summary>
        private AstRoot _astRoot;

        /// <summary>
        /// A callback that provides a way to check if processing should be canceled.
        /// </summary>
        private Func<bool> _cancelCallback;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rootNode">HTML tree root node</param>
        /// <param name="cancelCallback">A callback interface that provides a way to check if processing should be canceled.</param>
        public TextChangeProcessor(EditorTree editorTree, AstRoot astRoot, Func<bool> cancelCallback = null)
        {
#if DEBUG
            _tracePartialParse.Enabled = false;
#endif

            _editorTree = editorTree;
            _astRoot = astRoot;
            _cancelCallback = cancelCallback;
        }

        private bool IsCancellationRequested()
        {
            return _cancelCallback != null ? _cancelCallback() : false;
        }

        /// <summary>
        /// Processes a single text change incrementally. Enqueues resulting 
        /// tree changes in the supplied queue. Does not modify the tree. 
        /// If used in a multi-threaded environment, caller is responsible for
        /// read lock acquisition.
        /// </summary>
        /// <param name="start">Start position of the change</param>
        /// <param name="oldLength">Length of the original text (0 if insertion)</param>
        /// <param name="newLength">Length of the new text (0 if deletion)</param>
        /// <param name="oldSnapshot">Text snapshot before the change</param>
        /// <param name="newSnapshot">Text snapshot after the change</param>
        /// <param name="treeChanges">Queue of changes</param>
        public void ProcessChange(TextChange textChange, EditorTreeChanges treeChanges)
        {
            IAstNode startNode = null, endNode = null;
            PositionType startPositionType = PositionType.Undefined;
            PositionType endPositionType = PositionType.Undefined;
            IAstNode commonParent = null;

            int start = textChange.OldRange.Start;
            int oldLength = textChange.OldRange.Length;
            int newLength = textChange.NewRange.Length;
            int offset = newLength - oldLength;

            ITextProvider oldSnapshot = textChange.OldTextProvider;
            ITextProvider newSnapshot = textChange.NewTextProvider;

            // Find position type and the enclosing element node. Note that element positions had been adjusted already 
            // (it happened immediately in OnTextChange) so we should be looking at a new range even that tree hasn't 
            // been fully updated yet. For example,if we delete an element, subsequent elements were already shifted up 
            // and damaged element had been removed so element positions reflect text buffer state after the change.

            _astRoot.GetElementsEnclosingRange(start, newLength, out startNode, out startPositionType, out endNode, out endPositionType);

            if (startNode is AstRoot)
            {
                commonParent = _astRoot;
            }
            else if (startNode == endNode)
            {
                if (startPositionType == PositionType.Token)
                {
                    // Change in identifier name. 
                    commonParent = OnTokenNodeChange(startNode as TokenNode, start, oldLength, newLength);
                }
            }
            else
            {
                if (commonParent == null)
                {
                    // We need to find parent that still has well formed { and }
                    commonParent = FindWellFormedOuterScope(startNode);
                }

                if (commonParent == null)
                {
                    commonParent = _astRoot;
                }
            }

            if (IsCancellationRequested())
                return;

            if (!(commonParent is AstRoot))
            {
                // Partial parse case
                if (_tracePartialParse.Enabled)
                {
                    Debug.WriteLine("R editor parser: parsing {0}, {1}:{2}", commonParent.ToString(), commonParent.Start, commonParent.End);
                }

                ITextRange reparseRange = commonParent;
                AstRoot subTree = RParser.Parse(newSnapshot, reparseRange);

                if (subTree.Errors.Count == 0)
                {

                    if (IsCancellationRequested())
                        return;

                    CompareAndUpdate(commonParent, subTree, treeChanges, IsCancellationRequested);
                    return;
                }
            }

            if (_tracePartialParse.Enabled)
                Debug.WriteLine("R parser: full parse\r\n");

            AstRoot newTree = RParser.Parse(newSnapshot);
            treeChanges.ChangeQueue.Enqueue(new EditorTreeChange_NewTree(newTree));
        }

        /// <summary>
        /// Reflects change inside string or comment by shrinking or expanding token node.
        /// </summary>
        /// <returns></returns>
        private IAstNode OnTokenNodeChange(TokenNode node, int start, int oldLength, int newLength)
        {
            Debug.Assert(node != null);
            node.Token.Expand(0, newLength - oldLength);

            return node;
        }


        internal static void CompareAndUpdate(IAstNode oldNode, IAstNode newNode, EditorTreeChanges changes, Func<bool> isCanceled = null)
        {
            // When we compare elements we compare strings. String comparison for 300K file is about 3ms. 
            // We could calculate and store hashes instead, but hash calculation is about 10ms for 300K 
            // file so we'll stick with strings although they do require a bit more memory. 

            var removedElements = new List<IAstNode>(); // Elements removed from the tree
            var addedElements = new List<IAstNode>(); // New elements

            int[] newChildrenExistsInOld = new int[newNode.Children.Count];
            for (int i = 0; i < newChildrenExistsInOld.Length; i++)
            {
                newChildrenExistsInOld[i] = -1;
            }

            int[] oldChildrenExistsInNew = new int[oldNode.Children.Count];
            for (int i = 0; i < oldChildrenExistsInNew.Length; i++)
            {
                oldChildrenExistsInNew[i] = -1;
            }

            TreeCompare.CompareNodes(oldNode, newNode, isCanceled, newChildrenExistsInOld, oldChildrenExistsInNew);

            for (int i = 0; i < oldChildrenExistsInNew.Length; i++)
            {
                if (oldChildrenExistsInNew[i] < 0)
                {
                    removedElements.Add(oldNode.Children[i]);

                    if (_tracePartialParse.Enabled)
                    {
                        string s = oldNode.Children[i].GetType().ToString();
                        s = s.Substring(s.LastIndexOf('.') + 1);
                        Debug.WriteLine("Removed: {0} - {1}", s, oldNode.Children[i].Key);
                    }
                }
            }

            // Set parent of new children to the existing node
            foreach (IAstNode node in newNode.Children)
            {
                node.Parent = oldNode;
            }

            for (int i = 0; i < newChildrenExistsInOld.Length; i++)
            {
                var node = newNode.Children[i];

                if (newChildrenExistsInOld[i] < 0)
                {
                    Debug.Assert(node.Parent != null);
                    Debug.Assert(node.Children != null);

                    addedElements.Add(node);

                    if (_tracePartialParse.Enabled)
                    {
                        string s = node.Children[i].GetType().ToString();
                        s = s.Substring(s.LastIndexOf('.') + 1);
                        Debug.WriteLine("Added: {0} - {1}", s, node.Key);
                    }
                }
                else
                {
                    // existing element
                    int oldIndex = newChildrenExistsInOld[i];
                    TransferKeys(newNode.Children[i], oldNode.Children[oldIndex]);
                }
            }

            bool elementContentChanged = addedElements.Count > 0 || removedElements.Count > 0;

            if (elementContentChanged)
            {
                // Transfer children to the existing node
                List<IAstNode> newChildren = new List<IAstNode>(newNode.Children);
                newNode.RemoveChildren(0, newNode.Children.Count);

                changes.ChangeQueue.Enqueue(new EditorTreeChange_NodesChanged(oldNode.Key, newChildren, addedElements, removedElements));
            }

            // Now add new comments to the existing collection
            oldNode.Root.Comments.Merge(newNode.Root.Comments);
        }

        private static void TransferKeys(IAstNode dst, IAstNode src)
        {
            dst.Key = src.Key;

            for (int i = 0; i < src.Children.Count; i++)
            {
                TransferKeys(dst.Children[i], src.Children[i]);
            }
        }

        /// <summary>
        /// Invokes full parse pass. Called from a background tree updating task.
        /// </summary>
        /// <param name="incrementalUpdate">
        /// If true, parser will attempt to calculate differences between old and new trees 
        /// and fire 'elements added' and 'elements removed' events. If false, only 
        /// 'on new tree' event will fire.
        /// </param>
        public void FullParse(EditorTreeChanges changes, ITextProvider newSnapshot, bool incrementalUpdate)
        {
            AstRoot newTree = RParser.Parse(newSnapshot);

            if (incrementalUpdate)
            {
                CompareAndUpdate(_astRoot, newTree, changes);
                _editorTree.AstRoot.Comments = newTree.Comments;
            }
            else
            {
                changes.ChangeQueue.Enqueue(new EditorTreeChange_NewTree(newTree));
            }
        }

        /// <summary>
        /// Finds well formed ancestor scope. Well formed means element has
        /// both open { and closing }.
        /// </summary>
        /// <param name="node">Node to start search with</param>
        /// <returns>Well formed ancestor scope or root node</returns>
        private static IAstNode FindWellFormedOuterScope(IAstNode node)
        {
            var parent = node.Parent;

            while (true)
            {
                if (parent is AstRoot)
                    break;

                IScope scope = parent as IScope;
                if (scope != null && scope.OpenCurlyBrace != null && scope.CloseCurlyBrace != null)
                {
                    break;
                }

                parent = parent.Parent;
            }

            return parent;
        }
    }
}
