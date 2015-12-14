using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.TestApp {
    public partial class TreeWindow : Window {

        private ObservableTreeNode _rootNode;
        private TreeNodeCollection _nodeCollection;

        public TreeWindow() {
            InitializeComponent();

            SetRootNode();
        }

        private void SetRootNode() {
            _rootNode = new ObservableTreeNode(new TestNode(null, "0"));
            _rootNode.IsExpanded = true;
            _nodeCollection = new TreeNodeCollection(_rootNode);
            RootTreeGrid.ItemsSource = _nodeCollection.ItemList;
        }
    }

    internal class TestData {
        public TestData(TestData parent, string id) {
            Id = id;
            Parent = parent;
            Path = parent?.Path + ":" + id;
        }

        public string Id { get; }

        public string Path { get; }

        private TestData Parent { get; }
    }

    internal class TestNode : ITreeNode {
        public TestNode(TestNode parent, string id) {
            Content = new TestData((TestData)parent?.Content, id);
        }

        #region ITreeNode support

        public object Content { get; set; }

        public bool HasChildren {
            get {
                return true;
            }
        }

        public bool CanUpdateTo(ITreeNode node) {
            var testNode = node as TestNode;
            if (testNode != null) {
                return ((TestData)Content).Path == ((TestData)testNode.Content).Path;
            }
            return false;
        }

        public Task<IReadOnlyList<ITreeNode>> GetChildrenAsync(CancellationToken cancellationToken) {
            return Task.Run(() => {
                List<ITreeNode> children = new List<ITreeNode>();
                for (int i = 0; i < 3; i++) {
                    children.Add(new TestNode(this, i.ToString()));
                }
                return (IReadOnlyList<ITreeNode>) children;
            });
        }

        #endregion
    }
}
