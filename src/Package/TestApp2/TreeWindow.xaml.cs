// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.TestApp {
    public partial class TreeWindow : Window {

        private ObservableTreeNode _rootNode;
        private TreeNodeCollection _nodeCollection;

        public TreeWindow() {
            InitializeComponent();

            SetRootNode();

            SortDirection = ListSortDirection.Ascending;
            this.RootTreeGrid.Sorting += RootTreeGrid_Sorting;
        }

        private void RootTreeGrid_Sorting(object sender, DataGridSortingEventArgs e) {
            SortDirection = ListSortDirection.Ascending;
            if (e.Column.SortDirection.HasValue) {
                if (e.Column.SortDirection.Value == ListSortDirection.Ascending) {
                    SortDirection = ListSortDirection.Descending;   // toggle
                }
            }

            e.Column.SortDirection = SortDirection;

            _rootNode.Sort();
            e.Handled = true;
        }

        private void SetRootNode() {
            _rootNode = new ObservableTreeNode(
                new TestNode(null, "0"),
                Comparer<ITreeNode>.Create(Comparison));
            _nodeCollection = new TreeNodeCollection(_rootNode);

            RootTreeGrid.ItemsSource = _nodeCollection.ItemList;
        }

        private ListSortDirection SortDirection { get; set; }

        private int Comparison(ITreeNode left, ITreeNode right) {
            var leftNode = (TestNode)left;
            var rightNode = (TestNode)right;

            if (SortDirection == ListSortDirection.Ascending) {
                return TestNode.ComparisonById(leftNode, rightNode);
            } else {
                return TestNode.ComparisonById(rightNode, leftNode);
            }
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

        private TestData TestData { get { return (TestData)Content; } }

        public static int ComparisonById(TestNode left, TestNode right) {
            return string.CompareOrdinal(left.TestData.Id, right.TestData.Id);
        }

        #endregion
    }
}
