using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    /// <summary>
    /// ITreeNode for unittest
    /// </summary>
    class TestNode : ITreeNode {
        private readonly int _childCount;
        private readonly string _throwAt;

        /// <summary>
        /// create new instance of <see cref="TestNode"/>
        /// </summary>
        /// <param name="content">content of the node</param>
        /// <param name="childCount">number children of the node, and every descendants (bottomless, if nonzero)</param>
        /// <param name="throwAt">throws if content is same as this</param>
        public TestNode(string content, int childCount = 0, string throwAt = null) {
            Content = content;
            _childCount = childCount;
            _throwAt = throwAt;

            if (content == throwAt) {
                throw new Exception(string.Format("Test Exception:{0}", throwAt));
            }
        }

        public TestNode(int content)
            : this(content.ToString()) { }

        public object Content { get; set; }

        public Task<IReadOnlyList<ITreeNode>> GetChildrenAsync(CancellationToken cancellationToken) {
            return Task.Run(() => CreateChilds());
        }

        public bool CanUpdateTo(ITreeNode node) {
            return ((string)Content) == ((string)node.Content);
        }

        public bool HasChildren {
            get {
                return _childCount != 0;
            }
        }

        private IReadOnlyList<ITreeNode> CreateChilds() {
            var children = new List<ITreeNode>();

            for (int i = 0; i < _childCount; i++) {
                children.Add(
                    new TestNode(
                        string.Format("{0}.{1}", Content, i.ToString()),
                        _childCount,
                        _throwAt));
            }

            return children;
        }
    }
}
