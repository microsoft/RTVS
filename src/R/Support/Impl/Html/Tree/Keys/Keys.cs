// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Html.Core.Tree.Nodes;

namespace Microsoft.Html.Core.Tree.Keys {
    /// <summary>
    /// Collection of HTML element keys. Tracks if element is still in the tree
    /// and provides access to an element given the element key.
    /// </summary>
    public sealed class ElementKeys {
        /// <summary>
        /// Map or element keys to elements. Allows easy check that element is still
        /// in the tree and also an easy way to find an element by its key.
        /// Useful when accessing elements from long running threads when
        /// elements can get deleted by the time thread gets to it.
        /// </summary>
        private ConcurrentDictionary<int, ElementNode> _elementKeys = new ConcurrentDictionary<int, ElementNode>();
        private HtmlTree _tree;

        public ElementKeys(HtmlTree tree) {
            _tree = tree;
        }

        /// <summary>
        /// Retrieves element by its key. Must only be called if caller has tree read lock.
        /// Element node may become invalid during next tree update unless caller is holding 
        /// a read lock or caller is on the application main thread.
        /// </summary>
        public ElementNode GetElement(int key) {
            if (key == 0)
                return _tree.RootNode;

            ElementNode node = null;
            _elementKeys.TryGetValue(key, out node);

            return node;
        }

        /// <summary>
        /// Retrieves element by its key. Must only be called if caller has tree read lock.
        /// Element node may become invalid during next tree update unless caller is holding 
        /// a read lock or caller is on the application main thread.
        /// </summary>
        public ElementNode this[int key] {
            get { return GetElement(key); }
        }

        /// <summary>
        /// Recreates key collection for the entire tree
        /// </summary>
        internal void Rebuild() {
            _elementKeys.Clear();

            foreach (var node in _tree.RootNode.Children) {
                AddElement(node);
            }
        }

        /// <summary>
        /// Adds element and its children to the collection
        /// </summary>
        internal void AddElement(ElementNode node) {
            _elementKeys.TryAdd(node.Key, node);

            foreach (var child in node.Children) {
                AddElement(child);
            }
        }

        /// <summary>
        /// Removes element and its children from the collection
        /// </summary>
        internal void RemoveElement(ElementNode node) {
            ElementNode n;
            _elementKeys.TryRemove(node.Key, out n);

            foreach (var child in node.Children)
                RemoveElement(child);
        }

        /// <summary>
        /// Recreates keys for a subtree starting at a given element
        /// </summary>
        /// <param name="node"></param>
        internal void Rebuild(ElementNode node) {
            _elementKeys[node.Key] = node;

            foreach (var child in node.Children)
                Rebuild(child);
        }
    }
}
