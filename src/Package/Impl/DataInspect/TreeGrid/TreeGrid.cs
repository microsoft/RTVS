// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class TreeGrid : DataGrid {

        #region Keyboard navigation

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.Key == Key.Right) {
                // if expanded, move current to the first child
                // if not expanded, expand
                // if not expandable (has no child), no action
                var node = (ObservableTreeNode)CurrentItem;
                if (node.HasChildren) {
                    if (node.IsExpanded) {
                        int index = Items.IndexOf(node) + 1;
                        if (index < Items.Count) {
                            SetCurrentItem(index);
                        }
                    } else {
                        node.IsExpanded = true;
                    }
                }
                e.Handled = true;
            } else if (e.Key == Key.Left) {
                // if expanded, collapse
                // if not expanded, move current to parent
                var node = (ObservableTreeNode)CurrentItem;
                if (node.IsExpanded) {
                    node.IsExpanded = false;
                } else {
                    int childIndex = Items.IndexOf(node);
                    int parentIndex = GetParentNodeIndex(childIndex, node.Depth);
                    if (parentIndex >= 0) {
                        SetCurrentItem(parentIndex);
                    }
                }
                e.Handled = true;
            } else if (e.Key == Key.Home) {
                // Home moves the current to the first item like Ctrl+Home
                if (Items.Count > 0) {
                    SetCurrentItem(Items[0]);
                    e.Handled = true;
                }
            } else if (e.Key == Key.End) {
                // End moves the current to the first item like Ctrl+End
                if (Items.Count > 0) {
                    SetCurrentItem(Items[Items.Count - 1]);
                    e.Handled = true;
                }
            }

            if (!e.Handled) {
                base.OnKeyDown(e);
            }
        }

        private int GetParentNodeIndex(int startIndex, int depth) {
            for (int i = startIndex; i >= 0; i--) {
                if (((ObservableTreeNode)Items[i]).Depth < depth) {
                    return i;
                }
            }
            return -1;
        }

        private void SetCurrentItem(object item) {
            ScrollIntoView(item);
            SelectedItem = item;
            CurrentItem = item;
        }

        private void SetCurrentItem(int index) {
            var item = Items[index];
            SetCurrentItem(item);
        }

        #endregion
    }
}
