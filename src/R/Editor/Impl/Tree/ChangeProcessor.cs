using System;
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
        /// Changes are to be sent to the main thread and applied from there.
        /// Caller is responsible for the tree read lock acquisition. 
        /// </summary>
        /// <param name="start">Start position of the change</param>
        /// <param name="oldLength">Length of the original text (0 if insertion)</param>
        /// <param name="newLength">Length of the new text (0 if deletion)</param>
        /// <param name="oldSnapshot">Text snapshot before the change</param>
        /// <param name="newSnapshot">Text snapshot after the change</param>
        /// <param name="treeChanges">Collection of tree changes to apply 
        /// from the main thread</param>
        public void ProcessChange(TextChange textChange, EditorTreeChangeCollection treeChanges)
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

            // Find position type and the enclosing element node. Note that element 
            // positions have been adjusted already (it happens immediately in OnTextChange) 
            // so we should be looking at the new range even that tree hasn't 
            // been fully updated yet. For example,if we delete a node, subsequent 
            // elements were already shifted up and damaged nodes have been removed 
            // so current node positions reflect text buffer state after the change.

            _astRoot.GetElementsEnclosingRange(start, newLength, out startNode,
                          out startPositionType, out endNode, out endPositionType);

            if (startNode is AstRoot)
            {
                commonParent = _astRoot;
            }
            else if (startNode == endNode)
            {
                if (startPositionType == PositionType.Token)
                {
                    // Change in comment or string content. 
                    commonParent = OnTokenNodeChange(startNode as TokenNode, start, oldLength, newLength);
                }
            }
            else
            {
                if (commonParent == null)
                {
                    // Find parent that still has well formed curly braces.
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
                Debug.Assert(commonParent is IScope);

                // Partial parse and update case
                if (_tracePartialParse.Enabled)
                {
                    Debug.WriteLine("R editor parser: parsing {0}, {1}:{2}",
                        commonParent.ToString(), commonParent.Start, commonParent.End);
                }

                AstRoot subTree = RParser.Parse(newSnapshot, commonParent);
                return;
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

        /// <summary>
        /// Invokes full parse pass. Called from a background tree updating task.
        /// </summary>
        public void FullParse(EditorTreeChangeCollection changes, ITextProvider newSnapshot)
        {
            AstRoot newTree = RParser.Parse(newSnapshot);
            changes.ChangeQueue.Enqueue(new EditorTreeChange_NewTree(newTree));
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
