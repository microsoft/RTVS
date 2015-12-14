using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Utility;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Microsoft.VisualStudio.R.Package.DataInspect {

    public class ObservableTreeNode : BindableBase {
        #region factory


        public ObservableTreeNode(ITreeNode provider) {
            Children = new ObservableTreeNodeCollection(this);
            Model = provider;
        }

        #endregion

        #region public/protected

        public IComparer<ObservableTreeNode> Comparer { get; set; }

        private bool _hasChildren;
        /// <summary>
        /// true for non-leaf node, false for leaf node
        /// </summary>
        public bool HasChildren {
            get { return _hasChildren; }
            set { SetProperty<bool>(ref _hasChildren, value); }
        }

        private bool _isVisible = true;
        public bool IsVisible {
            get { return _isVisible; }
            set {
                SetProperty(ref _isVisible, value);
            }
        }


        private bool _isExpanded = false;
        /// <summary>
        /// Indicate this node expand to show children
        /// </summary>
        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded == value) {
                    return;
                }

                _isExpanded = value;

                if (_isExpanded) {
                    if (HasChildren) {
                        StartUpdatingChildren(Model).DoNotWait();
                    }
                }

                OnPropertyChanged("IsExpanded");
            }
        }

        private Visibility _visibility = Visibility.Visible;
        /// <summary>
        /// Visibility of this node
        /// </summary>
        public Visibility Visibility {
            get { return _visibility; }
            set {
                SetProperty<Visibility>(ref _visibility, value);
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
                    return -1;
                }
                return Parent.Depth + 1;
            }
        }

        public ObservableTreeNodeCollection Children {
            get;
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
            set {
                var newModel = value;

                if (newModel == null) {
                    RemoveChildren();
                } else {
                    if (!newModel.HasChildren) {
                        RemoveChildren();
                    }
                    HasChildren = newModel.HasChildren;
                    if (IsExpanded) {
                        StartUpdatingChildren(newModel).SilenceException<Exception>().DoNotWait();
                    }
                }

                SetProperty(ref _model, value);
            }
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
        /// add a direct child node
        /// </summary>
        /// <param name="newItem">a node to be added</param>
        public void AddChild(ObservableTreeNode newItem) {
            if (newItem == null || newItem.Parent != null) {
                throw new ArgumentException("Null tree node or a tree with non-null Parent can't be added as a child");
            }

            newItem.Parent = this;

            int insertionIndex = this.Children.BinarySearch(newItem, Comparer);
            if (insertionIndex < 0) {
                // BinarySearch returns bitwise complement if not found
                insertionIndex = ~insertionIndex;
            }
            Children.Insert(insertionIndex, newItem);
        }

        public void RemoveChild(int index) {
            var child = Children[index];
            RemoveChild(child);
        }

        /// <summary>
        /// remove a direct child node, and all children, of course
        /// </summary>
        /// <param name="index">direct child index</param>
        public void RemoveChild(ObservableTreeNode child) {
            child.RemoveChildren();
            Children.Remove(child);
            child.Parent = null;
        }

        private void RemoveChildren() {
            var toRemove = this.Children.ToList();
            foreach (var child in toRemove) {
                RemoveChild(child);
            }
        }

        #endregion

        #region private

        private async Task StartUpdatingChildren(ITreeNode model) {
            if (model == null) {
                return;
            }

            try {
                var nodes = await model.GetChildrenAsync(CancellationToken.None);
                ThreadHelper.Generic.BeginInvoke(
                    DispatcherPriority.Normal,
                    () => UpdateChildren(nodes));
            } catch (Exception e) {
                if (!(e is OperationCanceledException)) {
                    Debug.Fail(e.ToString());
                }
                SetStatus("Error while enumerating members", e);   // TODO: move to resource, or bypass the exception message
            }
        }

        private void SetStatus(string message, Exception e) {
            ErrorContent = new Exception(message, e);
        }

        /// <remarks>
        /// Assumes new data (update) and current Children are sorted in same order
        /// </remarks>
        private void UpdateChildren(IReadOnlyList<ITreeNode> update) {
            // special case of no update
            if (update == null || update.Count == 0) {
                RemoveChildren();
                return;
            }

            int srcIndex = 0;
            int updateIndex = 0;

            while (srcIndex < Children.Count) {
                int sameUpdateIndex = -1;
                for (int u = updateIndex; u < update.Count; u++) {
                    if (Children[srcIndex].Model.CanUpdateTo(update[u])) {
                        sameUpdateIndex = u;
                        break;
                    }
                }

                if (sameUpdateIndex != -1) {
                    int insertIndex = srcIndex;
                    for (int i = updateIndex; i < sameUpdateIndex; i++) {
                        var newItem = new ObservableTreeNode(update[i]);
                        newItem.Parent = this;

                        Children.Insert(insertIndex++, newItem);
                        srcIndex++;
                    }

                    Children[srcIndex].Model = update[sameUpdateIndex];
                    srcIndex++;

                    updateIndex = sameUpdateIndex + 1;
                } else {
                    RemoveChild(srcIndex);
                }
            }

            if (updateIndex < update.Count) {
                Debug.Assert(srcIndex == Children.Count);

                int insertIndex = srcIndex;
                for (int i = updateIndex; i < update.Count; i++) {
                    var newItem = new ObservableTreeNode(update[i]);
                    newItem.Parent = this;

                    Children.Insert(insertIndex++, newItem);
                }
            }
        }

        #endregion
    }

    public class ObservableTreeNodeCollection : ObservableCollection<ObservableTreeNode> {
        public ObservableTreeNodeCollection(ObservableTreeNode parent) {
            ParentItem = parent;
        }

        public ObservableTreeNode ParentItem { get; }
    }
}
