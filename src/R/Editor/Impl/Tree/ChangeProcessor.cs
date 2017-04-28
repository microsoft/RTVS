// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Tree {
    /// <summary>
    /// Class that handles processing of changes happened in the text buffer.
    /// </summary>
    internal sealed class TextChangeProcessor {
        /// <summary>
        /// Editor tree
        /// </summary>
        private readonly EditorTree _editorTree;

        /// <summary>
        /// Tree root node
        /// </summary>
        private readonly AstRoot _astRoot;

        /// <summary>
        /// A callback that provides a way to check if processing should be canceled.
        /// </summary>
        private readonly Func<bool> _cancelCallback;

        public TextChangeProcessor(EditorTree editorTree, AstRoot astRoot, Func<bool> cancelCallback = null) {
            _editorTree = editorTree;
            _astRoot = astRoot;
            _cancelCallback = cancelCallback;
        }

        private bool IsCancellationRequested() => _cancelCallback != null ? _cancelCallback() : false;

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
        public void ProcessChange(TextChange textChange, EditorTreeChangeCollection treeChanges) {
            IAstNode startNode = null, endNode = null;
            var startPositionType = PositionType.Undefined;
            var endPositionType = PositionType.Undefined;
            IAstNode commonParent = null;

            var start = textChange.OldRange.Start;
            var oldLength = textChange.OldRange.Length;
            var newLength = textChange.NewRange.Length;
            var offset = newLength - oldLength;

            var oldSnapshot = textChange.OldTextProvider;
            var newSnapshot = textChange.NewTextProvider;

            // Find position type and the enclosing element node. Note that element 
            // positions have been adjusted already (it happens immediately in OnTextChange) 
            // so we should be looking at the new range even that tree hasn't 
            // been fully updated yet. For example,if we delete a node, subsequent 
            // elements were already shifted up and damaged nodes have been removed 
            // so current node positions reflect text buffer state after the change.

            _astRoot.GetElementsEnclosingRange(start, newLength, out startNode,
                          out startPositionType, out endNode, out endPositionType);

            if (startNode is AstRoot) {
                commonParent = _astRoot;
            } else if (startNode == endNode) {
                if (startPositionType == PositionType.Token) {
                    // Change in comment or string content. 
                    commonParent = OnTokenNodeChange(startNode as TokenNode, start, oldLength, newLength);
                }
            } else {
                //if (commonParent == null)
                //{
                //    // Find parent that still has well formed curly braces.
                //    commonParent = FindWellFormedOuterScope(startNode);
                //}

                if (commonParent == null) {
                    commonParent = _astRoot;
                }
            }

            if (IsCancellationRequested()) {
                return;
            }

            if (!(commonParent is AstRoot)) {
                Debug.Assert(commonParent is IScope);
                var subTree = RParser.Parse(newSnapshot, commonParent, _editorTree.ExpressionTermFilter);
                return;
            }

            AstRoot newTree = RParser.Parse(newSnapshot, _editorTree.ExpressionTermFilter);
            treeChanges.Append(new EditorTreeChange_NewTree(newTree));
        }

        /// <summary>
        /// Reflects change inside string or comment by shrinking or expanding token node.
        /// </summary>
        /// <returns></returns>
        private IAstNode OnTokenNodeChange(TokenNode node, int start, int oldLength, int newLength) {
            Debug.Assert(node != null);
            node.Token.Expand(0, newLength - oldLength);
            return node;
        }

        /// <summary>
        /// Invokes full parse pass. Called from a background tree updating task.
        /// </summary>
        public void FullParse(EditorTreeChangeCollection changes, ITextProvider newSnapshot) {
            var newTree = RParser.Parse(newSnapshot, _editorTree.ExpressionTermFilter);
            changes.Append(new EditorTreeChange_NewTree(newTree));
        }
    }
}
