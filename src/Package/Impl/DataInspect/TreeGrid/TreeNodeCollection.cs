using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {

    /// <summary>
    /// CollectionViewSource for ObservableTreeNode, can be bound to ItemsControl.ItemsSource
    /// </summary>
    public class TreeNodeCollection : CollectionViewSource {
        private readonly TreeToListConverter _treeToList;

        public TreeNodeCollection(ObservableTreeNode rootNode) {
            this.Filter += CollectionView_Filter;

            _treeToList = new TreeToListConverter(rootNode, this);
            this.Source = _treeToList;
        }

        internal void Refresh() // TODO: Refresh is known having perf issue. 
        {
            this.View.Refresh();
        }

        private static void CollectionView_Filter(object sender, FilterEventArgs e) {
            var node = e.Item as ObservableTreeNode;
            if (node != null) {
                e.Accepted = (node.Visibility == Visibility.Visible);
            } else {
                e.Accepted = false;
            }
        }

        class TreeToListConverter : ObservableCollection<ObservableTreeNode> {
            public TreeToListConverter(ObservableTreeNode rootNode, TreeNodeCollection owner) {
                Owner = owner;

                rootNode.CollectionChanged += Root_CollectionChanged;

                if (rootNode.Depth >= 0) {
                    AddNode(rootNode);
                }
            }

            public TreeNodeCollection Owner { get; }

            void AddNode(ObservableTreeNode node) {
                node.PropertyChanged += Node_PropertyChanged;
                Add(node);
                if (node.HasChildren) {
                    foreach (var child in node.Children) {
                        AddNode(child);
                    }
                }
            }

            private void Root_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
                ThreadHelper.Generic.Invoke(() => {
                    switch (e.Action) {
                        case NotifyCollectionChangedAction.Add:
                            int insertIndex = e.NewStartingIndex;
                            foreach (var item in e.NewItems) {
                                var node = (ObservableTreeNode)item;
                                node.PropertyChanged += Node_PropertyChanged;

                                this.Insert(insertIndex, node);
                                insertIndex++;
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            int removeIndex = e.OldStartingIndex;
                            for (int i = 0; i < e.OldItems.Count; i++) {
                                this[removeIndex].PropertyChanged -= Node_PropertyChanged;
                                this.RemoveAt(removeIndex);
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                        case NotifyCollectionChangedAction.Replace:
                        case NotifyCollectionChangedAction.Move:
                        default:
                            throw new NotSupportedException();
                    }
                });
            }

            private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                if (e.PropertyName == "IsExpanded") {
                    Owner?.Refresh();
                }
            }
        }
    }
}
