using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Item container in <see cref="DynamicGrid"/>, and also ItemsControl for cells in a row
    /// </summary>
    internal class DynamicGridRow : ItemsControl, IScrollInfoGiver {
        private LinkedList<DynamicGridCell> _realizedCells = new LinkedList<DynamicGridCell>();

        static DynamicGridRow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGridRow), new FrameworkPropertyMetadata(typeof(DynamicGridRow)));
        }

        public DynamicGridRow() {
            Track = new LinkedListNode<DynamicGridRow>(this);
            ColumnHeader = false;
        }

        public bool ColumnHeader { get; set; }

        internal LinkedListNode<DynamicGridRow> Track { get; }

        public static readonly DependencyProperty HeaderProperty =
                DependencyProperty.Register(
                        "Header",
                        typeof(object),
                        typeof(DynamicGridRow),
                        new FrameworkPropertyMetadata(null));

        public object Header {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        internal DynamicGrid ParentGrid { get; private set; }

        #region IScrollInfoGiver support

        public SharedScrollInfo GetScrollInfo(Size size) {
            Debug.Assert(ParentGrid != null);
            return ParentGrid.GetLayoutInfo(size);
        }

        public void InvalidateScrollInfo() {
            if (ColumnHeader) {
                ParentGrid.OnInvalidateScrollInfo();
            }
        }

        public event EventHandler SharedScrollChanged;

        #endregion

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (ColumnHeader) {
                ParentGrid = WpfHelper.FindParent<DynamicGrid>(this);
                ParentGrid.ColumnHeadersPresenter = this;
            }
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new DynamicGridCell();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            var cell = (DynamicGridCell)element;
            int column = this.Items.IndexOf(item);
            if (column == -1) {
                throw new InvalidOperationException("Item is not found in collection");
            }

            if (cell.Owner != this) {
                _realizedCells.AddFirst(cell.Track);
            }

            cell.Prepare(this, ParentGrid.GetColumnWidth(column));

        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            var cell = (DynamicGridCell)element;
            if (cell.Owner == this) {
                _realizedCells.Remove(cell.Track);
            }
            cell.CleanUp();
        }

        internal void Prepare(DynamicGrid owner, object item) {
            if (!(item is IList)) {
                throw new NotSupportedException("JointCollectionGridRow supports only IList for item");
            }

            ParentGrid = owner;

            var items = (IList)item;
            ItemsSource = items;
        }

        internal void CleanUp(DynamicGrid owner, object item) {
            // when VirtualizationMode == Recycling, next lines must not be called as system calls them
            var mode = VirtualizingPanel.GetVirtualizationMode(ParentGrid);
            if (mode != VirtualizationMode.Recycling) {
                foreach (var cell in _realizedCells) {
                    cell.CleanUp();
                }
                _realizedCells.Clear();
            }

            ParentGrid = null;
        }

        internal void ScrollChanged() {
            if (SharedScrollChanged != null) {
                SharedScrollChanged(this, EventArgs.Empty);
            }
        }

        internal DynamicGridRowHeader RowHeader { get; set; }

        internal void NotifyRowHeader() {
            if (RowHeader != null) {
                RowHeader.InvalidateMeasure();
            }
        }
    }
}
