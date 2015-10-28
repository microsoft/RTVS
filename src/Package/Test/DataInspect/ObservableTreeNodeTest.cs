using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ObservableTreeNodeTest {
        private List<ObservableTreeNode> _linearized;
        private ObservableTreeNode _rootNode;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitializeTest() {
            _linearized = new List<ObservableTreeNode>();
            _rootNode = new ObservableTreeNode(new TestNode(0));
            _rootNode.CollectionChanged += Target_CollectionChanged;

            _linearized.Add(_rootNode);
        }

        [TestCleanup]
        public void CleanupTest() {
            _rootNode.CollectionChanged -= Target_CollectionChanged;

            _rootNode = null;
            _linearized = null;
        }

        #region manual Add/Removal Test

        [TestMethod]
        public void ObservableTreeNodeConstructorTest() {
            var target = new ObservableTreeNode(new TestNode(1234));
            Assert.AreEqual(false, target.HasChildren, "Default HasChildren value");
            Assert.AreEqual(1234.ToString(), target.Model.Content);
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(0, target.Children.Count);
        }

        [TestMethod]
        public void ObservableTreeNodeAddChildTest() {
            EditorShell.SetShell(TestEditorShell.Create(EditorTestCompositionCatalog.Current));
            var target = _rootNode;
            target.InsertChildAt(0, GetTestTree());

            int[] expected = { 0, 1, 11, 111, 112, 12, 121, 122, 13, 131, 1311, 1312, 132 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void ObservableTreeNodeRemoveChildTest() {
            var target = _rootNode;
            target.InsertChildAt(0, GetTestTree());

            target.Children[0].Children[2].RemoveChild(0);

            int[] expected = { 0, 1, 11, 111, 112, 12, 121, 122, 13, 132 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void AddLeafChildTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(10)));
            target.AddChild(new ObservableTreeNode(new TestNode(11)));
            target.AddChild(new ObservableTreeNode(new TestNode(12)));

            int[] expected = { 0, 10, 11, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void AddChildTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(10)));

            var added = new ObservableTreeNode(new TestNode(11));
            added.AddChild(new ObservableTreeNode(new TestNode(111)));
            added.AddChild(new ObservableTreeNode(new TestNode(112)));

            target.AddChild(added);
            target.AddChild(new ObservableTreeNode(new TestNode(12)));

            int[] expected = { 0, 10, 11, 111, 112, 12 };

            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void InsertChildOrderTest() {
            var target = _rootNode;
            target.InsertChildAt(0, new ObservableTreeNode(new TestNode(12)));
            target.InsertChildAt(0, new ObservableTreeNode(new TestNode(10)));
            target.InsertChildAt(1, new ObservableTreeNode(new TestNode(11)));

            int[] expected = { 0, 10, 11, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void InsertAnoterTreeTest() {
            var target = _rootNode;
            target.InsertChildAt(0, new ObservableTreeNode(new TestNode(12)));

            target.InsertChildAt(0, new ObservableTreeNode(new TestNode(10)));

            var added = new ObservableTreeNode(new TestNode(11));
            added.AddChild(new ObservableTreeNode(new TestNode(111)));
            added.AddChild(new ObservableTreeNode(new TestNode(112)));

            target.InsertChildAt(1, added);

            int[] expected = { 0, 10, 11, 111, 112, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void InsertChildTest() {
            var target = _rootNode;
            target.InsertChildAt(0, new ObservableTreeNode(new TestNode(10)));
            target.InsertChildAt(1, new ObservableTreeNode(new TestNode(11)));
            target.InsertChildAt(2, new ObservableTreeNode(new TestNode(12)));

            target.Children[1].InsertChildAt(0, new ObservableTreeNode(new TestNode(111)));

            int[] expected = { 0, 10, 11, 111, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void RemoveLeafChildTest() {
            var target = _rootNode;
            target.InsertChildAt(0, new ObservableTreeNode(new TestNode(10)));
            target.InsertChildAt(1, new ObservableTreeNode(new TestNode(11)));
            target.InsertChildAt(2, new ObservableTreeNode(new TestNode(12)));

            target.RemoveChild(1);

            int[] expected = { 0, 10, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        #endregion manual Add/Removal Test

        #region test utilities

        private void AssertLinearized(int[] expected, IList<ObservableTreeNode> target, ObservableTreeNode targetTree) {
            Assert.AreEqual(expected.Length, targetTree.Count);
            Assert.AreEqual(expected.Length, target.Count);
            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i].ToString(), target[i].Model.Content, string.Format("{0}th item is different", i));
            }
        }

        private void Target_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    int insertIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems) {
                        _linearized.Insert(insertIndex, (ObservableTreeNode)item);
                        insertIndex++;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _linearized.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _linearized.RemoveRange(1, _linearized.Count - 1);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    Assert.Fail("Not supported collection change detected");
                    break;
            }
        }

        private ObservableTreeNode GetTestTree() {
            var n1 = new ObservableTreeNode(new TestNode(11));
            var n11 = new ObservableTreeNode(new TestNode(111));
            var n12 = new ObservableTreeNode(new TestNode(112));
            n1.InsertChildAt(0, n11);
            n1.InsertChildAt(1, n12);

            var n2 = new ObservableTreeNode(new TestNode(12));
            var n21 = new ObservableTreeNode(new TestNode(121));
            var n22 = new ObservableTreeNode(new TestNode(122));
            n2.InsertChildAt(0, n21);
            n2.InsertChildAt(1, n22);

            var n3 = new ObservableTreeNode(new TestNode(13));

            var n311 = new ObservableTreeNode(new TestNode(1311));
            var n312 = new ObservableTreeNode(new TestNode(1312));
            var n31 = new ObservableTreeNode(new TestNode(131));
            n31.InsertChildAt(0, n311);
            n31.InsertChildAt(1, n312);

            var n32 = new ObservableTreeNode(new TestNode(132));
            n3.InsertChildAt(0, n31);
            n3.InsertChildAt(1, n32);

            var n = new ObservableTreeNode(new TestNode(1));
            n.InsertChildAt(0, n1);
            n.InsertChildAt(1, n2);
            n.InsertChildAt(2, n3);

            return n;
        }

        #endregion
    }
}
