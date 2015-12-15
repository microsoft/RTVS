using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ObservableTreeNodeTest {
        private ObservableTreeNode _rootNode;
        private TreeNodeCollection _linearized;
        private Comparer<ITreeNode> _testNodeComparer;

        [TestInitialize]
        public void InitializeTest() {
            _testNodeComparer = Comparer<ITreeNode>.Create(TestNode.Comparison);
            _rootNode = new ObservableTreeNode(new TestNode(0), _testNodeComparer);
            _linearized = new TreeNodeCollection(_rootNode);
        }


        [TestCleanup]
        public void CleanupTest() {
            _linearized = null;
            _rootNode = null;
        }

        #region manual Add/Removal Test

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void ObservableTreeNodeConstructorTest() {
            var target = new ObservableTreeNode(new TestNode(1234), _testNodeComparer);
            Assert.AreEqual(false, target.HasChildren, "Default HasChildren value");
            Assert.AreEqual(1234.ToString(), target.Model.Content);
            Assert.AreEqual(0, target.Children.Count);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void ObservableTreeNodeAddChildTest() {
            int[] expected;
            var target = _rootNode;

            // full tree: { 0, 1, 11, 111, 112, 12, 121, 122, 13, 131, 1311, 1312, 132 }
            target.AddChild(GetTestTree());

            // only root expanded by default
            expected = new int[] { 1 };
            AssertLinearized(expected, _linearized.ItemList, target);

            // expand 1st at level 0
            Expand(0, 3);
            expected = new int[] { 1, 11, 12, 13 };
            AssertLinearized(expected, _linearized.ItemList, target);

            // expand 3rd at level 1
            Expand(3, 2);
            expected = new int[] { 1, 11, 12, 13, 131, 132 };
            AssertLinearized(expected, _linearized.ItemList, target);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void ObservableTreeNodeRemoveChildTest() {
            var target = _rootNode;
            target.AddChild(GetTestTree());
            Expand(0, 3);
            Expand(3, 2);
            Expand(4, 2);
            target.Children[0].Children[2].RemoveChild(0);

            int[] expected = { 1, 11, 12, 13, 132 };
            AssertLinearized(expected, _linearized.ItemList, target);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void AddChildOutOrderTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(10), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(11), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(12), _testNodeComparer));

            int[] expected = { 10, 11, 12 };
            AssertLinearized(expected, _linearized.ItemList, target);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void AddChildInOrderTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(10), _testNodeComparer));

            var added = new ObservableTreeNode(new TestNode(11), _testNodeComparer);
            added.IsExpanded = true;
            added.AddChild(new ObservableTreeNode(new TestNode(111), _testNodeComparer));
            added.AddChild(new ObservableTreeNode(new TestNode(112), _testNodeComparer));

            target.AddChild(added);
            target.AddChild(new ObservableTreeNode(new TestNode(12), _testNodeComparer));

            int[] expected = { 10, 11, 111, 112, 12 };

            AssertLinearized(expected, _linearized.ItemList, target);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void AddTreeTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(12), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(10), _testNodeComparer));

            var tree = new ObservableTreeNode(new TestNode(11), _testNodeComparer);
            tree.IsExpanded = true;
            tree.AddChild(new ObservableTreeNode(new TestNode(111), _testNodeComparer));
            tree.AddChild(new ObservableTreeNode(new TestNode(112), _testNodeComparer));

            target.AddChild(tree);

            int[] expected = { 10, 11, 111, 112, 12 };
            AssertLinearized(expected, _linearized.ItemList, target);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void InsertChildTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(10), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(11), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(12), _testNodeComparer));
            target.Children[1].IsExpanded = true;

            target.Children[1].AddChild(new ObservableTreeNode(new TestNode(111), _testNodeComparer));

            int[] expected = { 10, 11, 111, 12 };
            AssertLinearized(expected, _linearized.ItemList, target);
        }

        [TestMethod]
        [TestCategory("Variable.Explorer")]
        public void RemoveLeafChildTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(10), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(11), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(12), _testNodeComparer));

            target.RemoveChild(1);

            int[] expected = { 10, 12 };
            AssertLinearized(expected, _linearized.ItemList, target);
        }

        #endregion manual Add/Removal Test

        #region test utilities

        private void AssertLinearized(int[] expected, IList<ObservableTreeNode> target, ObservableTreeNode targetTree) {
            Assert.AreEqual(expected.Length, target.Count);
            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i].ToString(), target[i].Model.Content, string.Format("{0}th item is different", i));
            }
        }

        private ObservableTreeNode GetTestTree() {
            var n1 = new ObservableTreeNode(new TestNode(11), _testNodeComparer);
            var n11 = new ObservableTreeNode(new TestNode(111), _testNodeComparer);
            var n12 = new ObservableTreeNode(new TestNode(112), _testNodeComparer);
            n1.AddChild(n11);
            n1.AddChild(n12);

            var n2 = new ObservableTreeNode(new TestNode(12), _testNodeComparer);
            var n21 = new ObservableTreeNode(new TestNode(121), _testNodeComparer);
            var n22 = new ObservableTreeNode(new TestNode(122), _testNodeComparer);
            n2.AddChild(n21);
            n2.AddChild(n22);

            var n3 = new ObservableTreeNode(new TestNode(13), _testNodeComparer);

            var n311 = new ObservableTreeNode(new TestNode(1311), _testNodeComparer);
            var n312 = new ObservableTreeNode(new TestNode(1312), _testNodeComparer);
            var n31 = new ObservableTreeNode(new TestNode(131), _testNodeComparer);
            n31.AddChild(n311);
            n31.AddChild(n312);

            var n32 = new ObservableTreeNode(new TestNode(132), _testNodeComparer);
            n3.AddChild(n31);
            n3.AddChild(n32);

            var n = new ObservableTreeNode(new TestNode(1), _testNodeComparer);
            n.AddChild(n1);
            n.AddChild(n2);
            n.AddChild(n3);

            return n;
        }

        #endregion

        private void Expand(int index, int childCount) {
            using (var countDown = new AddCountDownEvent(childCount, _linearized.ItemList)) {
                _linearized.ItemList[index].IsExpanded = true;
                Assert.IsTrue(countDown.Wait(TimeSpan.FromMilliseconds(1000)));
            }
        }

        class AddCountDownEvent : CountdownEvent {
            private INotifyCollectionChanged _collection;

            public AddCountDownEvent(int initialCount, ReadOnlyObservableCollection<ObservableTreeNode> collection)
                : base(initialCount) {
                _collection = collection;
                _collection.CollectionChanged += ObservableTreeNodeTest_CollectionChanged;
            }

            private void ObservableTreeNodeTest_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
                if (e.Action == NotifyCollectionChangedAction.Add) {
                    Signal();
                }
            }

            protected override void Dispose(bool disposing) {
                base.Dispose(disposing);

                if (_collection != null) {
                    _collection.CollectionChanged -= ObservableTreeNodeTest_CollectionChanged;
                    _collection = null;
                }
            }
        }

    }
}
