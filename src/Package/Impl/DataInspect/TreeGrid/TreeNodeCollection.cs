using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class TreeNodeCollection {
        private ObservableTreeNode _rootNode;
        private ObservableCollection<ObservableTreeNode> itemList;
        private ReadOnlyObservableCollection<ObservableTreeNode> readonlyList;
        private readonly bool includeRoot;
        private ObservableTreeNode excludedRootItem;

        public TreeNodeCollection(
            ObservableTreeNode rootNode,
            bool includeRoot = false) {
            _rootNode = rootNode;
            this.itemList = new ObservableCollection<ObservableTreeNode>();
            this.readonlyList = new ReadOnlyObservableCollection<ObservableTreeNode>(this.itemList);
            this.includeRoot = includeRoot;
            this.RebuildList();
        }

        public ReadOnlyObservableCollection<ObservableTreeNode> ItemList {
            get {
                return this.readonlyList;
            }
        }

        // clears the item list.
        public void ClearList() {
            this.RemoveAllHandlers();
            this.itemList.Clear();
        }

        public void RebuildList(bool overrideIncludeRoot = false) {
            this.RemoveAllHandlers();

            // Build the list from scratch - add the rootItem and its handlers, and then all the children if any.
            this.excludedRootItem = null;
            this.itemList.Clear();
            ObservableTreeNode rootItem = _rootNode;

            if (rootItem != null) {
                this.AddHandlers(rootItem);

                if (this.includeRoot && !overrideIncludeRoot) {
                    this.itemList.Add(rootItem);
                } else {
                    this.excludedRootItem = rootItem;
                }

                // if not including the root, always expand
                if (rootItem.IsExpanded || !this.includeRoot) {
                    // if we're including the root in the list, we want to start 
                    // inserting children after it (so index 1, root is in 0);
                    // if no root, start adding children at the beginning of the list
                    int index = this.includeRoot ? 1 : 0;
                    this.InsertChildrenInList(rootItem, ref index);
                }
            }
        }

        private void RemoveAllHandlers() {
            // remove all existing handlers.
            foreach (var treeItem in this.itemList) {
                this.RemoveHandlers(treeItem);
            }

            // if we built this.itemList with includeRoot==false, then the rootItem is not in the list - remove its handler explicitly.
            // Note that the provider's root item may have changed so we need to keep track of the last root item we added
            if (!this.includeRoot && (this.excludedRootItem != null)) {
                this.RemoveHandlers(this.excludedRootItem);
            }
        }

        private void InsertChildrenInList(ObservableTreeNode parentItem, ref int index) {
            foreach (var childItem in parentItem.Children) {
                if (childItem.IsVisible) {
                    this.AddHandlers(childItem);
                    this.itemList.Insert(index, childItem);
                    index++;
                    if (childItem.IsExpanded) {
                        this.InsertChildrenInList(childItem, ref index);
                    }
                }
            }
        }

        private void RemoveChildrenFromList(ObservableTreeNode parentItem) {
            int startIndex = this.itemList.IndexOf(parentItem) + 1;
            int endIndex = startIndex;
            while (endIndex < this.itemList.Count && this.itemList[endIndex].Depth > parentItem.Depth) {
                ++endIndex;
            }
            for (int i = endIndex - 1; i >= startIndex; --i) {
                this.RemoveHandlers(this.itemList[i]);
                this.itemList.RemoveAt(i);
            }
        }

        private void VirtualizingTreeItem_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsExpanded") {
                var treeItem = (ObservableTreeNode)sender;
                if (treeItem.IsExpanded) {
                    int index = this.itemList.IndexOf(treeItem) + 1;
                    this.InsertChildrenInList(treeItem, ref index);
                } else {
                    this.RemoveChildrenFromList(treeItem);
                }
            }
        }

        private void VirtualizingTreeItem_ChildrenCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            var childrenCollection = (ObservableTreeNodeCollection)sender;
            var parentItem = childrenCollection.ParentItem;

            if (parentItem.IsExpanded) {
                int index;
                switch (e.Action) {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:

                        int invisibleCount = 0;

                        for (int i = 0; i < e.NewStartingIndex; i++) {
                            if (!parentItem.Children[i].IsVisible) {
                                invisibleCount++;
                            }
                        }

                        // Compute the index of where the insertion should start
                        index = this.itemList.IndexOf(parentItem) + 1;
                        int count = 0;
                        while (index < this.itemList.Count) {
                            if (this.itemList[index].Depth <= (parentItem.Depth + 1)) {
                                count++;
                            }
                            if (count > (e.NewStartingIndex - invisibleCount)) {
                                break;
                            }
                            index++;
                        }

                        // Add the items starting at the computed start index
                        foreach (ObservableTreeNode treeItem in e.NewItems) {
                            this.AddHandlers(treeItem);
                            this.itemList.Insert(index, treeItem);
                            index++;
                            if (treeItem.IsExpanded) {
                                this.InsertChildrenInList(treeItem, ref index);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (ObservableTreeNode treeItem in e.OldItems) {
                            this.RemoveChildrenFromList(treeItem);
                            this.RemoveHandlers(treeItem);
                            this.itemList.Remove(treeItem);
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        index = this.itemList.IndexOf(parentItem) + 1;
                        this.RemoveChildrenFromList(parentItem);
                        this.InsertChildrenInList(parentItem, ref index);
                        break;
                }
            }
        }

        private void AddHandlers(ObservableTreeNode treeItem) {
            treeItem.PropertyChanged += VirtualizingTreeItem_PropertyChanged;
            treeItem.Children.CollectionChanged += VirtualizingTreeItem_ChildrenCollectionChanged;
        }

        private void RemoveHandlers(ObservableTreeNode treeItem) {
            treeItem.PropertyChanged -= VirtualizingTreeItem_PropertyChanged;
            treeItem.Children.CollectionChanged -= VirtualizingTreeItem_ChildrenCollectionChanged;
        }
    }
}
