using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Tree node that supposts <see cref="INotifyCollectionChanged"/>
    /// This node notifies collection change in linear order including children.
    /// </summary>
    public class ObservableTreeNode : BindableBase, INotifyCollectionChanged {
        #region factory

        /// <summary>
        /// create new instance of <see cref="ObservableTreeNode"/>, usually root node
        /// </summary>
        /// <param name="node">Model for this node</param>
        public ObservableTreeNode(ITreeNode node)
            : this(node, 0, 1) {
        }

        private ObservableTreeNode(ITreeNode node, int depthAdjust, int indexStart) {
            DepthAdjust = depthAdjust;
            IndexStart = indexStart;

            Parent = null;
            Model = node;

            ResetCount();
        }

        private int DepthAdjust;
        private int IndexStart;
        public static ObservableTreeNode CreateAsRoot(ITreeNode node, bool includeInCollection) {
            var instance = includeInCollection ? new ObservableTreeNode(node, 0, 1) : new ObservableTreeNode(node, -1, 0);

            if (!includeInCollection) {
                instance.IsExpanded = true;
            }
            instance.Visibility = Visibility.Visible;

            return instance;
        }

        #endregion

        #region public/protected

        private bool _hasChildren;
        /// <summary>
        /// true for non-leaf node, false for leaf node
        /// </summary>
        public bool HasChildren {
            get { return _hasChildren; }
            set { SetProperty<bool>(ref _hasChildren, value); }
        }


        private bool _isExpanded = false;
        /// <summary>
        /// Indicate this node expand to show children
        /// </summary>
        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (HasChildren) {
                    foreach (var child in ChildrenInternal) {
                        SetNodeVisibility(child, value);
                    }
                }

                SetProperty<bool>(ref _isExpanded, value);

                if (_isExpanded && HasChildren) {
                    foreach (var child in ChildrenInternal) // TODO: synchronize Visibility/IsExpanded change, basically Provider should be intelligent sequence and load balance update request
                    {
                        child.StartUpdatingChildren().DoNotWait();
                    }
                }
            }
        }

        private Visibility _visibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of this node
        /// </summary>
        public Visibility Visibility {
            get { return _visibility; }
            set {
                SetProperty<Visibility>(ref _visibility, value);
                if (_visibility == Visibility.Visible) {
                    StartUpdatingChildren().DoNotWait();
                }
            }
        }

        /// <summary>
        /// parent node, null if root
        /// </summary>
        protected ObservableTreeNode Parent { get; private set; }

        /// <summary>
        /// Depth from the root.
        /// </summary>
        public int Depth {
            get {
                if (Parent == null) {
                    return DepthAdjust;
                }
                return Parent.Depth + DepthAdjust + 1;
            }
        }

        List<ObservableTreeNode> _children;
        /// <summary>
        /// Direct children of this node
        /// </summary>
        public IReadOnlyList<ObservableTreeNode> Children {
            get { return ChildrenInternal; }
        }

        protected List<ObservableTreeNode> ChildrenInternal {
            get {
                if (_children == null) {
                    _children = new List<ObservableTreeNode>();
                }
                return _children;
            }
        }

        /// <summary>
        /// the number of node including all children and itself
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///  value  contained in this node
        /// </summary>
        private ITreeNode _model;
        public ITreeNode Model {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }

        private object _errorContent;
        /// <summary>
        /// Content when any error happens internally
        /// </summary>
        public object ErrorContent {
            get { return _errorContent; }
            set { SetProperty(ref _errorContent, value); }
        }

        /// <summary>
        /// Insert a direct child node at given position
        /// </summary>
        /// <param name="index">child position, relative to direct Children</param>
        /// <param name="item">tree node</param>
        public virtual void InsertChildAt(int index, ObservableTreeNode item) {
            item.Parent = this;
            SetNodeVisibility(item, this.IsExpanded);

            int addedStartingIndex = AddUpChildCount(index);

            ChildrenInternal.Insert(index, item);
            SetHasChildren();

            item.CollectionChanged += Item_CollectionChanged;

            Count += item.Count;

            if (CollectionChanged != null) {
                IList addedItems = Linearize(item);

                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        addedItems,
                        addedStartingIndex));
            }
        }

        /// <summary>
        /// add a direct child node
        /// </summary>
        /// <param name="item">a node to be added</param>
        public virtual void AddChild(ObservableTreeNode item) {
            InsertChildAt(ChildrenInternal.Count, item);
        }

        /// <summary>
        /// remove a direct child node, and all children, of course
        /// </summary>
        /// <param name="index">direct child index</param>
        public virtual void RemoveChild(int index) {
            if (!HasChildren) {
                throw new ArgumentException("No child node to remove");
            }

            int removedStartingIndex = AddUpChildCount(index);

            ObservableTreeNode toBeRemoved = Children[index];
            toBeRemoved.CollectionChanged -= Item_CollectionChanged;
            ChildrenInternal.RemoveAt(index);

            SetHasChildren();

            Count -= toBeRemoved.Count;
            Debug.Assert(Count >= IndexStart);

            if (CollectionChanged != null) {
                IList removedItems = Linearize(toBeRemoved);

                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        removedItems,
                        removedStartingIndex));
            }
        }

        /// <summary>
        /// clear direct children
        /// </summary>
        public void RemoveAllChildren() {
            int removedStartingIndex = AddUpChildCount(0);
            List<ObservableTreeNode> removedItems = new List<ObservableTreeNode>();
            foreach (var child in Children) {
                child.CollectionChanged -= Item_CollectionChanged;
                removedItems.Add(child);
            }

            ChildrenInternal.Clear();
            SetHasChildren();

            ResetCount();

            if (CollectionChanged != null) {
                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        removedItems,
                        removedStartingIndex));
            }
        }

        public async Task Update(ITreeNode node) {
            Model = node;

            await StartUpdatingChildren();
        }

        #endregion

        #region INotifyCollectionChanged

        /// <summary>
        /// notification for adding and removing child node
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region private

        private void ResetCount() {
            Count = IndexStart;
        }

        private List<ObservableTreeNode> Linearize(ObservableTreeNode tree) {
            return new List<ObservableTreeNode>(tree.TraverseDepthFirst((t) => t.HasChildren ? t.ChildrenInternal : null));
        }

        protected void Traverse(
            ObservableTreeNode tree,
            Action<ObservableTreeNode> action,
            Func<ObservableTreeNode, bool> parentPredicate) {
            action(tree);
            if (tree.HasChildren && parentPredicate(tree) && tree.Children != null) {
                foreach (var child in tree.Children) {
                    Traverse(child, action, parentPredicate);
                }
            }
        }

        private void Item_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            int nodeIndex = -1;
            var node = sender as ObservableTreeNode;
            if (node != null) {
                nodeIndex = ChildrenInternal.IndexOf(node);
            }
            if (node == null || nodeIndex == -1) {
                throw new ArgumentException("CollectionChanged is rasied with wrong sender");
            }

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    Count += e.NewItems.Count;
                    if (CollectionChanged != null) {
                        int nodeStartIndex = AddUpChildCount(nodeIndex);

                        CollectionChanged(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Add,
                                e.NewItems, e.NewStartingIndex + nodeStartIndex));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Count -= e.OldItems.Count;
                    if (CollectionChanged != null) {
                        int nodeStartIndex = AddUpChildCount(nodeIndex);

                        CollectionChanged(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                e.OldItems, e.OldStartingIndex + nodeStartIndex));
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var deleted = Linearize(node);
                    Count -= deleted.Count;
                    if (CollectionChanged != null) {
                        CollectionChanged(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                deleted,
                                nodeIndex));
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    throw new NotSupportedException("ObservableTreeNode doesn't support Replace or Move item");
            }
        }

        /// <summary>
        /// Returns all descendant nodes upto given direct child's index (open ended)
        /// </summary>
        /// <param name="nodeIndex">open ended upper boundary to which cound is added up</param>
        /// <returns>number of total children nodes</returns>
        private int AddUpChildCount(int nodeIndex) {
            int count = IndexStart;
            for (int i = 0; i < nodeIndex; i++) {
                count += Children[i].Count;
            }
            return count;
        }

        private void SetNodeVisibility(ObservableTreeNode rootNode, bool expanded) {
            Visibility visibility = expanded ? Visibility.Visible : Visibility.Collapsed;

            foreach (var node in rootNode.TraverseDepthFirst((n) => VisibleChildren(n))) {
                node.Visibility = visibility;
            }
        }

        private IEnumerable<ObservableTreeNode> VisibleChildren(ObservableTreeNode parent) {
            return (parent.IsExpanded && parent.HasChildren) ? parent.ChildrenInternal : null;
        }

        private void SetHasChildren() {
            if (_children != null && _children.Count > 0) {
                HasChildren = true;
            } else {
                HasChildren = false;
            }
        }

        private async Task StartUpdatingChildren() {
            if (Model == null) {
                return;
            }

            try {
                var nodes = await Model.GetChildrenAsync(CancellationToken.None);

                UpdateChildren(nodes);
            } catch (Exception e) {
                Debug.Assert(false, e.ToString());
                SetStatus("Errot at enumerating members", e);   // TODO: move to resource, or bypass the exception message
            }
        }

        private void SetStatus(string message, Exception e) {
            ErrorContent = new Exception(message, e);
        }

        private void UpdateChildren(IReadOnlyList<ITreeNode> update) {
            if (!HasChildren && (update == null || update.Count == 0)) {
                return; // trivial case: neither new or old has no child
            }

            int srcIndex = 0;
            int updateIndex = 0;

            while (srcIndex < ChildrenInternal.Count) {
                int sameUpdateIndex = -1;
                for (int u = updateIndex; u < update.Count; u++) {
                    if (ChildrenInternal[srcIndex].Model.CanUpdateTo(update[u])) {
                        sameUpdateIndex = u;
                        break;
                    }
                }

                if (sameUpdateIndex != -1) {
                    int insertIndex = srcIndex;
                    for (int i = updateIndex; i < sameUpdateIndex; i++) {
                        InsertChildAt(insertIndex++, new ObservableTreeNode(update[i]));
                        srcIndex++;
                    }

                    ChildrenInternal[srcIndex].Model = update[sameUpdateIndex];
                    srcIndex++;

                    updateIndex = sameUpdateIndex + 1;
                } else {
                    RemoveChild(srcIndex);
                }
            }

            if (updateIndex < update.Count) {
                Debug.Assert(srcIndex == ChildrenInternal.Count);

                int insertIndex = srcIndex;
                for (int i = updateIndex; i < update.Count; i++) {
                    InsertChildAt(insertIndex++, new ObservableTreeNode(update[i]));
                }
            }
        }

        #endregion
    }
}
