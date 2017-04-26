// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Common.Wpf.Extensions;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class TreeGrid : DataGrid {
        public static readonly DependencyProperty IsCellFocusedProperty = DependencyProperty.RegisterAttached("IsCellFocused", typeof(bool), typeof(TreeGrid), new PropertyMetadata(false, OnIsCellFocusedChanged));
        public static readonly DependencyProperty IsFocusInRowProperty = DependencyProperty.RegisterAttached("IsFocusInRow", typeof(bool), typeof(TreeGrid), new PropertyMetadata(false));

        public static bool GetIsCellFocused(DataGridCell cell) => (bool)cell.GetValue(IsCellFocusedProperty);
        public static void SetIsCellFocused(DataGridCell cell, bool value) => cell.SetValue(IsCellFocusedProperty, value);
             
        public static bool GetIsFocusInRow(DataGridCell cell) => (bool)cell.GetValue(IsFocusInRowProperty);
        public static void SetIsFocusInRow(DataGridCell cell, bool value) => cell.SetValue(IsFocusInRowProperty, value);
             
        private static void OnIsCellFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var cellsPanel = d.GetParentOfType<DataGridCellsPanel>();
            var cells = cellsPanel.GetChildrenOfType<DataGridCell>().ToList();
            var isFocused = cells.Any(c => c.IsFocused);
            foreach (var cell in cells) {
                SetIsFocusInRow(cell, isFocused);
            }
        }

        public event EventHandler<NotifyCollectionChangedEventArgs> ItemsChanged;

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e) {
            base.OnItemsChanged(e);
            ItemsChanged?.Invoke(this, e);
        }
 
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

        public void SetCurrentItem(int index) {
            var item = Items[index];
            SetCurrentItem(item);
        }

        #endregion
    }
}
