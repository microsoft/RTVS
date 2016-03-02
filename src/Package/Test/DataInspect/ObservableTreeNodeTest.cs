// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    public class ObservableTreeNodeTest {
        private readonly ObservableTreeNode _rootNode;
        private readonly TreeNodeCollection _linearized;
        private readonly Comparer<ITreeNode> _testNodeComparer;

        public ObservableTreeNodeTest() {
            _testNodeComparer = Comparer<ITreeNode>.Create(TestNode.Comparison);
            _rootNode = new ObservableTreeNode(new TestNode(0), _testNodeComparer);
            _linearized = new TreeNodeCollection(_rootNode);
        }

        #region manual Add/Removal Test

        [Test]
        [Category.Variable.Explorer]
        public void ObservableTreeNodeConstructorTest() {
            var target = new ObservableTreeNode(new TestNode(1234), _testNodeComparer);
            target.HasChildren.Should().BeFalse("It is default HasChildren value");
            target.Children.Should().BeEmpty();
            target.Model.Content.Should().Be("1234");
        }

        [Test]
        [Category.Variable.Explorer]
        public void ObservableTreeNodeAddChildTest() {
            var target = _rootNode;

            // full tree: { 0, 1, 11, 111, 112, 12, 121, 122, 13, 131, 1311, 1312, 132 }
            target.AddChild(GetTestTree());

            // only root expanded by default
            var expected = new[] { 1 };
            _linearized.ItemList.Should().Equal(expected, (a, e) => Equals(a.Model.Content, e.ToString()));

            // expand 1st at level 0
            Expand(0, 3);
            expected = new[] { 1, 11, 12, 13 };
            _linearized.ItemList.Should().Equal(expected, (a, e) => Equals(a.Model.Content, e.ToString()));

            // expand 3rd at level 1
            Expand(3, 2);
            expected = new[] { 1, 11, 12, 13, 131, 132 };
            _linearized.ItemList.Should().Equal(expected, (a, e) => Equals(a.Model.Content, e.ToString()));
        }

        [Test]
        [Category.Variable.Explorer]
        public void ObservableTreeNodeRemoveChildTest() {
            var target = _rootNode;
            target.AddChild(GetTestTree());
            Expand(0, 3);
            Expand(3, 2);
            Expand(4, 2);
            target.Children[0].Children[2].RemoveChild(0);

            int[] expected = { 1, 11, 12, 13, 132 };
            _linearized.ItemList.Should().Equal(expected, (a, e) => Equals(a.Model.Content, e.ToString()));
        }

        [Test]
        [Category.Variable.Explorer]
        public void AddChildOutOrderTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(10), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(11), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(12), _testNodeComparer));

            int[] expected = { 10, 11, 12 };
            _linearized.ItemList.Should().Equal(expected, (a, e) => Equals(a.Model.Content, e.ToString()));
        }

        [Test]
        [Category.Variable.Explorer]
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

            _linearized.ItemList.Should().Equal(expected, (a, e) => Equals(a.Model.Content, e.ToString()));
        }

        [Test]
        [Category.Variable.Explorer]
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
            _linearized.ItemList.Should().Equal(expected, (a, e) => Equals(a.Model.Content, e.ToString()));
        }

        [Test]
        [Category.Variable.Explorer]
        public void InsertChildTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(10), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(11), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(12), _testNodeComparer));
            target.Children[1].IsExpanded = true;

            target.Children[1].AddChild(new ObservableTreeNode(new TestNode(111), _testNodeComparer));

            int[] expected = { 10, 11, 111, 12 };
            _linearized.ItemList.Should().Equal(expected, (a, e) => Equals(a.Model.Content, e.ToString()));
        }

        [Test]
        [Category.Variable.Explorer]
        public void RemoveLeafChildTest() {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(new TestNode(10), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(11), _testNodeComparer));
            target.AddChild(new ObservableTreeNode(new TestNode(12), _testNodeComparer));

            target.RemoveChild(1);

            int[] expected = { 10, 12 };
            _linearized.ItemList.Should().Equal(expected, (a, e) => Equals(a.Model.Content, e.ToString()));
        }

        #endregion manual Add/Removal Test

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

        private void Expand(int index, int childCount) {
            using (var countDown = new AddCountDownEvent(childCount, _linearized.ItemList)) {
                _linearized.ItemList[index].IsExpanded = true;
                countDown.Wait(TimeSpan.FromMilliseconds(1000)).Should().BeTrue();
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
