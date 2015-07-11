using System.Collections.Concurrent;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST.Keys
{
    /// <summary>
    /// Collection of node keys. Tracks if element is still in the tree
    /// and provides access to an element given the element key.
    /// </summary>
    public sealed class NodeKeys
    {
        /// <summary>
        /// Map or element keys to elements. Allows easy check that element is still
        /// in the tree and also an easy way to find an element by its key.
        /// Useful when accessing elements from long running threads when
        /// elements can get deleted by the time thread gets to it.
        /// </summary>
        private ConcurrentDictionary<int, IAstNode> _keys = new ConcurrentDictionary<int, IAstNode>();
        private AstRoot _ast;

        public NodeKeys(AstRoot ast)
        {
            _ast = ast;
        }

        /// <summary>
        /// Retrieves element by its key. Must only be called if caller has tree read lock.
        /// Element node may become invalid during next tree update unless caller is holding 
        /// a read lock or caller is on the application main thread.
        /// </summary>
        public IAstNode GetElement(int key)
        {
            if (key == 0)
                return _ast;

            IAstNode node = null;
            _keys.TryGetValue(key, out node);

            return node;
        }

        /// <summary>
        /// Retrieves element by its key. Must only be called if caller has tree read lock.
        /// Element node may become invalid during next tree update unless caller is holding 
        /// a read lock or caller is on the application main thread.
        /// </summary>
        public IAstNode this[int key]
        {
            get { return GetElement(key); }
        }

        /// <summary>
        /// Recreates key collection for the entire tree
        /// </summary>
        internal void Rebuild()
        {
            _keys.Clear();

            foreach (var node in _ast.Children)
            {
                AddNode(node);
            }
        }

        /// <summary>
        /// Adds element and its children to the collection
        /// </summary>
        internal void AddNode(IAstNode node)
        {
            _keys.TryAdd(node.Key, node);

            foreach (var child in node.Children)
            {
                AddNode(child);
            }
        }

        /// <summary>
        /// Removes element and its children from the collection
        /// </summary>
        internal void RemoveElement(IAstNode node)
        {
            IAstNode n;
            _keys.TryRemove(node.Key, out n);

            foreach (var child in node.Children)
                RemoveElement(child);
        }

        /// <summary>
        /// Recreates keys for a subtree starting at a given element
        /// </summary>
        /// <param name="node"></param>
        internal void Rebuild(IAstNode node)
        {
            _keys[node.Key] = node;

            foreach (IAstNode child in node.Children)
            {
                Rebuild(child);
            }
        }
    }
}
