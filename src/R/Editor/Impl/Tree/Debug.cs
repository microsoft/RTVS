// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.AST;

namespace Microsoft.R.Editor.Tree {
    internal class DebugTree {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "editorTree")]
        public static void VerifyTree(EditorTree editorTree) {
#if ___DEBUG
            if (editorTree.TextBuffer != null)
            {
                var fullParseTree = new HtmlTree(new TextProvider(editorTree.TextSnapshot));
                fullParseTree.Build(rebuildCollection);

                CompareTrees(editorTree, fullParseTree);
            }
#endif
        }

        public static void CompareTrees(EditorTree editorTree, AstRoot tree2) {
#if ___DEBUG
            var cc1 = editorTree.ParseTree.CommentCollection;
            var cc2 = tree2.CommentCollection;

            Debug.Assert(cc1.Count == cc2.Count);
            for (int i = 0; i < cc1.Count; i++)
            {
                Debug.Assert(cc1[i].Start == cc2[i].Start);
                Debug.Assert(cc1[i].Length == cc2[i].Length);
            }

            CompareNodes(editorTree, editorTree.ParseTree.RootNode, tree2.RootNode);
#endif
        }

        public static void CompareNodes(EditorTree editorTree, IAstNode node1, IAstNode node2) {
#if ___DEBUG
            Debug.Assert(node1 is RootNode || editorTree.ParseTree.ContainsElement(node1.Key));

            if (!node1.ChildrenInvalidated)
                Debug.Assert(node1.Children.Count == node2.Children.Count);

            Debug.Assert(TextRange.AreEqual(node1.NameRange, node2.NameRange));
            Debug.Assert(node1.Attributes.Count == node2.Attributes.Count);

            Debug.Assert(TextRange.AreEqual(node1.OuterRange, node2.OuterRange));
            Debug.Assert(TextRange.AreEqual(node1.InnerRange, node2.InnerRange));

            Debug.Assert(node1.Start == node2.Start);
            Debug.Assert(node1.End == node2.End);

            if (!node1.ChildrenInvalidated)
            {
                if (node1.Children.Count == node2.Children.Count)
                {
                    for (int i = 0; i < node1.Children.Count; i++)
                    {
                        CompareNodes(editorTree, node1.Children[i], node2.Children[i]);
                    }
                }
            }
#endif
        }
    }
}
